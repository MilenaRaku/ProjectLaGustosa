using LaGustosa.DataModel.Model;
using LaGustosa.Resourse.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace LaGustosa.UI.UC
{
    public partial class UcOrdersChef : UserControl
    {
        public ObservableCollection<KitchenStatus> AllStatuses { get; set; }
        private List<Orders> _allOrders;

        public UcOrdersChef()
        {
            InitializeComponent();
            LoadData();
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

            // Фильтр по поиску
            if (!string.IsNullOrEmpty(TBoxSearch.Text.Trim()))
            {
                string searchText = TBoxSearch.Text.Trim();
                filteredOrders = filteredOrders.Where(o => o.Number.ToString().Contains(searchText));
            }

            OrdersList.ItemsSource = filteredOrders.ToList();
        }

        private void ComboStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            if (comboBox?.SelectedItem == null) return;

            var selectedStatus = comboBox.SelectedItem as KitchenStatus;
            var order = comboBox.DataContext as Orders;

            if (order != null && selectedStatus != null && order.OrderClosure == null)
            {
                try
                {
                    using (var context = new LaGustosaEntities4())
                    {
                        var orderToUpdate = context.Orders.Find(order.Number);
                        if (orderToUpdate != null)
                        {
                            orderToUpdate.KitchenStatusId = selectedStatus.Id;
                            context.SaveChanges();
                            order.KitchenStatusId = selectedStatus.Id;
                            order.KitchenStatus = selectedStatus;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении статуса: {ex.Message}");
                }
            }
        }

        private void CombBoxOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateOrders();
        }
    }
}