using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;
using LittleBitHelperExpenseTracker.Models;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");

        public HistoryModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public static class UsersList
        {
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }

        public string Message { get; set; } = "Initial Request";

        public IActionResult OnPostView(int id)
        {
            Message = $"View handler fired for {id}";
            _logger.LogDebug("OnPostView Message: {messageOnView}", Message);
            return new RedirectToPageResult("/Edit", $"{id}");
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(user));
            }

            var phoneNumber = user.PhoneNumber;
            _logger.LogDebug("database path: {dbPath}", dbPath);
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT * FROM expenses WHERE userId={phoneNumber};";
            connection.Query<Expenses>(sql);
            UsersList.NList = connection.Query<Expenses>(sql).ToList();
        }

        public IActionResult OnPost()
        {
            string? toDelete = Request.Form["toDelete"];
            string? toEdit = Request.Form["toEdit"];
            if (toEdit != null)
            {
                return new RedirectToPageResult("/Edit", toEdit);
            }
            else if (toDelete != null)
            {
                int del = int.Parse(toDelete);
                _logger.LogDebug("database path: {dbPath}", dbPath);
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    var sql = $"DELETE FROM expenses WHERE id={del}";
                    var rowsAffected = connection.Execute(sql);
                    _logger.LogDebug("{rowsAffected} row(s) inserted.", rowsAffected);
                }

                return new RedirectToPageResult("History");
            }

            return Page();
        }
    }
}