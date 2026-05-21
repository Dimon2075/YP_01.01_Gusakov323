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

namespace УП_01._01_Gusakov323
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnTabLogin_Click(object sender, RoutedEventArgs e)
        {
            PanelLogin.Visibility = Visibility.Visible;
            PanelRegister.Visibility = Visibility.Collapsed;
            TxtError.Visibility = Visibility.Collapsed;
        }

        private void BtnTabRegister_Click(object sender, RoutedEventArgs e)
        {
            PanelLogin.Visibility = Visibility.Collapsed;
            PanelRegister.Visibility = Visibility.Visible;
            TxtError.Visibility = Visibility.Collapsed;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLoginLogin.Text.Trim();
            string password = TxtLoginPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните все поля");
                return;
            }

            var user = Core.Context.Users
                .FirstOrDefault(u => u.Login == login && u.PasswordHash == password);

            if (user == null)
            {
                ShowError("Неверный логин или пароль");
                return;
            }

            CurrentUser.User = user;

            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtRegLogin.Text.Trim();
            string name = TxtRegName.Text.Trim();
            string email = TxtRegEmail.Text.Trim();
            string password = TxtRegPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(name) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните все поля");
                return;
            }
            if (!email.Contains("@"))
            {
                ShowError("Введите корректный email адрес");
                return;
            }

            if (Core.Context.Users.Any(u => u.Login == login))
            {
                ShowError("Такой логин уже занят");
                return;
            }

            if (Core.Context.Users.Any(u => u.Email == email))
            {
                ShowError("Такой email уже занят");
                return;
            }

            var newUser = new Users
            {
                Login = login,
                DisplayName = name,
                Email = email,
                PasswordHash = password,
                RoleId = 1,
                IsFrozen = false,
                CreatedAt = DateTime.Now
            };

            Core.Context.Users.Add(newUser);
            Core.Context.SaveChanges();

            MessageBox.Show("Регистрация прошла успешно! Теперь войдите.",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            BtnTabLogin_Click(null, null);
        }

        private void ShowError(string message)
        {
            TxtError.Text = message;
            TxtError.Visibility = Visibility.Visible;
        }
    }
}
