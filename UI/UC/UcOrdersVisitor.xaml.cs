using LaGustosa.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

namespace LaGustosa.UI.UC
{
    /// <summary>
    /// Логика взаимодействия для UcOrdersVisitor.xaml
    /// </summary>
    public partial class UcOrdersVisitor : UserControl
    {
        private List<Orders> _allOrders;
        private int? _currentTableNumber = null; 

        public UcOrdersVisitor(int? tableNumber = null)
        {
            InitializeComponent();
            _currentTableNumber = tableNumber;
            LoadData();

            DataContext = this;
        }

        private void CombBoxStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        private void FilterOrders()
        {
            if (_allOrders == null) return;

            var filteredOrders = _allOrders.AsEnumerable();

            // Фильтрация по статусу кухни
            if (CombBoxStatus.SelectedItem is KitchenStatus selectedStatus && selectedStatus.Id != 0)
            {
                filteredOrders = filteredOrders.Where(o => o.KitchenStatusId == selectedStatus.Id);
            }

            OrdersList.ItemsSource = filteredOrders.ToList();
            NoOrdersText.Visibility = filteredOrders.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        // Метод для обновления списка заказов (можно вызвать извне при необходимости)
        public void RefreshOrders()
        {
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                using (var context = new LaGustosaEntities4())
                {
                    // Загружаем статусы кухни для фильтрации
                    var allStatuses = context.KitchenStatus.ToList();
                    allStatuses.Insert(0, new KitchenStatus { Id = 0, Status = "Все статусы" });
                    CombBoxStatus.ItemsSource = allStatuses;
                    CombBoxStatus.SelectedIndex = 0;

                    // Загружаем заказы с связанными данными
                    var query = context.Orders.AsQueryable()
                        .Include(o => o.Tables)
                        .Include(o => o.KitchenStatus)
                        .Include("OrderComposition.Dish") // Альтернативный синтаксис для Include
                        .Where(o => o.OrderClosure == null); // Только открытые заказы

                    // Если известен номер стола, фильтруем по нему
                    if (_currentTableNumber.HasValue)
                    {
                        query = query.Where(o => o.Tables.Number == _currentTableNumber.Value);
                    }

                    _allOrders = query.OrderByDescending(o => o.OrderOpenings).ToList();

                    // Показываем/скрываем сообщение о пустом списке
                    NoOrdersText.Visibility = _allOrders.Any() ? Visibility.Collapsed : Visibility.Visible;

                    OrdersList.ItemsSource = _allOrders;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
