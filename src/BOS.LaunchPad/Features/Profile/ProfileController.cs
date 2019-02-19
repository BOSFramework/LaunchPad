using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Auth.Client.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BOS.LaunchPad.Features.Profile
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IAuthClient _authClient;

        public ProfileController(IAuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var userResponse = await _authClient.GetUserByIdAsync<BOSUser>(new Guid(userId));

            if (userResponse.IsSuccessStatusCode)
            {
                var user = userResponse.User;

                var model = new ProfileViewModel() { User = user, NewEmail = user.Email, NewUsername = user.Username };

                return View(model);
            }

            return RedirectToAction("Index", "Error");
        }

        public async Task<IActionResult> Edit(ProfileViewModel data)
        {
            try
            {
                UpdateUserEmailResponse updateEmailResponse;
                UpdateUsernameResponse updateUsernameResponse;
                bool success = true;

                if (data.User.Email != data.NewEmail)
                {
                    updateEmailResponse = await _authClient.UpdateUserEmailAsync(data.User.Id, data.NewEmail);
                    success = updateEmailResponse == null ? true : updateEmailResponse.IsSuccessStatusCode;
                }

                if (success == true && data.User.Username != data.NewUsername)
                {
                    updateUsernameResponse = await _authClient.UpdateUsernameAsync(data.User.Id, data.NewUsername);
                    success = updateUsernameResponse == null ? true : updateUsernameResponse.IsSuccessStatusCode;
                }

                if (success)
                {
                    return RedirectToAction("Index", "Profile");
                }

                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel data)
        {
            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var changePasswordReponse = await _authClient.ChangePasswordAsync(new Guid(userId), data.OldPassword, data.Password);

                if (changePasswordReponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Profile");
                }

                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }
    }
}