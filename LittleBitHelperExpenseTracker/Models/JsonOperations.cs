using System.Text.Json;

namespace LittleBitHelperExpenseTracker.Models
{
    public static class JsonOperations
    {
        private static readonly ILogger _logger = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                .AddConsole();
        }).CreateLogger<Program>();

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
                _logger.LogError("JsonPath is null. Time: {Time}", DateTime.UtcNow);
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
                _logger.LogError("JSON file can not be mapped");
                return;
            }

            PersonPersistent.Disclaimer = person.Disclaimer;
            PersonPersistent.License = person.License;
            PersonPersistent.Timestamp = person.Timestamp;
            PersonPersistent.Base = person.Base;
            PersonPersistent.Rates = person.Rates;
            _logger.LogInformation("Json file mapped successfully");
        }

        public static void CreateJson(string input)
        {
            if (JsonPath == null)
            {
                _logger.LogError("JsonPath is null. Time: {Time}", DateTime.UtcNow);
                throw new ArgumentException(nameof(JsonPath));
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person? person = JsonSerializer.Deserialize<Person>(input, options);
            if (person != null)
            {
                _logger.LogError("person is null. Time: {Time}", DateTime.UtcNow);
                File.WriteAllText(JsonPath, input);
                _logger.LogInformation("JSON file updated");
            }
            else
            {
                _logger.LogError("Json file creation failure");
            }
        }

        public static bool CheckJson()
        {
            if (File.Exists(JsonPath))
            {
                _logger.LogInformation("Json file exists");
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
                        _logger.LogInformation("Json file is up to date");
                        return true;
                    }
                }
                else
                {
                    _logger.LogInformation("Json file is not up to date");
                }
            }

            _logger.LogInformation("Json file does not exist");
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
                    _logger.LogDebug("Exception Caught! Message :{exception} ", e.Message);
                }
            }
            else
            {
                MapJson();
            }
        }
    }
}