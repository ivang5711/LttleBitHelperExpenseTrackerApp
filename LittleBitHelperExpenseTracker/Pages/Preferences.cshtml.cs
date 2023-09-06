using Dapper;
using LittleBitHelperExpenseTracker.Models;
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
                    _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                    throw new ArgumentException(nameof(user));
                }

                CurrentUser = user.PhoneNumber;
            }

            _ = StarAsync();
            Thread.Sleep(100);
            _logger.LogDebug("database path: {dbPath}", dbPath);
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Thread.Sleep(100);
            DefaultCurrency = "USD";
            foreach (Users item in result)
            {
                DefaultCurrency = result.ToList()[0].LocalCurrency;
                _logger.LogDebug("User {Username} currency = {LocalCurrency}. Time: {Time}", item.LocalUserName, item.LocalCurrency, DateTime.UtcNow);
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
                _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(user));
            }

            CurrentUser = user.PhoneNumber;
            string? localCurrency = Request.Form["currency"];
            if (localCurrency is null)
            {
                _logger.LogError("localCurrency is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(localCurrency));
            }

            _logger.LogDebug("database path: {dbPath}", dbPath);
            using (SQLiteConnection connection = new($"Data Source={dbPath}"))
            {
                string sql = "Update users SET localCurrency = @LocalCurrency WHERE localUserId = @LocalUserId;";
                Users newRecord = new() { LocalUserId = int.Parse(CurrentUser), LocalCurrency = localCurrency };
                int rowsAffected = connection.Execute(sql, newRecord);
                _logger.LogDebug("{rowsAffected} row(s) inserted.", rowsAffected);
            }
            DefaultCurrency = localCurrency;
        }
    }
}