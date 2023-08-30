using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    public class AddModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public AddModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

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

        public IActionResult OnPost()
        {
            string expenseType = Request.Form["expenseType"];
            string expenseAmount = Request.Form["expenseAmount"];
            string expenseComment = Request.Form["expenseComment"];
            string dateTime = Request.Form["dateTime"];
            int userId = 394761293;
            string currency = Request.Form["currency"];

            Console.WriteLine("E-Mail: " +  expenseType);
            Console.WriteLine("E-Mail: " + expenseAmount);
            Console.WriteLine("E-Mail: " + expenseComment);
            Console.WriteLine("E-Mail: " + dateTime);
            Console.WriteLine("E-Mail: " + userId);
            Console.WriteLine("E-Mail: " + currency);

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = "INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES (@ExpenseType, @ExpenseAmount, @ExpenseComment, @DateTime, @UserId, @Currency)";
                {
                    var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = int.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), UserId = userId, Currency = currency };
                    var rowsAffected = connection.Execute(sql, newRecord);
                    Console.WriteLine($"{rowsAffected} row(s) inserted.");
                }
            }

            return new RedirectToPageResult("History");
        }
    }
}