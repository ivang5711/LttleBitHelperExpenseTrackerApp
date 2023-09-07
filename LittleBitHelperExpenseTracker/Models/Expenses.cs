namespace LittleBitHelperExpenseTracker.Models
{
    public class Expenses
    {
        public string? Currency { get; set; }
        public DateTime DateTime { get; set; }
        public float ExpenseAmount { get; set; }
        public string? ExpenseComment { get; set; }
        public string? ExpenseType { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
    }
}
