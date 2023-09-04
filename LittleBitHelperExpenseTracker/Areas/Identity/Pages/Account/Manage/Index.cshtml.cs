// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using LittleBitHelperExpenseTracker.Models;

namespace LittleBitHelperExpenseTracker.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private static readonly string dbPath = Environment.GetEnvironmentVariable("dbPathLBH");

        public IndexModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Telegram ID")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(IdentityUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            Username = userName;
            Input = new InputModel
            {
                PhoneNumber = phoneNumber
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = $"SELECT * FROM users WHERE localUserName = '{user.UserName}';";
                var res = connection.Query<Users>(sql);
                await _userManager.SetPhoneNumberAsync(user, res.ToList()[0].LocalUserId.ToString());
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var tempUserNumber = user.PhoneNumber;
            Console.WriteLine("TempUN = " + tempUserNumber);
            user.PhoneNumber = Input.PhoneNumber;
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = $"UPDATE users SET localUserId = {user.PhoneNumber} WHERE localUserName = '{user.UserName}';";
                try
                {
                    connection.Execute(sql);
                }
                catch (SQLiteException)
                {
                    StatusMessage = "Error occured. The ID value already used.";
                    return RedirectToPage();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    await Console.Out.WriteLineAsync($"SQLite DB update exception:\n{ex}");
                    Console.ForegroundColor = ConsoleColor.White;
                    return RedirectToPage();
                }
            }

            var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                StatusMessage = "Unexpected error when trying to set phone number.";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            using (var connection = new SQLiteConnection($"Data Source={dbPath}"))
            {
                var sql = $"UPDATE expenses SET userId = {user.PhoneNumber} WHERE userId = {tempUserNumber};";
                try
                {
                    connection.Execute(sql);
                }
                catch (SQLiteException ex)
                {
                    StatusMessage = "Error occured." + ex;
                    return RedirectToPage();
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    await Console.Out.WriteLineAsync($"SQLite DB update exception:\n{ex}");
                    Console.ForegroundColor = ConsoleColor.White;
                    return RedirectToPage();
                }
            }

            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
