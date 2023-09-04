using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;
using LittleBitHelperExpenseTracker.Models;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class PreferencesModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");
        public string CurrentUser { get; set; } = string.Empty;
        public static string DefaultCurrency { get; set; } = string.Empty;
        public PreferencesModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
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
                await Console.Out.WriteLineAsync(" BANKG Current USER hERE!=" + CurrentUser);
            }

            _ = StarAsync();
            Thread.Sleep(100);
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Thread.Sleep(100);
            DefaultCurrency = "USD";
            foreach (Users user in result)
            {
                Console.WriteLine("user cur= " + user.LocalCurrency);
                DefaultCurrency = result.ToList()[0].LocalCurrency;
            }
        }

        public void OnGet()
        {
            GetDefaultCurrecncy();
        }

        public async Task OnPostAsync()
        {
            IdentityUser? user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                throw new ArgumentException(nameof(user));
            }

            CurrentUser = user.PhoneNumber;
            await Console.Out.WriteLineAsync("CurrentUser: " + CurrentUser);
            string? localCurrency = Request.Form["currency"];
            if (localCurrency is null)
            {
                throw new ArgumentException(nameof(localCurrency));
            }

            Console.WriteLine($"database path: {dbPath}.");
            using (SQLiteConnection connection = new($"Data Source={dbPath}"))
            {
                string sql = "Update users SET localCurrency = @LocalCurrency WHERE localUserId = @LocalUserId;";
                Users newRecord = new() { LocalUserId = int.Parse(CurrentUser), LocalCurrency = localCurrency };
                int rowsAffected = connection.Execute(sql, newRecord);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }
            DefaultCurrency = localCurrency;
        }
    }
}