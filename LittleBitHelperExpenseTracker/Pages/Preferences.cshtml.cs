using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LittleBitHelperExpenseTracker.Pages
{
    public class PreferencesModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public PreferencesModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}