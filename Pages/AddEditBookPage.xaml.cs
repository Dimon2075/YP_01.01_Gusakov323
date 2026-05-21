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
    /// Логика взаимодействия для AddEditBookPage.xaml
    /// </summary>
    public partial class AddEditBookPage : Page
    {
        private int? _bookId;

        public AddEditBookPage(int? bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadGenres();

            if (bookId.HasValue)
            {
                TxtPageTitle.Text = "Редактировать книгу";
                LoadBook(bookId.Value);
            }
            else
            {
                TxtPageTitle.Text = "Добавить книгу";
            }
        }

        private void LoadGenres()
        {
            var genres = Core.Context.Genres.ToList();
            GenresList.ItemsSource = genres;
        }

        private void LoadBook(int bookId)
        {
            var book = Core.Context.Books
                .FirstOrDefault(b => b.BookId == bookId);
            if (book == null) return;

            TxtTitle.Text = book.Title;
            TxtDescription.Text = book.Description;
            TxtContent.Text = book.TextContent;
            TxtCoverPath.Text = book.CoverPath;

            foreach (var item in GenresList.Items)
            {
                var genre = item as Genres;
                if (genre != null && book.Genres.Any(g => g.GenreId == genre.GenreId))
                    GenresList.SelectedItems.Add(item);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Введите название книги");
                return;
            }

            if (_bookId.HasValue)
            {
                var book = Core.Context.Books
                    .FirstOrDefault(b => b.BookId == _bookId.Value);
                if (book == null) return;

                book.Title = TxtTitle.Text.Trim();
                book.Description = TxtDescription.Text.Trim();
                book.TextContent = TxtContent.Text.Trim();
                book.CoverPath = TxtCoverPath.Text.Trim();

                book.Genres.Clear();
                foreach (Genres genre in GenresList.SelectedItems)
                    book.Genres.Add(genre);
            }
            else
            {
                var book = new Books
                {
                    Title = TxtTitle.Text.Trim(),
                    Description = TxtDescription.Text.Trim(),
                    TextContent = TxtContent.Text.Trim(),
                    CoverPath = TxtCoverPath.Text.Trim(),
                    AuthorId = CurrentUser.User.UserId,
                    IsFrozen = false,
                    CreatedAt = DateTime.Now
                };

                Core.Context.Books.Add(book);
                Core.Context.SaveChanges();

                foreach (Genres genre in GenresList.SelectedItems)
                    book.Genres.Add(genre);
            }

            Core.Context.SaveChanges();
            MessageBox.Show("Книга сохранена");
            NavigationService.GoBack();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
