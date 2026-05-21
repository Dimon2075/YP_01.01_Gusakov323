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
    /// Логика взаимодействия для AuthorPage.xaml
    /// </summary>
    public partial class AuthorPage : Page
    {
        private bool _showFrozen = false;

        public AuthorPage()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            Core.ResetContext();
            int authorId = CurrentUser.User.UserId;

            var books = Core.Context.Books
                .Where(b => b.AuthorId == authorId
                         && b.IsFrozen == _showFrozen)
                .ToList()
                .Select(b => new AuthorBookViewModel
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Description = b.Description,
                    AppealVisible = _showFrozen
                        ? Visibility.Visible
                        : Visibility.Collapsed
                }).ToList();

            BooksList.ItemsSource = books;
        }

        private void BtnShowActive_Click(object sender, RoutedEventArgs e)
        {
            _showFrozen = false;
            LoadBooks();
        }

        private void BtnShowFrozen_Click(object sender, RoutedEventArgs e)
        {
            _showFrozen = true;
            LoadBooks();
        }

        private void BtnAddBook_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditBookPage(null));
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;

            if (btn?.Tag != null)
            {
                
                if (int.TryParse(btn.Tag.ToString(), out int bookId))
                {
                    NavigationService.Navigate(new AddEditBookPage(bookId));
                }
            }
        }

        private void BtnAppeal_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int bookId = (int)btn.Tag;

            bool exists = Core.Context.UnfreezeRequest.Any(ur =>
                ur.TargetBookId == bookId && ur.Status == "Обработка");

            if (exists)
            {
                MessageBox.Show("Заявка уже отправлена");
                return;
            }

            var req = new UnfreezeRequest
            {
                UserId = CurrentUser.User.UserId,
                TargetBookId = bookId,
                Reason = "Прошу разморозить книгу",
                Status = "Обработка",
                CreatedAt = DateTime.Now
            };
            Core.Context.UnfreezeRequest.Add(req);
            Core.Context.SaveChanges();
            MessageBox.Show("Заявка отправлена");
        }
    }

    public class AuthorBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Visibility AppealVisible { get; set; }
    }
}
