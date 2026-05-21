using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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


namespace УП_01._01_Gusakov323
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitWindow();
        }

        private void InitWindow()
        {
            var user = CurrentUser.User;

            TxtUserName.Text = user.DisplayName;
            TxtUserRole.Text = user.Roles?.RoleName ?? "";

            if (CurrentUser.IsAuthor)
                BtnAuthor.Visibility = Visibility.Visible;

            if (CurrentUser.IsAdmin)
                BtnAdmin.Visibility = Visibility.Visible;

            if (CurrentUser.IsFrozen)
                FreezeWarning.Visibility = Visibility.Visible;

            NavigateTo(new Pages.CatalogPage());
        }

        private void NavigateTo(Page page)
        {
            MainFrame.Navigate(page);
        }

        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Pages.CatalogPage());
        }

        private void BtnMyList_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Pages.BookListsPage());
        }

        private void BtnProfile_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Pages.ProfilePage());
        }

        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Pages.AuthorPage());
        }

        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(new Pages.AdminPage());
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.User = null;
            Core.ResetContext();

            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
