using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LittleBitHelperExpenseTracker.Pages
{
    public class DeleteModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public DeleteModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}