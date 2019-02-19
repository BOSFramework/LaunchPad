using System;
using System.Linq;
using System.Threading.Tasks;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using BOS.Auth.Client.Responses;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BOS.LaunchPad.Features.UserManagement
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IAuthClient _authClient;
        private readonly IEmailSender _emailSender;
        public UserManagementController(IAuthClient authClient, IEmailSender emailSender)
        {
            _authClient = authClient;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index()
        {
            var usersResponse = await _authClient.GetUsersAsync<BOSUser>();
            var model = new UserManagementOverviewViewModel { Users = usersResponse.Users };
            return View(model);
        }

        [HttpGet]
        public IActionResult AddUser()
        {
            return View();
        }

        public async Task<IActionResult> AddUser(AddUserViewModel model)
        {
            try
            {
                var response = await _authClient.AddNewUserAsync<BOSUser>(model.Email, model.Email, model.Password);

                if (response.IsSuccessStatusCode)
                {
                    await _emailSender.SendEmailAsync(
                                   model.Email,
                                   "Welcome to BOS",
                                   $"<h1>Welcome!</h1><hr /><p>Sign in with your username and password.</p><br /><p>Username: {model.Email}, Password: {model.Password}</p>");

                    return RedirectToAction("Index", "UserManagement");
                }

                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var userId = new Guid(id);
                var profile = await _authClient.GetUserByIdAsync<BOSUser>(userId);
                var rolesResponse = await _authClient.GetRolesAsync<BOSRole>();
                var assignedRolesResponse = await _authClient.GetUserRolesByUserIdAsync<BOSRole>(userId);
                var model = new EditUserViewModel
                {
                    Id = userId,
                    Email = profile.User.Email,
                    EmailRecord = profile.User.Email,
                    Username = profile.User.Username,
                    UsernameRecord = profile.User.Username,
                    AllRoles = rolesResponse.Roles
                };

                foreach (BOSRole r in rolesResponse.Roles)
                {
                    if (assignedRolesResponse.Roles.Any(ro => ro.Id == r.Id))
                    {
                        model.AssignedRoles.Add(r);
                    }
                }

                return View(model);
            }
            catch(Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            try
            {
                UpdateUserEmailResponse updateEmailResponse;
                UpdateUsernameResponse updateUsernameResponse;
                bool success = true;

                if (model.Email != model.EmailRecord)
                {
                    updateEmailResponse = await _authClient.UpdateUserEmailAsync(model.Id, model.Email);
                    success = updateEmailResponse == null ? true : updateEmailResponse.IsSuccessStatusCode;
                }

                if (success == true && model.Username != model.UsernameRecord)
                {
                    updateUsernameResponse = await _authClient.UpdateUsernameAsync(model.Id, model.Username);
                    success = updateUsernameResponse == null ? true : updateUsernameResponse.IsSuccessStatusCode;
                }

                if (success)
                {
                    return RedirectToAction("Edit", "UserManagement", new { id = model.Id });
                }

                return RedirectToAction("Index", "Error");
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> AddRole(string roleId, string userId)
        {
            try
            {
                var addRoleToUserResponse = await _authClient.AddRoleToUserAsync(new Guid(roleId), new Guid(userId));

                if (addRoleToUserResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Edit", "UserManagement", new { id = userId });
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> RevokeRole(string role, string user)
        {
            try
            {
                var roleId = new Guid(role);
                var userId = new Guid(user);

                var revokeRoleResponse = await _authClient.RevokeRoleAsync(roleId, userId);

                if (revokeRoleResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Edit", "UserManagement", new { id = user });
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> Delete(string userId)
        {
            try
            {
                var id = new Guid(userId);
                var deleteUserResponse = await _authClient.DeleteUserAsync(id);

                if (deleteUserResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "UserManagement");
                }
                return RedirectToAction("Index", "Error");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Error");
            }
        }

        public async Task<IActionResult> ChangePassword(EditUserViewModel model)
        {
            try
            {
                var updatePasswordResponse = await _authClient.ForcePasswordChangeAsync(model.Id, model.NewPassword);

                if (updatePasswordResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Edit", "UserManagement", new { id = model.Id });
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