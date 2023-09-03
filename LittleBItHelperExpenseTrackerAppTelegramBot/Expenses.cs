namespace TelegramBotExperiments
{
    public class Expenses
    {
        public int Id { get; set; }
        public string ExpenseType { get; set; } = string.Empty;
        public float ExpenseAmount { get; set; }
        public string ExpenseComment { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public int UserId { get; set; }
        public string Currency { get; set; } = string.Empty;

        public Expenses()
        {
        }
    }
}