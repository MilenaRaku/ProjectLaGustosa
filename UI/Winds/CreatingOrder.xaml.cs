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
using System.Windows.Shapes;
using LaGustosa.Resourse.Class;
using LaGustosa.UI.Pgs;

namespace LaGustosa.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для CreatingOrder.xaml
    /// </summary>
    public partial class CreatingOrder : Window
    {
        public CreatingOrder()
        {
            InitializeComponent();

            Menager.MainFrame = MainFrame;
            MainFrame.Navigate(new PgsCreatingOrder());
        }
    }
}
