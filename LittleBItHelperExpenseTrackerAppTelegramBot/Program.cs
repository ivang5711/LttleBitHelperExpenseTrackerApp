using Dapper;
using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using SQLitePCL;
using System.Data.SQLite;

namespace TelegramBotExperiments
{
    public class Expenses
    {
        public int Id { get; set; }
        public string ExpenseType { get; set; } = string.Empty;
        public int ExpenseAmount { get; set; }
        public string ExpenseComment { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public int UserId { get; set; }
        public string Currency { get; set; } = string.Empty;

        public Expenses()
        {
        }
    }

    public class Users
    {
        public string LocalCurrency { get; set; } = string.Empty;
        public int LocalUserId { get; set; }
        public string LocalUserName { get; set; } = string.Empty;
    }

    internal class Program
    {
        private static ITelegramBotClient bot = new TelegramBotClient("");

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                string messageRecieved = message.Text.ToLower();

                if (messageRecieved[0] == '/')
                {
                    if (messageRecieved.Length >= 6 && messageRecieved[0..6] == "/start")
                    {

                        Thread.Sleep(100);
                        string dbPath = "C:\\Users\\Smith\\source\\repos\\pet_projects\\LittleBitHelperExpenseTrackerApp\\LittleBitHelperExpenseTracker\\tracker-database.db";
                        Console.WriteLine($"database path: {dbPath}.");
                        using var connection = new SQLiteConnection($"Data Source={dbPath}");
                        var sql = $"SELECT localUserId FROM users;";
                        var result = connection.Query<Users>(sql).ToList();
                        Users a = new Users();
                        a.LocalUserId = (int)message.Chat.Id;
                        int flag = 0;
                        foreach(var item in result)
                        {
                            Console.WriteLine(item.LocalUserId);
                            Console.WriteLine(message.Chat.Id);
                            if (item.LocalUserId == message.Chat.Id)
                            {
                                Console.WriteLine("YES");
                                flag = 1;
                                break;
                            }
                        }
                        
                        Console.WriteLine("Finish");
                        Console.WriteLine(result.Count);
                        Console.WriteLine(a.LocalUserId);
                        Console.WriteLine(message.Chat.Id);
                        if (flag == 1)
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Dear {message.Chat.Username}\nHello and welcome to the LittleBitHelperBot!");
                            await botClient.SendTextMessageAsync(message.Chat, "A list of available commands:\n/start - shows this message\n/help - shows a list of available commands");

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Hello and welcome to the LittleBitHelperBot\nYour ID is:\n{message.Chat.Id}");
                        }
                        return;
                    }

                    if (messageRecieved.Length >= 5 && messageRecieved[0..5] == "/help")
                    {
                        await botClient.SendTextMessageAsync(message.Chat,
                            "A list of available commands:\n/start - shows welcome message\n/help - shows a list of available commands\n/new - add a new expense record.\nUseage: type amount comment.\nExample: taxi 350 to airport\n/all - provides a summ of all expenses for all time\n/typed - provides a list of types with total amount spent for each type\n/history - prints out all the records\n/delete - allows to deleta a record.\nUsage: specify a record id (history command can help to find an id).\nExample: 17\n/currency - allows to change the default currency.\nUsage: specify new currency.\nExample: USD\nYou can get the current currency value by sending the following command:\n/currency current");
                        return;
                    }


                    if (messageRecieved.Length >= 4 && messageRecieved == "/new")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Adds a new expense record");
                        return;
                    }

                    if (messageRecieved.Length >= 4 && messageRecieved == "/all")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Provides a sum of all expenses for all time");
                        return;
                    }

                    if (messageRecieved.Length >= 6 && messageRecieved == "/typed")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Provides a list of types with total amount spent for each type");
                        return;
                    }

                    if (messageRecieved.Length >= 8 && messageRecieved == "/history")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Prints out all the records");
                        return;
                    }

                    if (messageRecieved.Length >= 7 && messageRecieved == "/delete")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "To delete a record you must provide an ID of the record. ");
                        return;
                    }

                    if (messageRecieved.Length >= 9 && messageRecieved == "/currency")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Use this command to see and update the current default currency");
                        return;
                    }
                }
                await botClient.SendTextMessageAsync(message.Chat, "Command not recognized.Try again");
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

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