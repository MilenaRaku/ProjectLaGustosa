using LaGustosa.DataModel.Model;
using System;
using System.Collections.Generic;
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
using System.Data.Entity;
using System.Collections.ObjectModel;
using LaGustosa.Resourse.Class;
using LaGustosa.UI.Winds;

namespace LaGustosa.UI.UC
{
    /// <summary>
    /// Логика взаимодействия для ucOrders.xaml
    /// </summary>
    public partial class ucOrders : UserControl
    {
        private int _currentUserId;
        private int _currentUserRoleId;
        public bool IsWaiterOrAdmin => _currentUserRoleId == 3 || _currentUserRoleId == 1;

        public ObservableCollection<KitchenStatus> AllStatuses { get; set; }
        private List<Orders> _allOrders;


        public ucOrders(int userId)
        {
            InitializeComponent();
            LoadData();

            _currentUserId = userId;


            DataContext = this;
        }

        private void LoadData()
        {
            using (var context = new LaGustosaEntities4())
            {
                AllStatuses = new ObservableCollection<KitchenStatus>(context.KitchenStatus.ToList());
                _allOrders = context.Orders
                    .Include(o => o.Tables)
                    .Include(o => o.KitchenStatus)
                    .Include(o => o.User)
                    .Include(o => o.OrderComposition.Select(oc => oc.Dish))
                    .ToList();

                var statusList = new List<KitchenStatus>(AllStatuses);
                statusList.Insert(0, new KitchenStatus { Id = -1, Status = "Все заказы" });
                CombBoxStatus.ItemsSource = statusList;
                CombBoxStatus.SelectedIndex = 0;

                UpdateOrders();
            }
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateOrders();
        }

        private void DatePickerSearch_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }

        private void UpdateOrders()
        {
            IEnumerable<Orders> filteredOrders = _allOrders;

            // Фильтр по открытости/закрытости заказа
            if (CombBoxOrder.SelectedItem is ComboBoxItem selectedOrderFilter)
            {
                string filterType = selectedOrderFilter.Tag?.ToString();

                if (filterType == "Open")
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderClosure == null);
                }
                else if (filterType == "Closed")
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderClosure != null);
                }
            }

            // Фильтр по статусу кухни
            if (CombBoxStatus.SelectedItem is KitchenStatus selectedStatus && selectedStatus.Id != -1)
            {
                filteredOrders = filteredOrders.Where(o => o.KitchenStatusId == selectedStatus.Id);
            }

            // Фильтр по поиску (номер или дата)
            if (!string.IsNullOrEmpty(TBoxSearch.Text.Trim()))
            {
                string searchText = TBoxSearch.Text.Trim();
                filteredOrders = filteredOrders.Where(o =>
                    o.Number.ToString().Contains(searchText));
            }

            // Фильтр по дате
            if (DatePickerSearch.SelectedDate.HasValue)
            {
                DateTime selectedDate = DatePickerSearch.SelectedDate.Value.Date;
                filteredOrders = filteredOrders.Where(o =>
                    (o.OrderOpenings.Date == selectedDate) ||
                    (o.OrderClosure.HasValue && o.OrderClosure.Value.Date == selectedDate));
            }

            OrdersList.ItemsSource = filteredOrders.ToList();
        }

        

        private void ComboStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }



        private void CloseOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var order = button.DataContext as Orders;
            if (order == null) return;

            if (MessageBox.Show("Вы уверены, что хотите закрыть этот заказ?",
                               "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes)
            {
                return;
            }

            try
            { 
                using (var context = new LaGustosaEntities4())
                {
                    var orderToClose = context.Orders
                        .Include(o => o.User)
                        .Include(o => o.KitchenStatus) 
                        .FirstOrDefault(o => o.Number == order.Number);

                    if (orderToClose == null) return;

                    if (orderToClose.KitchenStatusId != 3)
                    {
                        MessageBox.Show("Заказ не готов. Закрытие невозможно.",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    orderToClose.OrderClosure = DateTime.Now;
                    orderToClose.UserId = _currentUserId;

                    context.SaveChanges();

                    order.OrderClosure = orderToClose.OrderClosure;
                    order.UserId = orderToClose.UserId;
                    order.User = orderToClose.User;

                    LoadData();

                    MessageBox.Show("Заказ успешно закрыт.", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при закрытии заказа:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CombBoxOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }
    }
}
