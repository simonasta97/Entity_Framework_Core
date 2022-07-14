namespace BookShop
{
    using BookShop.Models.Enums;
    using Data;
    using Initializer;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using var db = new BookShopContext();
            //DbInitializer.ResetDatabase(db);
            Console.WriteLine(GetBooksByPrice(db));
        }

        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            StringBuilder sb = new StringBuilder();

            AgeRestriction ageRestriction;
            bool parseSuccess = Enum.TryParse<AgeRestriction>(command, ignoreCase:true,out ageRestriction);

            if (!parseSuccess)
            {
                return String.Empty;
            }

            string[] bookTitles = context
                .Books
                .Where(b=>b.AgeRestriction == ageRestriction)
                .Select(b => b.Title)
                .OrderBy(t => t)
                .ToArray();

            return String.Join(Environment.NewLine, bookTitles);
        }

        public static string GetGoldenBooks(BookShopContext context)
        {
            string[] goldenBooks = context
                .Books
                .Where(b => b.EditionType == EditionType.Gold && b.Copies < 5000)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, goldenBooks);

        }

        public static string GetBooksByPrice(BookShopContext context)
        {
            var books = context.Books
                .Where(x => x.Price > 40)
                .Select(x => new
                {
                    Title = x.Title,
                    Price = x.Price,
                })
                .OrderByDescending(x => x.Price)
                .ToList();

            var result = string.Join(Environment.NewLine,
                books.Select(x => $"{x.Title} - ${x.Price:F2}"));

            return result;
        }

        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {
            string[] allTitles = context
                .Books
                .Where(b => b.ReleaseDate.Value.Year != year)
                .OrderBy(b => b.BookId)
                .Select(b => b.Title)
                .ToArray();

            return String.Join(Environment.NewLine, allTitles);
        }

        public static string GetBooksByCategory(BookShopContext context, string input)
        {
            var categories = input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToLower())
                .ToArray();

            var books = context.Books
                .Where(book => book.BookCategories
                    .Any(category => categories.Contains(category.Category.Name.ToLower())))
                .Select(books => books.Title)
                .OrderBy(title => title)
                .ToArray();

            var result = string.Join(Environment.NewLine, books);

            return result;
        }

        public static string GetBooksReleasedBefore(BookShopContext context, string date)
        {
            var targetDate = DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var books = context.Books
                .Where(x => x.ReleaseDate.Value < targetDate)
                .Select(x => new
                {
                    x.Title,
                    x.EditionType,
                    x.Price,
                    x.ReleaseDate.Value
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            var result = string.Join(Environment.NewLine,
                books.Select(book => $"{book.Title} - {book.EditionType} - ${book.Price:F2}"));

            return result;
        }

        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            string[] authors = context
                .Authors
                .Where(a => a.FirstName.EndsWith(input))
                .Select(a=> $"{a.FirstName} {a.LastName}")
                .ToArray()
                .OrderBy(b => b)
                .ToArray();

            return String.Join(Environment.NewLine, authors);
        }

        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            string[] titles = context
                .Books
                .Where(b => b.Title.ToLower().Contains(input.ToLower()))
                .Select(b => b.Title)
                .OrderBy(n => n)
                .ToArray();

            return String.Join(Environment.NewLine, titles);
        }

        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            string[] books = context
                .Books
                .Where(b => b.Author.LastName.ToLower().StartsWith(input.ToLower()))
                .OrderBy(b => b.BookId)
                .Select(a => $"{a.Title} ({a.Author.FirstName} {a.Author.LastName})")
                .ToArray();

            return String.Join(Environment.NewLine, books);
        }

        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var books = context
                .Books
                .Where(b => b.Title.Length > lengthCheck)
                .Select(b=>b)
                .ToList();

            return books.Count;
        }

        public static string CountCopiesByAuthor(BookShopContext context)
        {
            string[] authors = context
                .Authors
                .OrderByDescending(a => a.Books.Sum(b => b.Copies))
                .Select(a => $"{a.FirstName} {a.LastName} - {a.Books.Sum(b=>b.Copies)}")
                .ToArray();

            return String.Join(Environment.NewLine, authors);
        }

        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            string[] listOfProfit = context
                .Categories
                .OrderByDescending(a => a.CategoryBooks.Sum(b => b.Book.Price * b.Book.Copies))
                .ThenBy(a => a.Name)
                .Select(a => $"{a.Name} ${a.CategoryBooks.Sum(b => b.Book.Price * b.Book.Copies)}")
                .ToArray();

            return String.Join(Environment.NewLine, listOfProfit);
        }

        public static string GetMostRecentBooks(BookShopContext context)
        {
            var categoryBooks = context.Categories
                .Select(category => new
                {
                    CatName = category.Name,
                    Books = category.CategoryBooks.Select(b => new
                    {
                        Title = b.Book.Title,
                        Date = b.Book.ReleaseDate.Value,
                    })
                    .OrderByDescending(b => b.Date)
                    .Take(3)
                    .ToList(),
                })
                .OrderBy(x => x.CatName)
                .ToList();

            var sb = new StringBuilder();

            foreach (var category in categoryBooks)
            {
                sb.AppendLine($"--{category.CatName}");

                foreach (var book in category.Books)
                {
                    sb.AppendLine($"{book.Title} ({book.Date.Year})");
                }
            }

            return sb.ToString().TrimEnd();
        }

        public static void IncreasePrices(BookShopContext context)
        {
            var books = context.Books
                .Where(book => book.ReleaseDate.Value.Year < 2010)
                .ToList();

            foreach (var book in books)
            {
                book.Price += 5;
            }

            context.SaveChanges();
        }

        public static int RemoveBooks(BookShopContext context)
        {
            var books = context.Books
                .Where(b => b.Copies < 4200)
                .ToList();

            context.Books.RemoveRange(books);

            context.SaveChanges();

            return books.Count;
        }
    }
}
