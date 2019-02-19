using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BOS.LaunchPad.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly IEmailSender _emailSender;
        private readonly IAuthClient _authClient;

        public ForgotPasswordModel(IAuthClient authClient, IEmailSender emailSender)
        {
            _emailSender = emailSender;
            _authClient = authClient;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var userResponse = await _authClient.GetUserByEmailAsync<BOSUser>(Input.Email);

                if (userResponse.User == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var slugResponse = await _authClient.CreateSlugAsync(Input.Email);
                string url = $"http://localhost:50705/ResetPassword/";

                if (slugResponse.IsSuccessStatusCode)
                {
                    await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(url + slugResponse)}'>clicking here</a>.");

                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                RedirectToPage("../Error");
            }

            return Page();
        }
    }
}
