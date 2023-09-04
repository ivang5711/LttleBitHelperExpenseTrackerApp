using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Models
{
    [Authorize]
    public class GetDefaultCurrency : PageModel
    {

        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");
        public string CurrentUser { get; set; } = string.Empty;
        public string DefaultCurrency { get; set; } = string.Empty;
        public GetDefaultCurrency(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public class Users
        {
            public int LocalUserId { get; set; }
            public required string LocalCurrency { get; set; }
        }

        public string GetDefaultLocalCurrecncy()
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
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            DefaultCurrency = result.ToList()[0].LocalCurrency;
            return DefaultCurrency;
        }
    }

}
