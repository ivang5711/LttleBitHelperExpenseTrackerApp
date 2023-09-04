namespace LittleBitHelperExpenseTracker.Models
{
    public class Expenses
    {
        public string Currency { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public float ExpenseAmount { get; set; }
        public string ExpenseComment { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
