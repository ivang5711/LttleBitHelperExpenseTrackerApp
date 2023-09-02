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
                    throw new ArgumentException(nameof(user));
                }

                CurrentUser = user.PhoneNumber;
            }

            _ = StarAsync();
            Thread.Sleep(100);
            string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Thread.Sleep(100);
            Console.WriteLine("Result: " + result.ToList()[0].LocalCurrency);
            DefaultCurrency = result.ToList()[0].LocalCurrency;
            Console.WriteLine("DEFCU = " + DefaultCurrency);
        }

        public async Task OnGetAsync()
        {
            UsersList.NList.Clear();
            UsersList.FinalList.Clear();
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                Console.WriteLine("User is null");
                throw new ArgumentException(nameof(user));
            }

            var phoneNumber = user.PhoneNumber;
            if (phoneNumber != null)
            {
                string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
                Console.WriteLine($"database path: {dbPath}.");
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
                    Console.WriteLine("2-DEFCUR:= " + DefaultCurrency);
                    foreach (var item in UsersList.FinalList)
                    {
                        item.ExpenseAmount *= JsonOperations.PersonPersistent.Rates[DefaultCurrency];
                    }
                }
            }
            else
            {
                UsersList.FinalList.Clear();
                int tempTime = (int)(DateTime.UtcNow.Ticks / 1000);
                _ = _userManager.SetPhoneNumberAsync(user, tempTime.ToString());
                var a = _userManager.GenerateChangePhoneNumberTokenAsync(user, tempTime.ToString()).Result;
                _ = _userManager.ChangePhoneNumberAsync(user, tempTime.ToString(), a);

                if (user.UserName is null || user.UserName.Length == 0)
                {
                    throw new ArgumentException(nameof(user.UserName));
                }

                Thread.Sleep(1000);
                string dbPath = "..\\LittleBitHelperExpenseTracker\\tracker-database.db";
                Console.WriteLine($"database path: {dbPath}.");
                using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
                {
                    var sql = "INSERT INTO users (localUserName, localUserId) VALUES (@LocalUserName, @LocalUserId);";
                    Users newRecord = new Users() { LocalUserName = user.UserName, LocalUserId = tempTime, LocalCurrency = string.Empty };
                    var rowsAffected = connection.Execute(sql, newRecord);
                    Console.WriteLine($"{rowsAffected} row(s) inserted.");
                }
            }
        }

        public static class UsersList
        {
            public static List<Expenses> FinalList { get; set; } = new List<Expenses>();
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }

        public class Expenses
        {
            public string Currency { get; set; } = string.Empty;
            public DateTime DateTime { get; set; }
            public float ExpenseAmount { get; set; }
            public string ExpenseComment { get; set; } = string.Empty;
            public string ExpenseType { get; set; } = string.Empty;
            public int Id { get; set; }
            public int UserId { get; set; }
        }

        public class Users
        {
            public string LocalCurrency { get; set; } = string.Empty;
            public int LocalUserId { get; set; }
            public string LocalUserName { get; set; } = string.Empty;
        }
    }
}