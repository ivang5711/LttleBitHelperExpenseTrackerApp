using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");

        public IndexModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public static string DefaultCurrency { get; set; } = string.Empty;
        public string CurrentUser { get; set; } = string.Empty;

        public void GetDefaultCurrecncy()
        {
            async Task StarAsync()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null || user.PhoneNumber is null)
                {
                    _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                    throw new ArgumentException(nameof(user));
                }

                CurrentUser = user.PhoneNumber;
            }

            _ = StarAsync();
            Thread.Sleep(100);
            _logger.LogDebug("database path: {dbPath}", dbPath);
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Thread.Sleep(100);
            DefaultCurrency = "USD";

            foreach (Users item in result)
            {
                DefaultCurrency = result.ToList()[0].LocalCurrency;
                _logger.LogDebug("User {Username} currency = {LocalCurrency}. Time: {Time}", item.LocalUserName, item.LocalCurrency, DateTime.UtcNow);

            }
        }

        public async Task OnGetAsync()
        {
            UsersList.NList.Clear();
            UsersList.FinalList.Clear();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogError("User is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(user));
            }

            var phoneNumber = user.PhoneNumber;
            if (phoneNumber != null)
            {
                GetDefaultCurrecncy();
                _logger.LogDebug("database path: {dbPath}", dbPath);
                using var connection = new SQLiteConnection($"Data Source={dbPath}");
                var sql = $"SELECT expenseType, SUM(expenseAmount) AS expenseAmount, currency FROM expenses WHERE userId={phoneNumber} GROUP BY expenseType, currency;";
                connection.Query<Expenses>(sql);
                UsersList.NList = connection.Query<Expenses>(sql).ToList();
                if (UsersList.NList.Count > 0)
                {
                    foreach (var item in UsersList.NList)
                    {
                        item.ExpenseAmount /= JsonOperations.PersonPersistent.Rates[item.Currency];
                    }

                    UsersList.FinalList.Clear();
                    UsersList.FinalList.Add(UsersList.NList[0]);
                    for (int i = 1; i < UsersList.NList.Count; i++)
                    {
                        if (UsersList.NList[i].ExpenseType == UsersList.NList[i - 1].ExpenseType)
                        {
                            UsersList.FinalList[i - 1].ExpenseAmount += UsersList.NList[i].ExpenseAmount;
                        }
                        else
                        {
                            UsersList.FinalList.Add(UsersList.NList[i]);
                        }
                    }

                    GetDefaultCurrecncy();
                    foreach (var item in UsersList.FinalList)
                    {
                        item.ExpenseAmount *= JsonOperations.PersonPersistent.Rates[DefaultCurrency];
                    }
                }
            }
            else
            {
                UsersList.FinalList.Clear();
                int tempTime = Random.Shared.Next();
                _ = _userManager.SetPhoneNumberAsync(user, tempTime.ToString());
                var a = _userManager.GenerateChangePhoneNumberTokenAsync(user, tempTime.ToString()).Result;
                _ = _userManager.ChangePhoneNumberAsync(user, tempTime.ToString(), a);

                if (user.UserName is null || user.UserName.Length == 0)
                {
                    _logger.LogError("User.Username is null. Time: {Time}", DateTime.UtcNow);
                    throw new ArgumentException(nameof(user.UserName));
                }

                Thread.Sleep(1000);
                _logger.LogDebug("database path: {dbPath}", dbPath);
                using var connection = new SQLiteConnection($"Data Source={dbPath}");
                var sql = "INSERT INTO users (localUserName, localUserId) VALUES (@LocalUserName, @LocalUserId);";
                Users newRecord = new() { LocalUserName = user.UserName, LocalUserId = tempTime, LocalCurrency = string.Empty };
                var rowsAffected = connection.Execute(sql, newRecord);
                _logger.LogDebug("{rowsAffected} row(s) inserted.", rowsAffected);
            }
        }

        public static class UsersList
        {
            public static List<Expenses> FinalList { get; set; } = new List<Expenses>();
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }
    }
}