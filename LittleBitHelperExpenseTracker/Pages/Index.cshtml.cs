using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public class Expenses
        {
            public int Id { get; set; }
            public string ExpenseType { get; set; }
            public int ExpenseAmount { get; set; }
            public string ExpenseComment { get; set; }
            public DateTime DateTime { get; set; }
            public int UserId { get; set; }
            public string Currency { get; set; }

            public Expenses()
            {

            }
        }


        public static class UsersList
        {
            public static List<Expenses> NList { get; set; }

        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            //var userName = user.UserName;
            var phoneNumber = user.PhoneNumber;

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");

            using var connection = new SQLiteConnection($"Data Source={dbPath}");

            var sql = $"SELECT expenseType, SUM(expenseAmount) AS expenseAmount FROM expenses WHERE userId={phoneNumber} GROUP BY expenseType;";

            var results = connection.Query<Expenses>(sql);

            UsersList.NList = connection.Query<Expenses>(sql).ToList();

            foreach (var result in results)
            {
                Console.WriteLine($"Name: " + result.Id + " id: " + result.ExpenseType);
            }


        }
    }
}