﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LittleBitHelperExpenseTracker.Pages
{
    [Authorize]
    [AllowAnonymous]

    public class PrivacyModel : PageModel
    {

        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogDebug("Privacy page loaded");
        }
    }
}