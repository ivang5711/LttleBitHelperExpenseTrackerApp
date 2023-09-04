using System.Text.Json;

namespace LittleBitHelperExpenseTracker.Models
{
    public static class JsonOperations
    {
        public static string JsonData { get; set; } = string.Empty;
        public static string? JsonPath { get; set; } = Environment.GetEnvironmentVariable("jsonPath");

        static readonly HttpClient client = new();

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
            if (JsonPath == null)
            {
                throw new ArgumentException(nameof(JsonPath));
            }

            JsonData = File.ReadAllText(JsonPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person? person = JsonSerializer.Deserialize<Person>(JsonData, options);
            if (person is null)
            {
                Console.WriteLine("JSON file can not be mapped");
                return;
            }

            PersonPersistent.Disclaimer = person.Disclaimer;
            PersonPersistent.License = person.License;
            PersonPersistent.Timestamp = person.Timestamp;
            PersonPersistent.Base = person.Base;
            PersonPersistent.Rates = person.Rates;
        }

        public static void CreateJson(string input)
        {
            if (JsonPath == null)
            {
                throw new ArgumentException(nameof(JsonPath));
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person? person = JsonSerializer.Deserialize<Person>(input, options);
            if (person != null)
            {

                File.WriteAllText(JsonPath, input);
                Console.WriteLine("JSON file updated!!!");
            }
            else
            {
                Console.WriteLine("Something went wrong with the json file creation...");
            }
        }

        public static bool CheckJson()
        {
            if (File.Exists(JsonPath))
            {
                Console.WriteLine("File exists!");
                JsonData = File.ReadAllText(JsonPath);
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

        public static async Task JsonCheckAndUpdate()
        {
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
        }
    }
}