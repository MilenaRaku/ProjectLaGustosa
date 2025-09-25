using LaGustosa.DataModel.Model;
using LaGustosa.Resourse.Class;
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
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace LaGustosa.UI.Pgs
{
    /// <summary>
    /// Логика взаимодействия для PgsListDish.xaml
    /// </summary>
    public partial class PgsListDish : Page
    {
        private LaGustosaEntities4 _context = new LaGustosaEntities4();


        public PgsListDish()
        {
            InitializeComponent();

            Loaded += PgsListDish_Loaded;
           //!! //DataGridDish.ItemsSource = LaGustosaEntities4.GetContext().Dish.ToList();
        }

        private void PgsListDish_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDishes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private void LoadDishes()
        {
            try
            {
                _context.Dish
                    .Include(d => d.DishCategory)
                    .Load();

                DataGridDish.ItemsSource = new ObservableCollection<Dish>(_context.Dish.Local);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке блюд: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var searchText = SearchBox.Text.ToLower();
                var filtered = _context.Dish.Local
                    .Where(d => string.IsNullOrEmpty(searchText) ||
                               d.Name.ToLower().Contains(searchText) ||
                               (d.Description != null && d.Description.ToLower().Contains(searchText)) ||
                               (d.DishCategory != null && d.DishCategory.Category.ToLower().Contains(searchText)))
                    .ToList();

                DataGridDish.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}");
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                LaGustosaEntities4.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                DataGridDish.ItemsSource = LaGustosaEntities4.GetContext().Dish.ToList();
            }
        }

        private void BtEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var dish = button?.DataContext as Dish;

            if (dish != null)
            {
                // Создаем новую страницу редактирования
                var editPage = new PgsAddEditDish(dish, _context);

                // Переходим на страницу редактирования
                NavigationService.Navigate(editPage);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем выбранное блюдо из DataGrid
                var selectedDish = DataGridDish.SelectedItem as Dish;

                if (selectedDish == null)
                {
                    MessageBox.Show("Пожалуйста, выберите блюдо для удаления");
                    return;
                }

                // Подтверждение удаления
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить блюдо '{selectedDish.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Проверяем, есть ли связанные записи в OrderComposition
                    bool hasOrders = _context.OrderComposition.Any(oc => oc.DishId == selectedDish.Id);

                    if (hasOrders)
                    {
                        MessageBox.Show("Невозможно удалить блюдо, так как оно есть в существующих заказах");
                        return;
                    }

                    // Удаляем блюдо
                    _context.Dish.Remove(selectedDish);
                    _context.SaveChanges();

                    // Обновляем список
                    LoadDishes();

                    MessageBox.Show("Блюдо успешно удалено");
                }
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка базы данных при удалении: {dbEx.InnerException?.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Создаем новую страницу добавления
            var addPage = new PgsAddEditDish(_context);

            // Переходим на страницу добавления
            NavigationService.Navigate(addPage);
        }
        private void DataGridDish_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Background = new SolidColorBrush(Colors.White);
            if (e.Row.GetIndex() % 2 == 0)
            {
                e.Row.Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            }
        }

      /*  protected override void OnUnloaded(RoutedEventArgs e)
        {
            _context?.Dispose();
            base.OnUnloaded(e);
        }*/
    }
}
