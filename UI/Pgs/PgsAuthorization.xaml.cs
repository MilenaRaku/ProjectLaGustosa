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

namespace LaGustosa.UI.Pgs
{
    /// <summary>
    /// Логика взаимодействия для PgsAuthorization.xaml
    /// </summary>
    public partial class PgsAuthorization : Page
    {
        List<User> users = LaGustosaEntities4.GetContext().User.ToList();
        private bool _isPwdShown = false;

        public PgsAuthorization()
        {
            InitializeComponent();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new PgsCreatingOrder());
        }

        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            Authorization.LoginUser(TbLog, PbPasword, users, Menager.MainFrame);
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPwdShown = !_isPwdShown;
            if (_isPwdShown)
            {

                TbPassword.Visibility = Visibility.Visible;
                PbPasword.Visibility = Visibility.Hidden;
                TbPassword.Text = PbPasword.Password;
                return;

            }

            PbPasword.Visibility = Visibility.Visible;
            TbPassword.Visibility = Visibility.Hidden;
            PbPasword.Password = TbPassword.Text;
        }
    }
}
