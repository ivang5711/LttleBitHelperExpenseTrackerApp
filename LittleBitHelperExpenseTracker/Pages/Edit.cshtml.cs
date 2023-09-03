using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public EditModel(ILogger<IndexModel> logger)
        {
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

        public IActionResult OnPost()
        {
            int currentId = UsersList.NList[0].Id;

            string? expenseType = Request.Form["expenseType"];
            string? expenseAmount = Request.Form["expenseAmount"];
            string? expenseComment = Request.Form["expenseComment"];
            string? dateTime = Request.Form["dateTime"];
            string? currency = Request.Form["currency"];
            if (currency is null)
            {
                throw new ArgumentException(nameof(currency));
            }

            if (dateTime is null)
            {
                throw new ArgumentException(nameof(dateTime));
            }

            if (expenseComment is null)
            {
                throw new ArgumentException(nameof(expenseComment));
            }

            if (expenseAmount is null)
            {
                throw new ArgumentException(nameof(expenseAmount));
            }

            if (expenseType is null)
            {
                throw new ArgumentException(nameof(expenseType));
            }

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = $"UPDATE expenses SET expenseType = @ExpenseType, expenseAmount = @ExpenseAmount, expenseComment = @ExpenseComment, dateTime = @DateTime, currency = @Currency WHERE id = {currentId};";
                Console.WriteLine($"EXP TYPE: {expenseType} EXP AMNT: {expenseAmount} EXP COMM: {expenseComment} DATETIME: {dateTime} CURR: {currency} CURid: {currentId}");
                var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = float.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), Currency = currency };
                connection.Execute(sql, newRecord);
            }

            return new RedirectToPageResult("History");
        }

        public void OnPostTest(string TestString)
        {
            int getId = int.Parse(TestString);
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT * FROM expenses WHERE id={getId};";
            connection.Query<Expenses>(sql);
            UsersList.NList = connection.Query<Expenses>(sql).ToList();
        }
    }
}