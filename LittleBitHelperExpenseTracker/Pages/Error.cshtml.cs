using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace LittleBitHelperExpenseTracker.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }

        private readonly ILogger<ErrorModel> _logger;
        public static int OriginalStatusCode { get; set; }

        public string? OriginalPathAndQuery { get; set; }

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogDebug("Error page loaded");
            OriginalStatusCode = statusCode;
            var statusCodeReExecuteFeature =
                HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusCodeReExecuteFeature is not null)
            {
                OriginalPathAndQuery = string.Join(
                    statusCodeReExecuteFeature.OriginalPathBase,
                    statusCodeReExecuteFeature.OriginalPath,
                    statusCodeReExecuteFeature.OriginalQueryString);
            }
        }
    }
}