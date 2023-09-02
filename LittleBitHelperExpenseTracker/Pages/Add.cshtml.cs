using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private int phone;
        public static string DefaultCurrency { get; set; } = string.Empty;
        public string CurrentUser { get; set; } = string.Empty;
        public int GetPhone()
        {
            return phone;
        }

        public void SetPhone(int value)
        {
            phone = value;
        }

        public class Users
        {
            public int LocalUserId { get; set; }
            public required string LocalCurrency { get; set; }
        }

        public AddModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public class Expenses
        {
            public int Id { get; set; }
            public string ExpenseType { get; set; } = string.Empty;
            public int ExpenseAmount { get; set; }
            public string ExpenseComment { get; set; } = string.Empty;
            public DateTime DateTime { get; set; }
            public int UserId { get; set; }
            public string Currency { get; set; } = string.Empty;

            public Expenses()
            {
            }
        }

        public static class UsersList
        {
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }

        public async Task OnGetAsync()
        {
            Console.WriteLine("Get here! ");
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                throw new ArgumentException(nameof(user));
            }

            SetPhone(int.Parse(user.PhoneNumber));
            CurrentUser = user.PhoneNumber;
            await Console.Out.WriteLineAsync("PHO: " + GetPhone());
            Console.WriteLine("PHones:" + GetPhone());
            Thread.Sleep(100);
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Console.WriteLine("Result: " + result.ToList()[0].LocalCurrency);
            DefaultCurrency = result.ToList()[0].LocalCurrency;
            Console.WriteLine("DEFCU = " + DefaultCurrency);
            await Console.Out.WriteLineAsync("GRAND FINALE!");
        }

        public IActionResult OnPost()
        {
            async Task StarAsync()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null || user.PhoneNumber is null || user.UserName is null)
                {
                    throw new ArgumentException(nameof(user));
                }

                SetPhone(int.Parse(user.PhoneNumber));
                CurrentUser = user.UserName;
                await Console.Out.WriteLineAsync("PHO: " + GetPhone());
            }

            _ = StarAsync();
            Thread.Sleep(100);
            if (Request.Form["expenseType"].IsNullOrEmpty() ||
                Request.Form["expenseAmount"].IsNullOrEmpty() ||
                Request.Form["dateTime"].IsNullOrEmpty() ||
                    Request.Form is null)
            {
                throw new ArgumentException(nameof(Request));
            }

            string? expenseType = Request.Form["expenseType"];
            string? expenseAmount = Request.Form["expenseAmount"];
            string? expenseComment = Request.Form["expenseComment"];
            string? dateTime = Request.Form["dateTime"];
            if (expenseAmount == null)
            {
                throw new ArgumentException(nameof(expenseAmount));
            }

            if (expenseType == null)
            {
                throw new ArgumentException(nameof(expenseType));
            }

            if (expenseComment == null)
            {
                throw new ArgumentException(nameof(expenseComment));
            }

            if (dateTime == null)
            {
                throw new ArgumentException(nameof(dateTime));
            }

            Thread.Sleep(100);
            int userId = GetPhone();
            string? currency = Request.Form["currency"];
            if (currency == null)
            {
                throw new ArgumentException(nameof(currency));
            }

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = "INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES (@ExpenseType, @ExpenseAmount, @ExpenseComment, @DateTime, @UserId, @Currency)";
                var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = int.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), UserId = userId, Currency = currency };
                var rowsAffected = connection.Execute(sql, newRecord);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }

            return new RedirectToPageResult("History");
        }
    }
}