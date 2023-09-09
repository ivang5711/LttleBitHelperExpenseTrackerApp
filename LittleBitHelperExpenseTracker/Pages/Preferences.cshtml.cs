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
        private static readonly string? dbPath = Program.Default!.DbPathData;
        public static string DefaultCurrency { get; set; } = string.Empty;
        public int CurrentUserTelegramId { get; set; }

        public PreferencesModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(user));
            }

            var defaultCurrencyGetter = new GetDefaultCurrency();
            DefaultCurrency = defaultCurrencyGetter.GetDefaultCurrecncy(user);
            CurrentUserTelegramId = int.Parse(user.PhoneNumber);
        }

        public async Task OnGet()
        {
            await SetCurrentUser();
        }

        public async Task OnPostAsync()
        {
            await SetCurrentUser();

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
                Users newRecord = new() { LocalUserId = CurrentUserTelegramId, LocalCurrency = localCurrency };
                int rowsAffected = connection.Execute(sql, newRecord);
                _logger.LogDebug("{rowsAffected} row(s) inserted.", rowsAffected);
            }
            DefaultCurrency = localCurrency;
        }
    }
}