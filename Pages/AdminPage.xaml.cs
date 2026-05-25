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
using Microsoft.VisualBasic;

namespace УП_01._01_Gusakov323.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadComplaints();
        }

        private void ShowOnly(StackPanel panel)
        {
            PanelComplaints.Visibility = Visibility.Collapsed;
            PanelUnfreeze.Visibility = Visibility.Collapsed;
            PanelRoles.Visibility = Visibility.Collapsed;
            PanelUsers.Visibility = Visibility.Collapsed;
            panel.Visibility = Visibility.Visible;
        }

        private void LoadComplaints()
        {
            Core.ResetContext();

            // Формируем запрос IQueryable. EF Core транслирует этот код в ОДИН оптимизированный SQL-запрос.
            var list = Core.Context.Complaints
                .Select(c => new
                {
                    ComplaintId = c.ComplaintId,
                    // СУБД сама выполнит LEFT JOIN и проверит значение на null (через COALESCE или ISNULL)
                    From = "От: " + (c.Users.Login ?? ""),
                    Target = c.BookId != null
                        ? "Книга: " + c.Books.Title
                        : "Отзыв #" + c.ReviewId,
                    Reason = c.Reason
                })
                .ToList(); // Вызов .ToList() перенесен в конец. Данные материализуются только после выборки.

            ComplaintsList.ItemsSource = list;
        }

        

        private void LoadUnfreezeRequests()
        {
            Core.ResetContext();
            var list = Core.Context.UnfreezeRequest
                .Where(ur => ur.Status == "Обработка").ToList()
                .Select(ur => new
                {
                    RequestId = ur.UnfreezeRequestId,
                    UserLogin = ur.Users?.Login ?? "",
                    Reason = ur.Reason
                }).ToList();
            UnfreezeList.ItemsSource = list;
        }

        private void LoadRoleRequests()
        {
            Core.ResetContext();
            var list = Core.Context.RoleRequests
                .Where(rr => rr.Status == "Обработка").ToList()
                .Select(rr => new
                {
                    RequestId = rr.RoleRequestid,
                    UserLogin = rr.Users?.Login ?? ""
                }).ToList();
            RoleRequestsList.ItemsSource = list;
        }

        private void LoadUsers()
        {
            Core.ResetContext();

            // Рефакторинг метода загрузки пользователей
            var list = Core.Context.Users
                .Select(u => new
                {
                    UserId = u.UserId,
                    Login = u.Login,
                    RoleName = u.Roles.RoleName ?? "",
                    // Тернарный оператор выполнится на стороне СУБД (будет сгенерирован оператор CASE WHEN)
                    FreezeLabel = u.IsFrozen == true ? "Разморозить" : "Заморозить"
                })
                .ToList(); // Запрос уходит в БД в оптимизированном виде

            UsersList.ItemsSource = list;
        }

        private void BtnTabComplaints_Click(object sender, RoutedEventArgs e)
        { ShowOnly(PanelComplaints); LoadComplaints(); }

        private void BtnTabUnfreeze_Click(object sender, RoutedEventArgs e)
        { ShowOnly(PanelUnfreeze); LoadUnfreezeRequests(); }

        private void BtnTabRoles_Click(object sender, RoutedEventArgs e)
        { ShowOnly(PanelRoles); LoadRoleRequests(); }

        private void BtnTabUsers_Click(object sender, RoutedEventArgs e)
        { ShowOnly(PanelUsers); LoadUsers(); }

        private void BtnDeleteComplaint_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var c = Core.Context.Complaints.FirstOrDefault(x => x.ComplaintId == id);
            if (c != null)
            {
                Core.Context.Complaints.Remove(c);
                Core.Context.SaveChanges();
                LoadComplaints();
            }
        }

        private void BtnApproveUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var req = Core.Context.UnfreezeRequest.FirstOrDefault(ur => ur.UnfreezeRequestId == id);
            if (req == null) return;

            req.Status = "Одобрено";
            if (req.TargetBookId != null)
            {
                var book = Core.Context.Books.FirstOrDefault(b => b.BookId == req.TargetBookId);
                if (book != null) book.IsFrozen = false;
            }
            else
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserId == req.UserId);
                if (user != null) user.IsFrozen = false;
            }
            Core.Context.SaveChanges();
            LoadUnfreezeRequests();
        }

        private void BtnRejectUnfreeze_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var req = Core.Context.UnfreezeRequest.FirstOrDefault(ur => ur.UnfreezeRequestId == id);
            if (req != null)
            {
                req.Status = "Отклонено";
                Core.Context.SaveChanges();
                LoadUnfreezeRequests();
            }
        }

        private void BtnApproveRole_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var req = Core.Context.RoleRequests.FirstOrDefault(rr => rr.RoleRequestid == id);
            if (req == null) return;

            req.Status = "Одобрено";
            var user = Core.Context.Users.FirstOrDefault(u => u.UserId == req.UserId);
            if (user != null) user.RoleId = 2;
            Core.Context.SaveChanges();
            LoadRoleRequests();
        }

        private void BtnRejectRole_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            var req = Core.Context.RoleRequests.FirstOrDefault(rr => rr.RoleRequestid == id);
            if (req != null)
            {
                req.Status = "Отклонено";
                Core.Context.SaveChanges();
                LoadRoleRequests();
            }
        }

        private void BtnToggleFreeze_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).Tag;
            if (id == CurrentUser.User.UserId)
            {
                MessageBox.Show("Нельзя заморозить собственный аккаунт.",
                    "Запрещено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var user = Core.Context.Users.FirstOrDefault(u => u.UserId == id);
            if (user != null)
            {
                user.IsFrozen = !user.IsFrozen;
                Core.Context.SaveChanges();
                LoadUsers();
            }
        }

        private void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            if (userId == CurrentUser.User.UserId)
            {
                MessageBox.Show("Нельзя изменить пароль своего аккаунта!",
                                "Запрещено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newPassword = Interaction.InputBox(
                "Введите новый пароль:",
                "Смена пароля",
                "", -1, -1);

            if (string.IsNullOrWhiteSpace(newPassword))
                return;

            var user = Core.Context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user != null)
            {
                user.PasswordHash = newPassword;
                Core.Context.SaveChanges();

                MessageBox.Show("Пароль успешно изменён!", "Успешно",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnChangeRole_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            if (userId == CurrentUser.User.UserId)
            {
                MessageBox.Show("Нельзя изменить роль своего аккаунта!",
                                "Запрещено", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var roles = Core.Context.Roles.ToList();
            string roleList = string.Join("\n", roles.Select(r => $"{r.RoleId}. {r.RoleName}"));

            string input = Interaction.InputBox(
                $"Введите ID новой роли:\n\n{roleList}",
                "Смена роли",
                "", -1, -1);

            if (string.IsNullOrWhiteSpace(input))
                return;

            if (int.TryParse(input.Trim(), out int newRoleId))
            {
                var user = Core.Context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.RoleId = newRoleId;
                    Core.Context.SaveChanges();
                    LoadUsers();

                    MessageBox.Show("Роль успешно изменена!", "Успешно",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Нужно ввести число — ID роли!", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void BtnAcceptComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                // 1. Находим текущую жалобу, которую одобрил администратор
                var complaint = Core.Context.Complaints.FirstOrDefault(x => x.ComplaintId == id);
                if (complaint != null)
                {
                    int? bookIdToDelete = complaint.BookId;
                    int? reviewIdToDelete = complaint.ReviewId;

                    // Удаляем текущую жалобу сразу, чтобы высвободить контекст
                    Core.Context.Complaints.Remove(complaint);
                    Core.Context.SaveChanges();

                    // 2. БЕЗОПАСНО УДАЛЯЕМ КОНТЕНТ И ВСЕ СВЯЗАННЫЕ С НИМ ДРУГИЕ ЖАЛОБЫ
                    if (bookIdToDelete != null)
                    {
                        // А. Удаляем ВСЕ другие жалобы, которые были поданы на отзывы этой книги
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Complaints WHERE ReviewId IN (SELECT ReviewId FROM Reviews WHERE BookId = @p0)", bookIdToDelete);

                        // Б. Удаляем ВСЕ другие жалобы, которые были поданы напрямую на саму эту книгу
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Complaints WHERE BookId = @p0", bookIdToDelete);

                        // В. Теперь отзывы этой книги полностью свободны от внешних связей — удаляем их
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Reviews WHERE BookId = @p0", bookIdToDelete);

                        // Г. Книга полностью изолирована — удаляем её
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Books WHERE BookId = @p0", bookIdToDelete);
                    }
                    else if (reviewIdToDelete != null)
                    {
                        // Если удаляем конкретный ОДИН отзыв:
                        // А. Сначала удаляем ВСЕ остальные жалобы, которые были поданы на этот же самый отзыв
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Complaints WHERE ReviewId = @p0", reviewIdToDelete);

                        // Б. Теперь отзыв ничем не связан — удаляем его
                        Core.Context.Database.ExecuteSqlCommand(
                            "DELETE FROM Reviews WHERE ReviewId = @p0", reviewIdToDelete);
                    }

                    // Полностью сбрасываем кэш контекста Entity Framework
                    Core.ResetContext();

                    MessageBox.Show("Жалоба успешно принята. Нарушающий контент и связанные дубликаты жалоб удалены.",
                                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadComplaints(); // Перерисовываем список на экране
                }
            }
        }




        /// <summary>
        /// Обработчик кнопки "Отклонить" — удаляет только саму жалобу, контент остается
        /// </summary>
        private void BtnRejectComplaint_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int id)
            {
                var complaint = Core.Context.Complaints.FirstOrDefault(x => x.ComplaintId == id);
                if (complaint != null)
                {
                    // Просто удаляем саму жалобу
                    Core.Context.Complaints.Remove(complaint);
                    Core.Context.SaveChanges();

                    MessageBox.Show("Жалоба отклонена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadComplaints(); // Обновляем список на экране
                }
            }
        }

        
    }
}
