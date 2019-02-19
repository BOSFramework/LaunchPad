﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using BOS.LaunchPad.Models;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using System;
using BOS.LaunchPad.HttpClients;

namespace BOS.LaunchPad.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly IAuthClient _authClient;
        private readonly IIAHttpClient _iaClient;

        public LoginModel(ILogger<LoginModel> logger,
            IAuthClient authClient, IIAHttpClient iaClient)
        {
            _logger = logger;
            _authClient = authClient;
            _iaClient = iaClient;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _authClient.SignInAsync(Input.Username, Input.Password);
                if (result.IsVerified)
                {
                    _logger.LogInformation("User logged in.");

                    var rolesResponse = await _authClient.GetUserRolesByUserIdAsync<BOSRole>(result.UserId.Value).ConfigureAwait(false);

                    // Setting up claims for all the relevant information.
                    // The following sections set up:
                    // 1. The user claims, such as username and email,
                    // 2. A Role Claim for each role assosciated with the user
                    // 3. For each role, the permissions set for that role is retrieved,
                    //      and then role claims are created with the relevant codes.
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, result.UserId.ToString()),
                        new Claim(ClaimTypes.Email, result.Email),
                        new Claim(ClaimTypes.Name, result.Username)
                    };

                    foreach (BOSRole r in rolesResponse.Roles)
                    {
                        var roleClaim = new Claim(ClaimTypes.Role, r.Name);
                        roleClaim.Properties.Add("roleId", r.Id.ToString());

                        claims.Add(roleClaim);
                    }

                    foreach (var role in rolesResponse.Roles)
                    {
                        var permSets = await _iaClient.GetPermissionsForOwner(role.Id);
                        AddModuleCodeClaims(claims, permSets);
                        AddOperationsPermissionsClaims(claims, permSets);
                    }

                    var claimsIdentity = new ClaimsIdentity(
                                           claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        RedirectUri = returnUrl,
                        IssuedUtc = DateTime.UtcNow,
                        ExpiresUtc = Input.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(15),
                        IsPersistent = true
                    };

                    await HttpContext.SignInAsync(
                                           CookieAuthenticationDefaults.AuthenticationScheme,
                                           new ClaimsPrincipal(claimsIdentity),
                                           authProperties);

                    return LocalRedirect(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return Page();
                }
            }
            return Page();
        }

        private void AddOperationsPermissionsClaims(List<Claim> claims, List<PermissionsSet> permSets)
        {
            foreach (var pSet in permSets)
            {
                foreach (var op in pSet.Permissions)
                {
                    if (!claims.Any(c => c.Value == op.Code))
                    {
                        claims.Add(new Claim("ia_code", op.Code));
                    }
                }
            }
        }

        private void AddModuleCodeClaims(List<Claim> claims, List<PermissionsSet> permSets)
        {
            foreach (var pSet in permSets)
            {
                var moduleClaim = new Claim("ia_code", pSet.Code);

                if (!claims.Any(c => c.Value == pSet.Code))
                {
                    claims.Add(moduleClaim);
                }
            }
        }
    }
}
