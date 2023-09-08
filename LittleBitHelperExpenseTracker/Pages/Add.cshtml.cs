using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");

        public static string DefaultCurrency { get; set; } = string.Empty;
        public static IdentityUser? CurrentUser { get; set; }
        public int CurrentUserTelegramId { get; set; }

        public AddModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
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

        public async Task OnGetAsync()
        {
            await SetCurrentUser();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await SetCurrentUser();
            string? expenseType = Request.Form["expenseType"];
            string? expenseAmount = Request.Form["expenseAmount"];
            string? expenseComment = Request.Form["expenseComment"];
            if (string.IsNullOrEmpty(expenseComment))
            {
                _logger.LogError("expenseComment is null. Time: {Time}", DateTime.UtcNow);
                expenseComment = string.Empty;
            }

            if (string.IsNullOrEmpty(expenseAmount))
            {
                _logger.LogError("expenseAmmount is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(expenseAmount));
            }

            if (string.IsNullOrEmpty(expenseType))
            {
                _logger.LogError("ExpenseType is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(expenseType));
            }

            string? currency = Request.Form["currency"];
            if (currency == null || Request.Form is null)
            {
                _logger.LogError("Currency or Request.Form is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(currency));
            }

            _logger.LogDebug("database path: {dbPath}", dbPath);
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = "INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES (@ExpenseType, @ExpenseAmount, @ExpenseComment, @DateTime, @UserId, @Currency)";
                var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = float.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = DateTime.UtcNow, UserId = CurrentUserTelegramId, Currency = currency };
                var rowsAffected = connection.Execute(sql, newRecord);
                _logger.LogDebug("{rowsAffected} row(s) inserted.", rowsAffected);
            }

            return new RedirectToPageResult("History");
        }
    }
}