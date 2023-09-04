using Dapper;
using LittleBitHelperExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Data.SQLite;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static LittleBitHelperExpenseTracker.Models.JsonOperations;

namespace LittleBitHelperExpenseTrackerAppTelegramBot
{
    internal class Program
    {
        protected Program()
        { }

        private static readonly string? botToken = Environment.GetEnvironmentVariable("LBHB1");
        private static readonly string? dbPath = Environment.GetEnvironmentVariable("dbPathLBH");
        private static readonly string[] CurrencyKeys = new string[] { "default", "AED", "AFN", "ALL", "AMD", "ANG", "AOA", "ARS", "AUD", "AWG", "AZN", "BAM", "BBD", "BDT", "BGN", "BHD", "BIF", "BMD", "BND", "BOB", "BRL", "BSD", "BTC", "BTN", "BWP", "BYN", "BZD", "CAD", "CDF", "CHF", "CLF", "CLP", "CNH", "CNY", "COP", "CRC", "CUC", "CUP", "CVE", "CZK", "DJF", "DKK", "DOP", "DZD", "EGP", "ERN", "ETB", "EUR", "FJD", "FKP", "GBP", "GEL", "GGP", "GHS", "GIP", "GMD", "GNF", "GTQ", "GYD", "HKD", "HNL", "HRK", "HTG", "HUF", "IDR", "ILS", "IMP", "INR", "IQD", "IRR", "ISK", "JEP", "JMD", "JOD", "JPY", "KES", "KGS", "KHR", "KMF", "KPW", "KRW", "KWD", "KYD", "KZT", "LAK", "LBP", "LKR", "LRD", "LSL", "LYD", "MAD", "MDL", "MGA", "MKD", "MMK", "MNT", "MOP", "MRU", "MUR", "MVR", "MWK", "MXN", "MYR", "MZN", "NAD", "NGN", "NIO", "NOK", "NPR", "NZD", "OMR", "PAB", "PEN", "PGK", "PHP", "PKR", "PLN", "PYG", "QAR", "RON", "RSD", "RUB", "RWF", "SAR", "SBD", "SCR", "SDG", "SEK", "SGD", "SHP", "SLL", "SOS", "SRD", "SSP", "STD", "STN", "SVC", "SYP", "SZL", "THB", "TJS", "TMT", "TND", "TOP", "TRY", "TTD", "TWD", "TZS", "UAH", "UGX", "USD", "UYU", "UZS", "VES", "VND", "VUV", "WST", "XAF", "XAG", "XAU", "XCD", "XDR", "XOF", "XPD", "XPF", "XPT", "YER", "ZAR", "ZMW", "ZWL" };

