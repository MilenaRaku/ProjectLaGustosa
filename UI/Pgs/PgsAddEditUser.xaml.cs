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
    /// Логика взаимодействия для AddEditUser.xaml
    /// </summary>
    public partial class PgsAddEditUser : Page
    {
        private User _currentUser = new User();
        private Role _currentRole = new Role();

        public PgsAddEditUser( User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null) _currentUser = selectedUser;

            _currentUser = selectedUser ?? new User { IsActive = true };
            DataContext = _currentUser;
            ComBoxRole.ItemsSource = LaGustosaEntities4.GetContext().Role.ToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new PgsListUsers());
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TboxLN.Text))
                errors.AppendLine("Укажите фамилию.");
            if (string.IsNullOrWhiteSpace(TboxFN.Text))
                errors.AppendLine("Укажите имя.");
            if (string.IsNullOrWhiteSpace(TboxPatr.Text))
                errors.AppendLine("Укажите отчество.");
            if (string.IsNullOrWhiteSpace(TboxLogin.Text))
                errors.AppendLine("Укажите логин.");
            if (string.IsNullOrWhiteSpace(TboxPasw.Text))
                errors.AppendLine("Укажите пароль.");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            _currentUser.LName = TboxLN.Text;
            _currentUser.FName = TboxFN.Text;
            _currentUser.Patronymic = TboxPatr.Text;
            _currentUser.Login = TboxLogin.Text;
            _currentUser.Password = TboxPasw.Text;

            Role selectedRole = ComBoxRole.SelectedItem as Role;
            if (selectedRole != null)
            {
                _currentUser.RoleId = selectedRole.Id; 
            }

            if (_currentUser.Id == 0) 
            {
                LaGustosaEntities4.GetContext().User.Add(_currentUser);
            }
            else 
            {
                LaGustosaEntities4.GetContext().Entry(_currentUser).State = EntityState.Modified;
            }

            try
            {
                LaGustosaEntities4.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

}
        

