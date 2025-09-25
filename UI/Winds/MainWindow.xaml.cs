using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using LaGustosa.DataModel.Model;
using LaGustosa.Resourse.Class;
using LaGustosa.UI.Pgs;
using LaGustosa.UI.UC;

namespace LaGustosa.UI.Winds
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private User currentUser;
        private int currentOrderId;
        private int? _currentTableNumber;


        public MainWindow(User user)
        {
            InitializeComponent();
            currentUser = user;

            if (currentUser != null)
            {
                AccessControl(currentUser);
            }
            else
            {
                HideUserInfo();
            }

            StartTimer();
            Menager.MainFrame = MainFrame;

        }


        private void StartTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); 
            timer.Tick += Timer_Tick; 
            timer.Start(); 
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            TimeTextBlock.Text = DateTime.Now.ToString("HH:mm"); 
        }

        private void ImgClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }


        private void RbMenu_Click(object sender, RoutedEventArgs e)
        {
            int currentOrderId = GetCurrentOrderId(); // Получите текущий ID заказа каким-то образом

            MainFrame.Navigate(new UcMenu()); // Передаем ID заказа в UcMenu
        }

        private void RbExit_Click(object sender, RoutedEventArgs e)
        {
            CreatingOrder creatingOrder = new CreatingOrder();
            creatingOrder.Show();
            this.Close();
        }

        private void HideUserInfo()
        {
            FullNameTextBlock.Visibility = Visibility.Collapsed;
            RoleTextBlock.Visibility = Visibility.Collapsed;

            // Скрытие элементов интерфейса для неавторизованных пользователей
            RbUser.Visibility = Visibility.Collapsed;
            RbReports.Visibility = Visibility.Collapsed;
            RbMenuManagement.Visibility = Visibility.Collapsed;
            RbOrderChef.Visibility = Visibility.Collapsed;
            RbOrder.Visibility = Visibility.Collapsed;

        }

        private void AccessControl(User user)
        {
            RbOrderVisitor.Visibility = Visibility.Collapsed;
            RbUser.Visibility = Visibility.Collapsed;
            RbReports.Visibility = Visibility.Collapsed;
            RbMenuManagement.Visibility = Visibility.Collapsed;
            RbOrderChef.Visibility = Visibility.Collapsed;

            FullNameTextBlock.Text = $"{user.LName} {user.FName} {user.Patronymic}";
            RoleTextBlock.Text = user.Role != null ? user.Role.Role1 : " ";

            Debug.WriteLine($"User ID: {user.Id}");

            switch (user.Id)
            {
                case 1: // Администратор
                    RbMenu.Visibility = Visibility.Visible; 
                    RbOrder.Visibility = Visibility.Visible; 
                    RbUser.Visibility = Visibility.Visible; 
                    RbReports.Visibility = Visibility.Visible;
                    RbMenuManagement.Visibility = Visibility.Visible; 

                    break;

                case 3: // Официант
                    RbMenu.Visibility = Visibility.Visible;
                    RbOrder.Visibility = Visibility.Visible;
                    break;

                case 4: // Повар
                    RbMenu.Visibility = Visibility.Visible;
                    RbOrderChef.Visibility = Visibility.Visible;
                    RbOrder.Visibility = Visibility.Collapsed;
                    break;

                default:
                    break;
            }
        }

        private int GetCurrentOrderId()
        {
            return currentOrderId; 
        }
        
        private void RbUser_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PgsListUsers());
        }

        private void RbMenuManagement_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PgsListDish());
        }

        private void RbOrderChef_Click(object sender, RoutedEventArgs e)
        {
                MainFrame.Navigate(new UcOrdersChef());
        }

        private void RbOrderVisitor_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new UcOrdersVisitor(Menager.CurrentTableNumber));
        }

        private void RbOrder_Click(object sender, RoutedEventArgs e)
        {
            int userId = currentUser?.Id ?? 0; 

            Menager.MainFrame.Navigate(new ucOrders(userId));
        }

        private void RbReports_Click(object sender, RoutedEventArgs e)
        {
            Menager.MainFrame.Navigate(new UcReports());
        }
    }
}
