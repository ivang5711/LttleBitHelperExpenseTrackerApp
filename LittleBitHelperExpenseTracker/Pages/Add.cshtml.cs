﻿using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Data.SQLite;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class AddModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");
        private int phone;
        public static string DefaultCurrency { get; set; } = string.Empty;
        public string CurrentUser { get; set; } = string.Empty;
        public int GetPhone()
        {
            return phone;
        }

        public void SetPhone(int value)
        {
            phone = value;
        }

        public AddModel(UserManager<IdentityUser> userManager, ILogger<IndexModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public static class UsersList
        {
            public static List<Expenses> NList { get; set; } = new List<Expenses>();
        }


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
                await Console.Out.WriteLineAsync(" BANKG Current USER hERE!=" + CurrentUser);
            }

            _ = StarAsync();
            Thread.Sleep(100);
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            Thread.Sleep(100);
            DefaultCurrency = "USD";
            foreach (Users user in result)
            {
                Console.WriteLine("user cur= " + user.LocalCurrency);
                DefaultCurrency = result.ToList()[0].LocalCurrency;
            }
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null || user.PhoneNumber is null)
            {
                throw new ArgumentException(nameof(user));
            }

            GetDefaultCurrecncy();
            SetPhone(int.Parse(user.PhoneNumber));
            CurrentUser = user.PhoneNumber;
            await Console.Out.WriteLineAsync("PHO: " + GetPhone());
            Thread.Sleep(100);
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(CurrentUser)};";
            var result = connection.Query<Users>(sql);
            if (result.Any())
            {
                DefaultCurrency = result.ToList()[0].LocalCurrency;
                await Console.Out.WriteLineAsync("GRAND FINALE!");
            }
            else
            {
                DefaultCurrency = "USD";
            }
        }

        public IActionResult OnPost()
        {
            async Task StarAsync()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user is null || user.PhoneNumber is null || user.UserName is null)
                {
                    throw new ArgumentException(nameof(user));
                }

                GetDefaultCurrecncy();
                SetPhone(int.Parse(user.PhoneNumber));
                CurrentUser = user.UserName;
                await Console.Out.WriteLineAsync("PHO: " + GetPhone());
            }

            _ = StarAsync();
            Thread.Sleep(100);
            if (Request.Form["expenseType"].IsNullOrEmpty() ||
                Request.Form["expenseAmount"].IsNullOrEmpty() ||
                Request.Form["dateTime"].IsNullOrEmpty() ||
                    Request.Form is null)
            {
                throw new ArgumentException(nameof(Request));
            }

            string? expenseType = Request.Form["expenseType"];
            string? expenseAmount = Request.Form["expenseAmount"];
            string? expenseComment = Request.Form["expenseComment"];
            string? dateTime = Request.Form["dateTime"];
            if (expenseAmount == null)
            {
                throw new ArgumentException(nameof(expenseAmount));
            }

            if (expenseType == null)
            {
                throw new ArgumentException(nameof(expenseType));
            }

            if (expenseComment == null)
            {
                throw new ArgumentException(nameof(expenseComment));
            }

            if (dateTime == null)
            {
                throw new ArgumentException(nameof(dateTime));
            }

            Thread.Sleep(100);
            var user = _userManager.GetUserAsync(User).Result;
            if (user is null || user.PhoneNumber is null)
            {
                throw new ArgumentException(nameof(user));
            }

            SetPhone(int.Parse(user.PhoneNumber));
            int userId = GetPhone();
            string? currency = Request.Form["currency"];
            if (currency == null || Request.Form is null)
            {
                throw new ArgumentException(nameof(currency));
            }

            Console.WriteLine($"database path: {dbPath}.");
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = "INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES (@ExpenseType, @ExpenseAmount, @ExpenseComment, @DateTime, @UserId, @Currency)";
                var newRecord = new Expenses() { ExpenseType = expenseType, ExpenseAmount = float.Parse(expenseAmount), ExpenseComment = expenseComment, DateTime = Convert.ToDateTime(dateTime), UserId = userId, Currency = currency };
                var rowsAffected = connection.Execute(sql, newRecord);
                Console.WriteLine($"{rowsAffected} row(s) inserted.");
            }

            return new RedirectToPageResult("History");
        }
    }
}