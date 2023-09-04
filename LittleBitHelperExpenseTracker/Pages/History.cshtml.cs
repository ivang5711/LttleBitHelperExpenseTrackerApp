using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

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

        public class Expenses
        {
            public int Id { get; set; }
            public string ExpenseType { get; set; } = string.Empty;
            public float ExpenseAmount { get; set; }
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

        public string Message { get; set; } = "Initial Request";

        public IActionResult OnPostView(int id)
        {
            Message = $"View handler fired for {id}";
            Console.WriteLine(Message);
            return new RedirectToPageResult("/Edit", $"{id}");
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                throw new ArgumentException(nameof(user));
            }

            var phoneNumber = user.PhoneNumber;
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT * FROM expenses WHERE userId={phoneNumber};";
            connection.Query<Expenses>(sql);
            UsersList.NList = connection.Query<Expenses>(sql).ToList();
        }

        public IActionResult OnPost()
        {
            Console.WriteLine("post");
            string? toDelete = Request.Form["toDelete"];
            string? toEdit = Request.Form["toEdit"];
            if (toEdit != null)
            {
                return new RedirectToPageResult("/Edit", toEdit);
            }
            else if (toDelete != null)
            {
                int del = int.Parse(toDelete);
                Console.WriteLine($"database path: {dbPath}.");
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    var sql = $"DELETE FROM expenses WHERE id={del}";
                    var rowsAffected = connection.Execute(sql);
                    Console.WriteLine($"{rowsAffected} row(s) deleted.");
                }

                return new RedirectToPageResult("History");
            }

            return Page();
        }
    }
}