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
    /// Логика взаимодействия для BookPage.xaml
    /// </summary>
    public partial class BookPage : Page
    {
        private int _bookId;

        public BookPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadBook();
        }

        private void LoadBook()
        {
            Core.ResetContext();
            var book = Core.Context.Books
                .FirstOrDefault(b => b.BookId == _bookId);
            if (book == null) return;

            TxtTitle.Text = book.Title;
            TxtAuthor.Text = "Автор: " + (book.Users?.DisplayName ?? "Неизвестен");
            TxtDescription.Text = book.Description;
            TxtContent.Text = book.TextContent;

            // --- ИЗМЕНЕННЫЙ БЛОК ДЛЯ ЗАГРУЗКИ КАРТИНКИ ---
            if (!string.IsNullOrEmpty(book.CoverPath))
            {
                try
                {
                    // Собираем полный путь к файлу относительно папки с запущенным .exe
                    string fullPath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        book.CoverPath.TrimStart('\\', '/'));

                    if (System.IO.File.Exists(fullPath))
                    {
                        ImgCover.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
                    }
                    else
                    {
                        ImgCover.Source = null; // Файла нет на диске — оставляем пустым или ставим заглушку
                    }
                }
                catch
                {
                    ImgCover.Source = null; // Если путь поврежден — гасим ошибку, чтобы приложение не вылетало
                }
            }
            else
            {
                ImgCover.Source = null;
            }
            // ---------------------------------------------

            var genres = book.Genres.Select(g => g.Name);
            TxtGenres.Text = "Жанры: " + string.Join(", ", genres);

            double avg = book.Reviews.Any()
                ? book.Reviews.Average(r => r.Rating)
                : 0;
            TxtRating.Text = $"⭐ {avg:F1} ({book.Reviews.Count} отзывов)";

            if (CurrentUser.IsAdmin)
            {
                BtnFreeze.Visibility = Visibility.Visible;
                BtnFreeze.Content = book.IsFrozen == true
                    ? "Разморозить книгу"
                    : "Заморозить книгу";
            }

            LoadReviews();
        }

        private void LoadReviews()
        {
            var reviews = Core.Context.Reviews
                .Where(r => r.BookId == _bookId)
                .ToList()
                .Select(r => new ReviewViewModel
                {
                    ReviewId = r.ReviewId,
                    UserLogin = r.Users?.Login ?? "Неизвестен",
                    ReviewText = r.ReviewText,
                    Rating = r.Rating,
                    FreezeVisible = CurrentUser.IsAdmin
                        ? Visibility.Visible
                        : Visibility.Collapsed
                }).ToList();

            ReviewsList.ItemsSource = reviews;
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnAddToList_Click(object sender, RoutedEventArgs e)
        {
            if (CmbAddToList.SelectedIndex == 0)
            {
                MessageBox.Show("Выберите список");
                return;
            }

            string section = CmbAddToList.SelectedItem is ComboBoxItem item
                ? item.Content.ToString()
                : "";

            var existing = Core.Context.ReadingList
                .FirstOrDefault(rl => rl.UserId == CurrentUser.User.UserId
                                   && rl.BookId == _bookId);
            if (existing != null)
            {
                existing.Section = section;
            }
            else
            {
                var newItem = new ReadingList
                {
                    UserId = CurrentUser.User.UserId,
                    BookId = _bookId,
                    Section = section,
                    AddedAt = DateTime.Now
                };
                Core.Context.ReadingList.Add(newItem);
            }

            Core.Context.SaveChanges();
            MessageBox.Show("Книга добавлена в список «" + section + "»");
        }

        private void BtnComplain_Click(object sender, RoutedEventArgs e)
        {
            var complaint = new Complaints
            {
                UserId = CurrentUser.User.UserId,
                BookId = _bookId,
                Reason = "Жалоба на книгу",
                CreatedAt = DateTime.Now
            };
            Core.Context.Complaints.Add(complaint);
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба отправлена");
        }

        private void BtnFreeze_Click(object sender, RoutedEventArgs e)
        {
            var book = Core.Context.Books
                .FirstOrDefault(b => b.BookId == _bookId);
            if (book == null) return;

            book.IsFrozen = !book.IsFrozen;
            Core.Context.SaveChanges();

            BtnFreeze.Content = book.IsFrozen == true
                ? "Разморозить книгу"
                : "Заморозить книгу";

            MessageBox.Show(book.IsFrozen == true
                ? "Книга заморожена"
                : "Книга разморожена");
        }

        private void BtnAddReview_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TxtRatingInput.Text, out int rating)
                || rating < 1 || rating > 10)
            {
                MessageBox.Show("Оценка должна быть от 1 до 10");
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtReviewText.Text))
            {
                MessageBox.Show("Напишите текст отзыва");
                return;
            }

            var review = new Reviews
            {
                UserId = CurrentUser.User.UserId,
                BookId = _bookId,
                ReviewText = TxtReviewText.Text.Trim(),
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            Core.Context.Reviews.Add(review);
            Core.Context.SaveChanges();

            TxtReviewText.Text = "";
            TxtRatingInput.Text = "10";

            LoadBook();
        }

        private void BtnComplainReview_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int reviewId = (int)btn.Tag;

            var complaint = new Complaints
            {
                UserId = CurrentUser.User.UserId,
                ReviewId = reviewId,
                Reason = "Жалоба на отзыв",
                CreatedAt = DateTime.Now
            };
            Core.Context.Complaints.Add(complaint);
            Core.Context.SaveChanges();
            MessageBox.Show("Жалоба на отзыв отправлена");
        }

        private void BtnFreezeReview_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            int reviewId = (int)btn.Tag;

            var review = Core.Context.Reviews
                .FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null) return;

            Core.Context.Reviews.Remove(review);
            Core.Context.SaveChanges();

            MessageBox.Show("Отзыв удалён");
            LoadReviews();
        }
    }

    public class ReviewViewModel
    {
        public int ReviewId { get; set; }
        public string UserLogin { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public Visibility FreezeVisible { get; set; }
    }
}
