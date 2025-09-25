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
using LaGustosa.DataModel.Model;
using LaGustosa.Resourse.Class;


namespace LaGustosa.UI.Pgs
{
    /// <summary>
    /// Логика взаимодействия для PgsListUsers.xaml
    /// </summary>
    public partial class PgsListUsers : Page
    {
        public PgsListUsers()
        {
            InitializeComponent();

            DataGridUser.ItemsSource = LaGustosaEntities4.GetContext().User.ToList();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                LaGustosaEntities4.GetContext().ChangeTracker.Entries().ToList().ForEach(p => p.Reload());
                // Загружаем только активных пользователей
                DataGridUser.ItemsSource = LaGustosaEntities4.GetContext().User.Where(u => u.IsActive).ToList();
            }
        }

        private void BtEdit_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new PgsAddEditUser((sender as Button).DataContext as User));
        }
       

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            // Получаем выбранного пользователя
            var selectedUser = (sender as Button).DataContext as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить пользователя {selectedUser.LName} {selectedUser.FName}?\n" +
                               "Он не сможет войти в систему, но его данные останутся в базе.",
                               "Подтверждение удаления",
                               MessageBoxButton.YesNo,
                               MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var context = LaGustosaEntities4.GetContext();

                    // Помечаем пользователя как неактивного
                    selectedUser.IsActive = false;
                    context.Entry(selectedUser).State = EntityState.Modified;
                    context.SaveChanges();

                    // Обновляем список, показывая только активных пользователей
                    DataGridUser.ItemsSource = context.User.Where(u => u.IsActive).ToList();

                    MessageBox.Show("Пользователь успешно деактивирован!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при деактивации пользователя: {ex.Message}");
                }
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new PgsAddEditUser(null));
        }

        private void DataGridUser_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Background = new SolidColorBrush(Colors.White);
            if (e.Row.GetIndex() % 2 == 0)
            {
                e.Row.Background = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0));
            }
        }
    }
}
