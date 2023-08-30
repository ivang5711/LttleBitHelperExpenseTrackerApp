using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LittleBitHelperExpenseTracker.Pages
{
    public class CreateModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public CreateModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}