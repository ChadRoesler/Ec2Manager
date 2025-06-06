﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ec2Manager.Views.Home
{
    public class ErrorModel : PageModel
    {
        public string ErrorMessage { get; private set; }

        public void OnGet()
        {
            ErrorMessage = Request.Cookies["Error"] ?? "Whoops Error On Error";
        }
    }
}
