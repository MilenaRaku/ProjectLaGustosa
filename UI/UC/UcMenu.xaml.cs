using LaGustosa.DataModel.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Globalization;
using System.Diagnostics;
using LaGustosa.Resourse.Class;
using System.Collections.ObjectModel;

namespace LaGustosa.UI.UC
{
    public partial class UcMenu : UserControl
    {
        private LaGustosaEntities4 _context;
        private List<OrderItem> _currentOrderItems = new List<OrderItem>();
        private Tables _selectedTable;


        public UcMenu()
        {
            InitializeComponent();
            Loaded += UcMenu_Loaded;
        }

        private void UcMenu_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _context = new LaGustosaEntities4();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void LoadData()
        {
            try
            {
                // Загрузка блюд
                var dishes = _context.Dish.Include("DishCategory")
                                  .Where(d => d.Visibility)
                                  .ToList();
                DishesList.ItemsSource = dishes;

                // Загрузка категорий и добавление пункта "Все блюда"
                var categories = _context.DishCategory.ToList();
                var allCategories = new List<DishCategory>();

                // Создаем фиктивную категорию для "Всех блюд"
                allCategories.Add(new DishCategory { Id = -1, Category = "Все блюда" });
                allCategories.AddRange(categories);

                CombBoxMenu.ItemsSource = allCategories;
                CombBoxMenu.SelectedIndex = 0; // Выбираем "Все блюда" по умолчанию

                // Загрузка столов
                var tables = _context.Tables.ToList();
                TablesComboBox.ItemsSource = tables;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void BtnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var quantityTextBlock = FindVisualChild<TextBlock>(button.Parent as DependencyObject, "QuantityTextBlock");
            if (quantityTextBlock != null)
            {
                int currentQuantity;
                if (int.TryParse(quantityTextBlock.Text, out currentQuantity))
                {
                    quantityTextBlock.Text = (currentQuantity + 1).ToString();
                }
            }
        }

        private void BtnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var quantityTextBlock = FindVisualChild<TextBlock>(button.Parent as DependencyObject, "QuantityTextBlock");
            if (quantityTextBlock != null)
            {
                int currentQuantity;
                if (int.TryParse(quantityTextBlock.Text, out currentQuantity) && currentQuantity > 0)
                {
                    quantityTextBlock.Text = (currentQuantity - 1).ToString();
                }
            }
        }

        private void BtnAddToOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTable == null)
            {
                MessageBox.Show("Пожалуйста, выберите стол перед добавлением блюд в заказ.");
                return;
            }

            var button = sender as Button;
            if (button == null) return;

            var dish = button.DataContext as Dish;
            if (dish == null) return;

            var quantityTextBlock = FindVisualChild<TextBlock>(button.Parent as DependencyObject, "QuantityTextBlock");
            if (quantityTextBlock == null) return;

            int quantity;
            if (!int.TryParse(quantityTextBlock.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Количество порций должно быть больше нуля.");
                return;
            }

            AddDishToOrder(dish, quantity);
            quantityTextBlock.Text = "0"; 
        }

        private void AddDishToOrder(Dish dish, int quantity)
        {
            var existingItem = _currentOrderItems.FirstOrDefault(item => item.Dish.Id == dish.Id);

            if (existingItem != null)
            {
                existingItem.NumberPositions += quantity;
            }
            else
            {
                _currentOrderItems.Add(new OrderItem
                {
                    Dish = dish,
                    NumberPositions = quantity
                });
            }

            RefreshOrderList();
        }

        private void ChangeQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var orderItem = button.Tag as OrderItem;
            if (orderItem == null) return;

            if (button.Content.ToString() == "+")
            {
                orderItem.NumberPositions++;
            }
            else if (button.Content.ToString() == "-" && orderItem.NumberPositions > 1)
            {
                orderItem.NumberPositions--;
            }

            RefreshOrderList();
            UpdateTotalAmount();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var orderItem = button.Tag as OrderItem;
            if (orderItem == null) return;

