using LaGustosa.DataModel.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
using Microsoft.Win32;
using System.Data.Entity;
using System.Collections.ObjectModel;

namespace LaGustosa.UI.Pgs
{
    /// <summary>
    /// Логика взаимодействия для PgsAddEditDish.xaml
    /// </summary>
    public partial class PgsAddEditDish : Page
    {
        private Dish _dish;
        private LaGustosaEntities4 _context;
        private bool _isEditMode;

        public PgsAddEditDish(LaGustosaEntities4 context)
        {
            InitializeComponent();
            _context = context;
            _dish = new Dish();
            _dish.Visibility = true; 
            _isEditMode = false;
            DataContext = _dish;
            ComboBoxCatehory.ItemsSource = LaGustosaEntities4.GetContext().DishCategory.ToList();

        }

        // Конструктор для редактирования существующего блюда
        public PgsAddEditDish(Dish dish, LaGustosaEntities4 context)
        {
            InitializeComponent();
            _context = context;
            _dish = dish;
            _isEditMode = true;
            DataContext = _dish;

            ComboBoxCatehory.ItemsSource = LaGustosaEntities4.GetContext().DishCategory.ToList();
        }
        private void LoadCategories()
        {
            try
            {
                _context.DishCategory.Load();
                Category = new ObservableCollection<DishCategory>(_context.DishCategory.Local);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
                Category = new ObservableCollection<DishCategory>();
            }
        }

        public ObservableCollection<DishCategory> Category { get; set; }

    

    private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение блюда"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var image = new BitmapImage(new Uri(openFileDialog.FileName));
                    DishImage.Source = image;

                    using (var stream = new System.IO.MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(stream);
                        _dish.Image = stream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_dish.Name))
            {
                MessageBox.Show("Введите название блюда");
                return;
            }

            try
            {
                if (!_isEditMode)
                {
                    _context.Dish.Add(_dish);
                }

                _context.SaveChanges();
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
