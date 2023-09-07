using Dapper;
using LittleBitHelperExpenseTracker.Models;
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
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");

        public EditModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public static class UsersList
        {
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }

        public IActionResult OnPost()
        {
            string? toUpdate = Request.Form["toUpdate"];
            string? toCancell = Request.Form["toCancel"];
            if (toUpdate is null && toCancell is not null)
            {
                return new RedirectToPageResult("History");
            }

            int currentId = UsersList.NList[0].Id;
            string? expenseType = Request.Form["expenseType"];
            string? expenseAmount = Request.Form["expenseAmount"];
            string? expenseComment = Request.Form["expenseComment"];
            string? dateTime = Request.Form["dateTime"];
            string? currency = Request.Form["currency"];
            if (currency is null)
            {
                _logger.LogError("Currency is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(currency));
            }

            if (dateTime is null)
            {
                _logger.LogError("dateTime is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(dateTime));
            }

            if (expenseComment is null)
            {
                _logger.LogError("expenseComment is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(expenseComment));
            }

            if (expenseAmount is null)
            {
                _logger.LogError("expenseAmmount is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(expenseAmount));
            }

            if (expenseType is null)
            {
                _logger.LogError("ExpenseType is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(expenseType));
            }

            _logger.LogDebug("database path: {dbPath}", dbPath);
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = $"UPDATE expenses SET expenseType = @ExpenseType, expenseAmount = @ExpenseAmount, expenseComment = @ExpenseComment, dateTime = @DateTime, currency = @Currency WHERE id = {currentId};";

                _logger.LogDebug("expenseType: {expenseType} expenseAmount: {expenseAmount} expenseComment: {expenseComment} dateTime: {dateTime} currency: {currency} id: {currentId}", expenseType, expenseAmount, expenseComment, dateTime, currency, currentId);
                var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = float.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), Currency = currency };
                connection.Execute(sql, newRecord);
            }

            return new RedirectToPageResult("History");
        }

        public void OnPostEdit(string idToEdit)
        {
            int getId = int.Parse(idToEdit);
            _logger.LogDebug("database path: {dbPath}", dbPath);
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT * FROM expenses WHERE id={getId};";
            connection.Query<Expenses>(sql);
            UsersList.NList = connection.Query<Expenses>(sql).ToList();
        }
    }
}