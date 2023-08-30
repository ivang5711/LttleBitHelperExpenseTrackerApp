using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LittleBitHelperExpenseTracker.Pages
{
    public class DetailsModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public DetailsModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}