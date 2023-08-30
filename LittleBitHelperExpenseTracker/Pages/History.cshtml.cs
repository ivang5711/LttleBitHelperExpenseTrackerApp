using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class HistoryModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public HistoryModel(ILogger<IndexModel> logger)
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

        public string Message { get; set; } = "Initial Request";

        public IActionResult OnPostView(int id)
        {
            Message = $"View handler fired for {id}";
            Console.WriteLine(Message);
            return new RedirectToPageResult("/Edit", $"{id}");
        }

        public void OnGet()
        {
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");

            using var connection = new SQLiteConnection($"Data Source={dbPath}");

            var sql = "SELECT * FROM expenses;";

            var results = connection.Query<Expenses>(sql);


            UsersList.NList = connection.Query<Expenses>(sql).ToList();

            foreach (var result in results)
            {
                Console.WriteLine($"Name: " + result.Id + " id: " + result.ExpenseType);
            }
        }

        public IActionResult OnPost()
        {
            Console.WriteLine("post");
            string toDelete = Request.Form["toDelete"];
            string toEdit = Request.Form["toEdit"];

            Console.WriteLine("DELETE: " + toDelete);
            Console.WriteLine("EDIT: " + toEdit);

            if (toEdit != null)
            {
                Console.WriteLine("To Edit this!");
                int edit = int.Parse(toEdit);
                return new RedirectToPageResult("/Edit", toEdit);
            }
            else if (toDelete != null)
            {
                Console.WriteLine("To Delete this!");
                int del = int.Parse(toDelete);

                string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
                Console.WriteLine($"database path: {dbPath}.");

                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    var sql = $"DELETE FROM expenses WHERE id={del}";
                    {
                        var rowsAffected = connection.Execute(sql);
                        Console.WriteLine($"{rowsAffected} row(s) deleted.");
                    }
                }
                return new RedirectToPageResult("History");


            }

            return Page();
        }
    }
}