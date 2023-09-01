using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LittleBitHelperExpenseTracker.Models;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    public class PreferencesModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string MyProperty { get; set; }
        


        public PreferencesModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            Console.WriteLine("BASE HERE!!!: " + JsonOperations.PersonPersistent.Base);
        }

        public void OnPost()
        {

        }
    }
}