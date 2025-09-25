using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LaGustosa.DataModel.Model;
using LaGustosa.Resourse.Class;
using Microsoft.Win32;
using OfficeOpenXml;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.Entity;
using Path = System.IO.Path;


namespace LaGustosa.UI.UC
{
    /// <summary>
    /// Логика взаимодействия для UcReports.xaml
    /// </summary>
    public partial class UcReports : UserControl
    {
        private LaGustosaEntities4 _context = new LaGustosaEntities4();

        public UcReports()
        {
            InitializeComponent();

            Loaded += UcReports_Loaded;
        }

        private void UcReports_Loaded(object sender, RoutedEventArgs e)
        {
            // Установка дат по умолчанию
            StartDatePicker.SelectedDate = DateTime.Today.AddDays(-7);
            EndDatePicker.SelectedDate = DateTime.Today;
            ReportTypeComboBox.SelectionChanged += ReportTypeComboBox_SelectionChanged;
        }

        private void ReportTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null) return;

            var selectedType = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();

            switch (selectedType)
            {
                case "shift":
                    EndDatePicker.SelectedDate = StartDatePicker.SelectedDate;
                    EndDatePicker.IsEnabled = false;
                    break;
                case "week":
                    EndDatePicker.SelectedDate = StartDatePicker.SelectedDate?.AddDays(7);
                    EndDatePicker.IsEnabled = true;
                    break;
                case "month":
                    EndDatePicker.SelectedDate = StartDatePicker.SelectedDate?.AddMonths(1);
                    EndDatePicker.IsEnabled = true;
                    break;
            }
        }

        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (StartDatePicker.SelectedDate == null || EndDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите период для формирования отчета");
                return;
            }

            try
            {
                using (var context = new LaGustosaEntities4())
                {
                    var selectedType = (ReportTypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
                    DateTime startDate = StartDatePicker.SelectedDate.Value.Date;
                    DateTime endDate = selectedType == "shift"
                        ? StartDatePicker.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1)
                        : EndDatePicker.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

                    var orders = context.Orders
                        .Include("OrderComposition.Dish")
                        .Where(o => o.OrderClosure.HasValue &&
                                   o.OrderClosure.Value >= startDate &&
                                   o.OrderClosure.Value <= endDate)
                        .ToList();

                    List<ReportItem> reportData;

                    if (selectedType == "shift")
                    {
                        reportData = new List<ReportItem>
                {
                    new ReportItem
                    {
                        Date = startDate,
                        OrdersCount = orders.Count,
                        TotalAmount = orders.Sum(o => o.Amount),
                        AverageCheck = orders.Count > 0 ? orders.Average(o => o.Amount) : 0,
                        MostPopularDish = orders.Count > 0 ? GetMostPopularDishFromOrders(orders) : "Нет заказов"
                    }
                };
                    }
                    else
                    {
                        reportData = orders
                            .GroupBy(o => o.OrderClosure.Value.Date)
                            .Select(g => new ReportItem
                            {
                                Date = g.Key,
                                OrdersCount = g.Count(),
                                TotalAmount = g.Sum(o => o.Amount),
                                AverageCheck = g.Count() > 0 ? g.Average(o => o.Amount) : 0,
                                MostPopularDish = g.Count() > 0 ? GetMostPopularDishFromOrders(g.ToList()) : "Нет заказов"
                            })
                            .OrderBy(r => r.Date)
                            .ToList();
                    }

                    ReportDataGrid.ItemsSource = reportData.Any() ? reportData : new List<ReportItem>
                    {
                        new ReportItem
                        {
                           Date = startDate,
                           OrdersCount = 0,
                           TotalAmount = 0,
                           AverageCheck = 0,
                           MostPopularDish = "Нет заказов за выбранный период"
                }
            };

                    UpdateTotals(reportData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}");
            }
        }

        private string GetMostPopularDish(IGrouping<DateTime, Orders> ordersGroup)
        {
            return GetMostPopularDishFromOrders(ordersGroup.ToList());
        }

        private string GetMostPopularDishFromOrders(List<Orders> orders)
        {
            try
            {
                var dishCounts = orders
                    .SelectMany(o => o.OrderComposition)
                    .GroupBy(oc => oc.Dish.Name)
                    .Select(g => new { DishName = g.Key, Count = g.Sum(oc => oc.NumberPositions) })
                    .OrderByDescending(x => x.Count)
                    .FirstOrDefault();

                return dishCounts != null ? $"{dishCounts.DishName} ({dishCounts.Count} шт.)" : "Нет данных";
            }
            catch
            {
                return "Нет данных";
            }
        }

        private void UpdateTotals(List<ReportItem> reports)
        {
            if (reports == null || reports.Count == 0 || reports.All(r => r.OrdersCount == 0))
            {
                TotalOrdersText.Text = "Заказов: 0";
                TotalAmountText.Text = "Выручка: 0 руб.";
                AverageCheckText.Text = "Средний чек: 0 руб.";
                return;
            }

            var totalOrders = reports.Sum(r => r.OrdersCount);
            var totalAmount = reports.Sum(r => r.TotalAmount);
            var averageCheck = totalOrders > 0 ? totalAmount / totalOrders : 0;

            TotalOrdersText.Text = $"Заказов: {totalOrders}";
            TotalAmountText.Text = $"Выручка: {totalAmount:N2} руб.";
            AverageCheckText.Text = $"Средний чек: {averageCheck:N2} руб.";
        }

    }
}

