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

        public static string? JsonPath { get; set; }

        static readonly HttpClient client = new();

        public static class ExchangeRatePersistent
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

        public static void MapJson(string jsonPath)
        {
            JsonPath = jsonPath;
            var watch = System.Diagnostics.Stopwatch.StartNew();
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
                watch.Stop();
                _logger.LogDebug("Map Json total execution time is {time} milliseconds", watch.ElapsedMilliseconds);
                return;
            }

            ExchangeRatePersistent.Disclaimer = person.Disclaimer;
            ExchangeRatePersistent.License = person.License;
            ExchangeRatePersistent.Timestamp = person.Timestamp;
            ExchangeRatePersistent.Base = person.Base;
            ExchangeRatePersistent.Rates = person.Rates;
            _logger.LogInformation("Json file mapped successfully");
            watch.Stop();
            _logger.LogDebug("Map Json total execution time is {time} milliseconds", watch.ElapsedMilliseconds);
        }

        public static void CreateJson(string input, string jsonPath)
        {
            JsonPath = jsonPath;
            var watch = System.Diagnostics.Stopwatch.StartNew();
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
                File.WriteAllText(JsonPath, input);
                _logger.LogInformation("JSON file updated");
            }
            else
            {
                _logger.LogError("Json file creation failure");
            }

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            _logger.LogDebug("Create Json total execution time is {time} milliseconds", elapsedMs);
        }

        public static bool CheckJson(string jsonPath)
        {
            JsonPath = jsonPath;
            var watch = System.Diagnostics.Stopwatch.StartNew();
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
                        watch.Stop();
                        _logger.LogDebug("Check Json total execution time is {time} milliseconds", watch.ElapsedMilliseconds);
                        return true;
                    }
                }
                else
                {
                    _logger.LogInformation("Json file is not up to date");
                    watch.Stop();
                    _logger.LogDebug("Check Json total execution time is {time} milliseconds", watch.ElapsedMilliseconds);
                    return false;
                }
            }

            _logger.LogInformation("Json file does not exist");
            watch.Stop();
            _logger.LogDebug("Check Json total execution time is {time} milliseconds", watch.ElapsedMilliseconds);
            return false;
        }

        public static async Task JsonCheckAndUpdate(string jsonPath)
        {
            JsonPath = jsonPath;
            if (!CheckJson(JsonPath))
            {
                try
                {
                    string? ratesPath = Environment.GetEnvironmentVariable("exchangeRatesProviderPath");
                    string? app_id = Environment.GetEnvironmentVariable("exchangeAppId");
                    using HttpResponseMessage response = await client.GetAsync(ratesPath + app_id);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    CreateJson(responseBody, JsonPath);
                    MapJson(JsonPath);
                }
                catch (HttpRequestException e)
                {
                    _logger.LogDebug("Exception Caught! Message :{exception} ", e.Message);
                }
            }
            else
            {
                MapJson(JsonPath);
            }
        }
    }
}