            _currentOrderItems.Remove(orderItem);
            RefreshOrderList();
        }

        private void RefreshOrderList()
        {
            OrderItemsList.ItemsSource = null;
            OrderItemsList.ItemsSource = _currentOrderItems;
            UpdateTotalAmount();
        }

        private void UpdateTotalAmount()
        {
            decimal totalAmount = _currentOrderItems.Sum(item => item.Dish.Price * item.NumberPositions);
            TotalAmountText.Text = $"Итого: {totalAmount:#,##0.00} руб.";
        }

        private void TablesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTable = TablesComboBox.SelectedItem as Tables;
            if (_selectedTable != null)
            {
                // Сохраняем номер стола в Menager при выборе
                Menager.CurrentTableNumber = _selectedTable.Number;
            }
        }

        private void SubmitOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTable == null)
            {
                MessageBox.Show("Пожалуйста, выберите стол перед оформлением заказа.");
                return;
            }

            if (_currentOrderItems.Count == 0)
            {
                MessageBox.Show("Заказ не может быть пустым.");
                return;
            }

            try
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    try
                    {
                        var newOrder = new Orders
                        {
                            TableId = _selectedTable.Number, 
                            OrderOpenings = DateTime.Now,
                            Amount = _currentOrderItems.Sum(item => item.Dish.Price * item.NumberPositions),
                            KitchenStatusId = 1
                        };

                        _context.Orders.Add(newOrder);
                        _context.SaveChanges();

                        foreach (var item in _currentOrderItems)
                        {
                            _context.OrderComposition.Add(new OrderComposition
                            {
                                OrderId = newOrder.Number,
                                DishId = item.Dish.Id,
                                NumberPositions = item.NumberPositions
                            });
                        }

                        _context.SaveChanges();
                        transaction.Commit();

                        MessageBox.Show($"Заказ №{newOrder.Number} успешно оформлен!");

                        _currentOrderItems.Clear();
                        RefreshOrderList();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }

        private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T && ((FrameworkElement)child).Name == childName)
                {
                    return (T)child;
                }

                var result = FindVisualChild<T>(child, childName);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (_context == null)
                {
                    MessageBox.Show("Контекст данных не инициализирован");
                    return;
                }

                string searchText = TBoxSearch?.Text?.ToLower() ?? string.Empty;

                // Получаем базовый список в зависимости от выбранной категории
                IQueryable<Dish> baseQuery = _context.Dish.Where(d => d.Visibility);

                var selectedCategory = CombBoxMenu.SelectedItem as DishCategory;
                if (selectedCategory != null && selectedCategory.Id != -1) // Если выбрана конкретная категория
                {
                    baseQuery = baseQuery.Where(d => d.CategoryId == selectedCategory.Id);
                }

                var dishes = baseQuery.ToList();

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    dishes = dishes
                        .Where(d =>
                            (d.Name != null && d.Name.ToLower().Contains(searchText)) ||
                            (d.Description != null && d.Description.ToLower().Contains(searchText)))
                        .ToList();
                }

                DishesList.ItemsSource = dishes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске блюд: {ex.Message}");
                DishesList.ItemsSource = new List<Dish>();
            }
        }

        private void CombBoxMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = CombBoxMenu.SelectedItem as DishCategory;
            if (selectedItem == null) return;

            if (selectedItem.Id == -1) // Это наш пункт "Все блюда"
            {
                // Показываем все видимые блюда
                var dishes = _context.Dish
                            .Where(d => d.Visibility)
                            .ToList();
                DishesList.ItemsSource = dishes;
            }
            else
            {
                // Показываем блюда выбранной категории
                var dishes = _context.Dish
                            .Where(d => d.CategoryId == selectedItem.Id && d.Visibility)
                            .ToList();
                DishesList.ItemsSource = dishes;
            }
        }
    }

    public class OrderItem
    {
        public Dish Dish { get; set; }
        public int NumberPositions { get; set; }
    }
}