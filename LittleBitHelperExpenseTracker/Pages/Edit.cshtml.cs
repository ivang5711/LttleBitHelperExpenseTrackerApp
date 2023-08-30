using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
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

        public void OnGet(string toEdit)
        {
            string editable = toEdit;
            Console.WriteLine("&: " + toEdit);
            editable = "asd";
            Console.WriteLine("EDITABLE: " + editable);
        }

        public IActionResult OnPost(string toEdit)
        {
            int currentId = UsersList.NList[0].Id;
            
            string expenseType = Request.Form["expenseType"];
            string expenseAmount = Request.Form["expenseAmount"];
            string expenseComment = Request.Form["expenseComment"];
            string dateTime = Request.Form["dateTime"];
            //int userId = 394761293;
            string currency = Request.Form["currency"];

            Console.WriteLine("E-Mail: " + expenseType);
            Console.WriteLine("E-Mail: " + expenseAmount);
            Console.WriteLine("E-Mail: " + expenseComment);
            Console.WriteLine("E-Mail: " + dateTime);
            //Console.WriteLine("E-Mail: " + userId);
            Console.WriteLine("E-Mail: " + currency);

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                //var sql = "INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES (@ExpenseType, @ExpenseAmount, @ExpenseComment, @DateTime, @UserId, @Currency)";

                var sql = $"UPDATE expenses SET expenseType = @ExpenseType, expenseAmount = @ExpenseAmount, expenseComment = @ExpenseComment, dateTime = @DateTime, currency = @Currency WHERE id = {currentId};";

                {
                    Console.WriteLine($"EXP TYPE: {expenseType} EXP AMNT: {expenseAmount} EXP COMM: {expenseComment} DATETIME: {dateTime} CURR: {currency} CURid: {currentId}");
                    var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = int.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), Currency = currency };
                    connection.Execute(sql, newRecord);
                }
            }

            return new RedirectToPageResult("History");



        }

        public void OnPostTest(string TestString)
        {
            // Do something with TestString
            Console.WriteLine(TestString);
            Console.WriteLine("Success!");
            int getId = int.Parse(TestString);

            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");

            using var connection = new SQLiteConnection($"Data Source={dbPath}");

            var sql = $"SELECT * FROM expenses WHERE id={getId};";

            var results = connection.Query<Expenses>(sql);


            UsersList.NList = connection.Query<Expenses>(sql).ToList();

        }

    }
}