        public static async Task<bool> CheckUser(ITelegramBotClient botClient, Message? message)
        {
            Thread.Sleep(100);
            Console.WriteLine($"database path: {dbPath}.");
            using var connection = new SQLiteConnection($"Data Source={dbPath}");
            var sql = $"SELECT localUserId FROM users;";
            var result = connection.Query<Users>(sql).ToList();
            Users a = new();
            if (message != null)
            {
                a.LocalUserId = message.Chat.Id;
                int flag = 0;
                foreach (var item in result)
                {
                    if (item.LocalUserId == message.Chat.Id)
                    {
                        flag = 1;
                        break;
                    }
                }

                if (flag == 1)
                {
                    return true;
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, $"Hello and welcome to the LittleBitHelperBot\nYour ID is:\n{message.Chat.Id}\nRegister on the Web App first. use /start command to get your telegram id and go to the web app to register. After registration go to settings and fill in telegram ID field with your telegram Id.");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                Message? message = update.Message;
                if (message is null || message.Text is null)
                {
                    return;
                }
                string messageRecieved = message.Text.ToLower();
                if (messageRecieved.Length > 0 && messageRecieved[0] == '/')
                {
                    bool check = await CheckUser(botClient, message);
                    if (check)
                    {
                        if (messageRecieved.Length >= 6 && messageRecieved[0..6] == "/start")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Dear {message.Chat.Username}\nHello and welcome to the LittleBitHelperBot!\nStart here:\n/start - shows this message\n/help - shows a list of available commands", cancellationToken: cancellationToken);
                            return;
                        }

                        if (messageRecieved.Length >= 5 && messageRecieved[0..5] == "/help")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "A list of available commands:\n/start - shows welcome message\n/help - shows a list of available commands\n/new - add a new expense record.\nUseage: type amount comment.\nExample: taxi 350 to airport\n/all - provides a summ of all expenses for all time\n/typed - provides a list of types with total amount spent for each type\n/history - prints out all the records\n/delete - allows to deleta a record.\nUsage: specify a record id (history command can help to find an id).\nExample: 17\n/currency - allows to change the default currency.\nUsage: specify new currency.\nExample: USD\nYou can get the current currency value by sending the following command:\n/currency current", cancellationToken: cancellationToken);
                            return;
                        }

                        if (messageRecieved.Length >= 4 && messageRecieved[0..4] == "/new")
                        {
                            if (messageRecieved.TrimEnd() == "/new")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "To add a record you must provide type, amount and optional comment using following syntax: /new [type] [ammount] [comment]\nExample:\n/new Food 123 Bought a bag of fruits on a local food market. Tasty!", cancellationToken: cancellationToken);
                            }
                            else
                            {
                                string[] messageParsed = messageRecieved.Split();
                                List<string> messages = new();
                                foreach (string item in messageParsed)
                                {
                                    if (!string.IsNullOrWhiteSpace(item))
                                    {
                                        messages.Add(item);
                                    }
                                }

                                if (messages.Count >= 3)
                                {
                                    if (float.TryParse(messages[2], out float amountParsed))
                                    {
                                        Console.WriteLine("Parsed successfully!");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Parsing failure", cancellationToken: cancellationToken);
                                        return;
                                    }
                                    string parsedComment = string.Empty;
                                    if (messages.Count > 3)
                                    {
                                        StringBuilder bld = new();
                                        for (int i = 3; i < messages.Count; i++)
                                        {
                                            bld.Append($" {messages[i]}");
                                        }

                                        parsedComment = bld.ToString();
                                    }

                                    Thread.Sleep(100);
                                    Console.WriteLine($"database path: {dbPath}.");
                                    using var connection = new SQLiteConnection($"Data Source={dbPath}");
                                    var sql = $"SELECT localCurrency FROM users WHERE localUserId = {message.Chat.Id};";
                                    var result = connection.Query<Users>(sql).ToList();
                                    string defaultCurrency = string.Empty;
                                    foreach (var item in result)
                                    {
                                        defaultCurrency = item.LocalCurrency;
                                    }

                                    Console.WriteLine($"database path: {dbPath}.");
                                    Thread.Sleep(100);
                                    sql = $"INSERT INTO expenses (expenseType, expenseAmount, expenseComment, dateTime, userId, currency) VALUES ('{messages[1]}', {amountParsed}, '{parsedComment}', '{DateTime.UtcNow}', {message.Chat.Id}, '{defaultCurrency}');";
                                    Thread.Sleep(100);
                                    var res = connection.Execute(sql);
                                    Thread.Sleep(100);
                                    Console.WriteLine("result count: " + result);
                                    if (res > 0)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, $"New record added successfully", cancellationToken: cancellationToken);
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, $"The record was not added", cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Wrong arguments. Try again...", cancellationToken: cancellationToken);
                                }
                            }
                        }

                        if (messageRecieved.Length >= 4 && messageRecieved[0..4] == "/all")
                        {
                            Thread.Sleep(100);
                            Console.WriteLine($"database path: {dbPath}.");
                            using var connection = new SQLiteConnection($"Data Source={dbPath}");
                            var sql = $"SELECT localCurrency FROM users WHERE localUserId = {message.Chat.Id};";
                            var result = connection.Query<Users>(sql).ToList();
                            string defaultCurrency = string.Empty;
                            foreach (var item in result)
                            {
                                defaultCurrency = item.LocalCurrency;
                            }

                            Thread.Sleep(100);
                            sql = $"SELECT expenseType, SUM(expenseAmount) AS expenseAmount, currency FROM expenses WHERE userId={message.Chat.Id} GROUP BY expenseType, currency;";
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

                                float total = 0;
                                foreach (var item in UsersList.FinalList)
                                {
                                    item.ExpenseAmount *= JsonOperations.PersonPersistent.Rates[defaultCurrency];
                                    total += item.ExpenseAmount;
                                }

                                await botClient.SendTextMessageAsync(message.Chat, $"Expense summ calculated in your default currency: {defaultCurrency}\nTOTAL: {total}", cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "No records so far...", cancellationToken: cancellationToken);
                            }

                            return;
                        }

                        if (messageRecieved.Length >= 6 && messageRecieved[0..6] == "/typed")
                        {
                            Thread.Sleep(100);
                            Console.WriteLine($"database path: {dbPath}.");
                            using var connection = new SQLiteConnection($"Data Source={dbPath}");
                            var sql = $"SELECT localCurrency FROM users WHERE localUserId = {message.Chat.Id};";
                            var result = connection.Query<Users>(sql).ToList();
                            string defaultCurrency = string.Empty;
                            foreach (var item in result)
                            {
                                defaultCurrency = item.LocalCurrency;
                            }

                            Thread.Sleep(100);
                            sql = $"SELECT expenseType, SUM(expenseAmount) AS expenseAmount, currency FROM expenses WHERE userId={message.Chat.Id} GROUP BY expenseType, currency;";
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

                                foreach (var item in UsersList.FinalList)
                                {
                                    item.ExpenseAmount *= JsonOperations.PersonPersistent.Rates[defaultCurrency];
                                }

                                StringBuilder bld = new();
                                bld.Append($"Expense summary calculated in your default currency: {defaultCurrency}");
                                string resString = string.Empty;
                                foreach (var item in UsersList.FinalList)
                                {
                                    bld.Append($"\nTYPE: {item.ExpenseType} | AMMOUNT: {item.ExpenseAmount}");
                                }

                                resString = bld.ToString();
                                await botClient.SendTextMessageAsync(message.Chat, resString, cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "No records so far...", cancellationToken: cancellationToken);
                            }

                            return;
                        }

                        if (messageRecieved.Length >= 8 && messageRecieved[0..8] == "/history")
                        {
                            Thread.Sleep(100);
                            Console.WriteLine($"database path: {dbPath}.");
                            using var connection = new SQLiteConnection($"Data Source={dbPath}");
                            var sql = $"SELECT * FROM expenses WHERE userId = {message.Chat.Id};";
                            var result = connection.Query<Expenses>(sql).ToList();
                            if (result.Count > 0)
                            {
                                StringBuilder bld = new();
                                bld.Append("Prints out all the records history");
                                string resString = string.Empty;
                                foreach (var item in result)
                                {
                                    bld.Append($"\nID: {item.Id} | TYPE: {item.ExpenseType} | AMMOUNT: {item.ExpenseAmount} | COMMENT: {item.ExpenseComment} | TIME: {item.DateTime} | CURRENCY: {item.Currency}");
                                }

                                resString = bld.ToString();
                                await botClient.SendTextMessageAsync(message.Chat, resString, cancellationToken: cancellationToken);
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "no history yet...\nTo add new record use /new command", cancellationToken: cancellationToken);
                            }
                            return;
                        }

                        if (messageRecieved.Length >= 7 && messageRecieved[0..7] == "/delete")
                        {
                            if (messageRecieved.TrimEnd() == "/delete")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "To delete a record you must provide an ID of the record.\nUse /history command to get ID of a record to delete", cancellationToken: cancellationToken);
                            }
                            else
                            {
                                string[] messageParsed = messageRecieved.Split();
                                List<string> messages = new();
                                foreach (string item in messageParsed)
                                {
                                    if (!string.IsNullOrWhiteSpace(item))
                                    {
                                        messages.Add(item);
                                    }
                                }

                                if (messages.Count == 2)
                                {
                                    if (int.TryParse(messages[1], out int delId))
                                    {
                                        Console.WriteLine("Parsed successfully!");
                                    }

                                    Thread.Sleep(100);
                                    Console.WriteLine($"database path: {dbPath}.");
                                    using var connection = new SQLiteConnection($"Data Source={dbPath}");
                                    int tmpId = (int)message.Chat.Id;
                                    var sql = $"SELECT * FROM expenses WHERE userId = {tmpId} AND id = {delId};";
                                    var result = connection.Query<Expenses>(sql).ToList();
                                    if (result.Count > 0)
                                    {
                                        Thread.Sleep(100);
                                        Console.WriteLine($"database path: {dbPath}.");
                                        sql = $"DELETE FROM expenses WHERE userId = {tmpId} AND id = {delId};";
                                        int res = connection.Execute(sql);
                                        if (res > 0)
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, $"Record with ID {delId} deleted successfully", cancellationToken: cancellationToken);
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, $"failed to delete record with ID {delId}", cancellationToken: cancellationToken);
                                        }
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "no such id in your records", cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Wrong argument. Try again...", cancellationToken: cancellationToken);
                                }
                            }
                        }

                        if (messageRecieved.Length >= 9 && messageRecieved[0..9] == "/currency")
                        {
                            if (messageRecieved.TrimEnd() == "/currency")
                            {
                                Thread.Sleep(100);
                                Console.WriteLine($"database path: {dbPath}.");
                                using var connection = new SQLiteConnection($"Data Source={dbPath}");
                                var sql = $"SELECT localCurrency FROM users WHERE localUserId = {message.Chat.Id};";
                                var result = connection.Query<Users>(sql).ToList();
                                string defaultCurrency = string.Empty;
                                foreach (var item in result)
                                {
                                    defaultCurrency = item.LocalCurrency;
                                    await botClient.SendTextMessageAsync(message.Chat, $"Current default currency is: {defaultCurrency}\nTo change your default currency use the following syntax:\n/currency [your new currency]\nExample:\n/currency USD", cancellationToken: cancellationToken);
                                }
                            }
                            else
                            {
                                string[] messageParsed = messageRecieved.Split();
                                List<string> messages = new();
                                foreach (string item in messageParsed)
                                {
                                    if (!string.IsNullOrWhiteSpace(item))
                                    {
                                        messages.Add(item);
                                    }
                                }

                                if (messages.Count == 2)
                                {
                                    if (CurrencyKeys.Contains(messages[1].ToUpper()))
                                    {
                                        Thread.Sleep(100);
                                        Console.WriteLine($"database path: {dbPath}.");
                                        using var connection = new SQLiteConnection($"Data Source={dbPath}");
                                        Thread.Sleep(100);
                                        var sql = $"UPDATE users SET localCurrency = '{messages[1].ToUpper()}' WHERE localUserId = {message.Chat.Id};";
                                        Thread.Sleep(100);
                                        var result = connection.Execute(sql);
                                        Thread.Sleep(100);
                                        Console.WriteLine("result count: " + result);
                                        if (result > 0)
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, $"Your default currency updated", cancellationToken: cancellationToken);
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, $"Your default currency was not updated", cancellationToken: cancellationToken);
                                        }
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, $"{messages[1].ToUpper()} not found...", cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Wrong argument. Try again...", cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("User not registred");
                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Command not recognized. Try /help command", cancellationToken: cancellationToken);
                }
            }
        }

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

        static readonly HttpClient client = new HttpClient();

        private static async Task Main()
        {
            if (botToken is null)
            {
                throw new ArgumentException(nameof(botToken));
            }

            ITelegramBotClient bot = new TelegramBotClient(botToken);

            if (!CheckJson())
            {
                try
                {
                    string? ratesPath = Environment.GetEnvironmentVariable("exchangeRatesProviderPath");
                    string? app_id = Environment.GetEnvironmentVariable("exchangeAppId");
                    using HttpResponseMessage response = await client.GetAsync(ratesPath + app_id);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    CreateJson(responseBody);
                    MapJson();
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            else
            {
                MapJson();
            }

            Console.WriteLine("Bot client for \"" + bot.GetMeAsync().Result.FirstName + "\" bot started");
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}