using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class PreferencesModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        public string CurrentUser { get; set; } = string.Empty;
        public static string DefaultCurrency { get; set; } = string.Empty;
        public PreferencesModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public class Users
        {
            public int LocalUserId { get; set; }
            public required string LocalCurrency { get; set; }
        }

        public void GetDefaultCurrecncy()
        {
            async Task StarAsync()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null || user.PhoneNumber is null)
                {
                    throw new ArgumentException(nameof(user));
                }
                CurrentUser = user.PhoneNumber;
            }

            _ = StarAsync();
            Thread.Sleep(100);
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Console.WriteLine("Result: " + result.ToList()[0].LocalCurrency);
            DefaultCurrency = string.Empty;
            DefaultCurrency = result.ToList()[0].LocalCurrency;
            Console.WriteLine("DEFCU = " + DefaultCurrency);
        }

        public void OnGet()
        {
            GetDefaultCurrecncy();
        }

        public Task OnPost()
        {
            GetDefaultCurrecncy();
            async Task StarAsync()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null || user.PhoneNumber is null)
                {
                    throw new ArgumentException(nameof(user));
                }

                CurrentUser = user.PhoneNumber;
                await Console.Out.WriteLineAsync("CurrentUser: " + CurrentUser);
            }

            _ = StarAsync();
            string? localCurrency = Request.Form["currency"];
            if (localCurrency is null)
            {
                throw new ArgumentException(nameof(localCurrency));
            }

            Console.WriteLine("Currency: " + localCurrency);
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = "Update users SET localCurrency = @LocalCurrency WHERE localUserId = @LocalUserId;";
                Users newRecord = new Users() { LocalUserId = int.Parse(CurrentUser), LocalCurrency = localCurrency };
                var rowsAffected = connection.Execute(sql, newRecord);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
            GetDefaultCurrecncy();
            return Task.CompletedTask;
        }
    }
}