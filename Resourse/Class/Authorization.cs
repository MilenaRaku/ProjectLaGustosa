using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LaGustosa.DataModel.Model;
using LaGustosa.UI.Pgs;
using LaGustosa.UI.Winds;


namespace LaGustosa.Resourse.Class
{
    class Authorization
    {
        public static List<User> GetUserList()
        {
            var context = new LaGustosaEntities4();
            return context.User.ToList();
        }

        public static void LoginUser(TextBox txtLogin, PasswordBox pbPassword, List<User> users, Frame mainFrame)
        {
            try
            {
                using (var context = new LaGustosaEntities4())
                {
                    if (string.IsNullOrWhiteSpace(txtLogin.Text))
                    {
                        MessageBox.Show("Введите логин!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(pbPassword.Password))
                    {
                        MessageBox.Show("Введите пароль!");
                        return;
                    }

                    var user = context.User.Include("Role")
                        .FirstOrDefault(u => u.Login == txtLogin.Text &&
                                           u.Password == pbPassword.Password);

                    if (user == null)
                    {
                        MessageBox.Show("Неправильный логин или пароль!");
                        return;
                    }

                    if (!user.IsActive)
                    {
                        MessageBox.Show("Доступ запрещен: пользователь деактивирован");
                        return;
                    }


                    MainWindow mainWnd = new MainWindow(user);
                    mainWnd.Show();
                    Window.GetWindow(mainFrame)?.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
            }
        }
    }
    
}
