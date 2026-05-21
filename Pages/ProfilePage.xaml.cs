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

namespace УП_01._01_Gusakov323.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProfilePage.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
        }

        private void LoadProfile()
        {
            var user = CurrentUser.User;

            TxtName.Text = "Имя: " + user.DisplayName;
            TxtLogin.Text = "Логин: " + user.Login;
            TxtEmail.Text = "Email: " + user.Email;
            TxtRole.Text = "Роль: " + (user.Roles?.RoleName ?? "");

            if (user.RoleId == 1)
                BtnRequestAuthor.Visibility = Visibility.Visible;

            if (CurrentUser.IsFrozen)
                FreezePanel.Visibility = Visibility.Visible;

            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.UserId == CurrentUser.User.UserId)
                .ToList()
                .Select(r => new
                {
                    BookTitle = r.Books?.Title ?? "",
                    Rating = r.Rating,
                    ReviewText = r.ReviewText
                }).ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            bool exists = Core.Context.RoleRequests.Any(rr =>
                rr.UserId == CurrentUser.User.UserId &&
                rr.Status == "Обработка");

            if (exists)
            {
                MessageBox.Show("Вы уже подали заявку, она на рассмотрении");
                return;
            }

            var request = new RoleRequests
            {
                UserId = CurrentUser.User.UserId,
                Status = "Обработка",
                CreatedAt = DateTime.Now
            };

            Core.Context.RoleRequests.Add(request);
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка отправлена на рассмотрение");
        }

        private void BtnUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            bool exists = Core.Context.UnfreezeRequest.Any(ur =>
                ur.UserId == CurrentUser.User.UserId &&
                ur.TargetBookId == null &&
                ur.Status == "Обработка");

            if (exists)
            {
                MessageBox.Show("Заявка уже отправлена, ожидайте решения");
                return;
            }

            var request = new UnfreezeRequest
            {
                UserId = CurrentUser.User.UserId,
                Reason = "Прошу разморозить аккаунт",
                Status = "Обработка",
                CreatedAt = DateTime.Now
            };

            Core.Context.UnfreezeRequest.Add(request);
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка на разморозку отправлена");
        }
    }
}
