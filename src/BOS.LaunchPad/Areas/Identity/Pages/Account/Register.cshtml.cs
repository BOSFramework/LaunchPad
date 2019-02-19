﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BOS.LaunchPad.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        public string AccessToken { get; }

        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        private readonly IAuthClient _authClient;

        public RegisterModel(IConfiguration configuration,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IAuthClient authClient)
        {
            _authClient = authClient;
            _logger = logger;
            _emailSender = emailSender;
            AccessToken = configuration["BOSApiKey"];

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            try
            {
                returnUrl = returnUrl ?? Url.Content("~/");
                UserCreationInput UserData = new UserCreationInput();
                UserData.Username = Input.Email.ToString();
                UserData.Email = Input.Email.ToString();
                UserData.Password = Input.Password.ToString();
                UserData.PasswordConfirmation = Input.ConfirmPassword.ToString();
                var result = await _authClient.AddNewUserAsync<BOSUser>(Input.Email, Input.Email, Input.Password);
                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                return LocalRedirect(returnUrl);

            }
        }
    }
}
