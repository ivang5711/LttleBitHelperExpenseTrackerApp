using Microsoft.CodeAnalysis.CSharp;
using System.Text.Json;
using static LittleBitHelperExpenseTracker.Models.JsonOperations;

namespace LittleBitHelperExpenseTracker.Models
{
    public static class JsonOperations
    {
        public static string JsonData { get; set; }

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
            public string Disclaimer { get; set; }
            public string License { get; set; }
            public int Timestamp { get; set; }
            public string Base { get; set; }
            public Dictionary<string, float> Rates { get; set; }

        }

        public static void SetJson()
        {
            JsonData = File.ReadAllText("data.json");


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Person person = JsonSerializer.Deserialize<Person>(JsonData, options);

            PersonPersistent.Disclaimer = person.Disclaimer;
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
    }
}