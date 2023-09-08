using Microsoft.AspNetCore.Identity;
using System.Data.SQLite;
using Dapper;

namespace LittleBitHelperExpenseTracker.Models
{
    public class GetDefaultCurrency
    {
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");
        public string GetDefaultCurrecncy(IdentityUser user)
        {

            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            if (user.PhoneNumber == null)
            {
                throw new ArgumentException(null, nameof(user));
            }

            var sql = $"SELECT localCurrency FROM users WHERE localUserId={int.Parse(user.PhoneNumber)};";
            var result = connection.Query<Users>(sql);
            string defaultCurrency = "USD";

            foreach (Users item in result)
            {
                defaultCurrency = result.ToList()[0].LocalCurrency;
            }

            return defaultCurrency;
        }
    }
}   