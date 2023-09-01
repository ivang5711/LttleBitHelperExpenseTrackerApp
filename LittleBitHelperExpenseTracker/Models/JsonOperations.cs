using System.Text.Json;

namespace LittleBitHelperExpenseTracker.Models
{
    public static class JsonOperations
    {
        public static string JsonData { get; set; } = string.Empty;

        public static class PersonPersistent
        {
            public static string Disclaimer { get; set; } = string.Empty;
            public static string License { get; set; } = string.Empty;
            public static int Timestamp { get; set; } = 0;
            public static string Base { get; set; } = string.Empty;
            public static Dictionary<string, float> Rates { get; set; } = new Dictionary<string, float>();
        }

        public class Person
        {
            public required string Disclaimer { get; set; }
            public required string License { get; set; }
            public int Timestamp { get; set; }
            public required string Base { get; set; }
            public required Dictionary<string, float> Rates { get; set; }

        }

        public static void MapJson()
        {
            JsonData = File.ReadAllText("data.json");
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person? person = JsonSerializer.Deserialize<Person>(JsonData, options);
            PersonPersistent.Disclaimer = person!.Disclaimer;
            PersonPersistent.License = person.License;
            PersonPersistent.Timestamp = person.Timestamp;
            PersonPersistent.Base = person.Base;
            PersonPersistent.Rates = person.Rates;
            Console.WriteLine("Disclaimer: " + PersonPersistent.Disclaimer);
            Console.WriteLine("License: " + PersonPersistent.License);
            Console.WriteLine("Timestamp: " + PersonPersistent.Timestamp);
            Console.WriteLine("Base: " + PersonPersistent.Base);
            Console.WriteLine("JSON is HERE!!!");

        }

        public static void CreateJson(string input)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person? person = JsonSerializer.Deserialize<Person>(input, options);
            if (person != null)
            {
                File.WriteAllText("data.json", input);
                Console.WriteLine("JSON file updated!!!");
            }
            else
            {
                Console.WriteLine("Something went wrong with the json file creation...");
            }
        }

        public static bool CheckJson()
        {
            if (File.Exists("data.json"))
            {
                Console.WriteLine("File exists!");
                JsonData = File.ReadAllText("data.json");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                Person? person = JsonSerializer.Deserialize<Person>(JsonData, options);
                if (person != null)
                {
                    double timeStampJson = person.Timestamp;
                    DateTime dateTime = DateTime.UnixEpoch;
                    dateTime = dateTime.AddSeconds(timeStampJson);
                    Console.WriteLine("Json is up to date: " + (dateTime.Date == DateTime.UtcNow.Date));
                    Console.WriteLine(DateTime.UtcNow.Date);
                    Console.WriteLine(dateTime.Date);
                    if (dateTime.Date == DateTime.UtcNow.Date)
                    {
                        Console.WriteLine("Json is up to date!");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Json is not up to date!");
                }
            }

            Console.WriteLine("File does not exist");
            return false;
        }
    }
}