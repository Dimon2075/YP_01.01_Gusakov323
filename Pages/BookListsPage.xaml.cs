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
    /// Логика взаимодействия для BookListsPage.xaml
    /// </summary>
    public partial class BookListsPage : Page
    {
        private string _currentSection = "В планах";
        private List<ListBookViewModel> _allBooks;

        public BookListsPage()
        {
            InitializeComponent();
            LoadSection(_currentSection);
        }

        private void LoadSection(string section)
        {
            Core.ResetContext();
            _currentSection = section;

            var items = Core.Context.ReadingList
                .Where(rl => rl.UserId == CurrentUser.User.UserId
                          && rl.Section == section)
                .ToList();

            _allBooks = items.Select(rl => new ListBookViewModel
            {
                ReadingListId = rl.ReadingListId,
                BookId = rl.BookId,
                Title = rl.Books?.Title ?? "",
                AuthorName = rl.Books?.Users?.DisplayName ?? "",
                CoverPath = rl.Books?.CoverPath ?? "",
                Section = rl.Section
            }).ToList();

            ApplySearch();
        }

        private void ApplySearch()
        {
            var result = _allBooks.AsEnumerable();
            string search = TxtSearch.Text.Trim();

            if (!string.IsNullOrEmpty(search) && search != "Поиск...")
            {
                search = search.ToLower();
                result = result.Where(b =>
                    b.Title.ToLower().Contains(search) ||
                    b.AuthorName.ToLower().Contains(search));
            }

            ListPanel.ItemsSource = result.ToList();
        }

        private void BtnSection_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            LoadSection(btn.Tag.ToString());
        }

        private void BookCard_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var book = border?.DataContext as ListBookViewModel;
            if (book == null) return;
            NavigationService.Navigate(new BookPage(book.BookId));
        }

        private void CmbMoveSection_Changed(object sender, SelectionChangedEventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb?.SelectedItem is ComboBoxItem item && cmb.Tag != null)
            {
                int id = (int)cmb.Tag;
                string newSection = item.Content.ToString();

                var entry = Core.Context.ReadingList
                    .FirstOrDefault(rl => rl.ReadingListId == id);
                if (entry != null)
                {
                    entry.Section = newSection;
                    Core.Context.SaveChanges();
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_allBooks == null) return;
            ApplySearch();
        }

        private void TxtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtSearch.Text == "Поиск...")
            {
                TxtSearch.Text = "";
                TxtSearch.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void TxtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtSearch.Text))
            {
                TxtSearch.Text = "Поиск...";
                TxtSearch.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }

    public class ListBookViewModel
    {
        public int ReadingListId { get; set; }
        public int BookId { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string CoverPath { get; set; }
        public string Section { get; set; }
    }
}
