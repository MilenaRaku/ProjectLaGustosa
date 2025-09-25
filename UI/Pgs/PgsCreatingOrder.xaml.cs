using LaGustosa.UI.Winds;
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
using LaGustosa.Resourse.Class;
using LaGustosa.DataModel.Model;

namespace LaGustosa.UI.Pgs
{
    /// <summary>
    /// Логика взаимодействия для PgsCreatingOrder.xaml
    /// </summary>
    public partial class PgsCreatingOrder : Page
    {
        public PgsCreatingOrder()
        {
            InitializeComponent();

        }

        private void HlCreateOrder_Click(object sender, RoutedEventArgs e)
        {
           
                MainWindow mainWindow = new MainWindow(null);
                mainWindow.Show();
                Window.GetWindow(this)?.Close();
        }

        private void HlAutorizating_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new PgsAuthorization());
        }
    }
}
