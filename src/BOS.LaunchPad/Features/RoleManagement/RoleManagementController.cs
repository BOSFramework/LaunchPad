using System;
using System.Threading.Tasks;
using BOS.Auth.Client;
using BOS.Auth.Client.ClientModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BOS.LaunchPad.Features.RoleManagement
{
    [Authorize(Roles = "Admin")]
    public class RoleManagementController : Controller
    {
        private readonly IAuthClient _authClient;
        public RoleManagementController(IAuthClient authClient)
        {
            _authClient = authClient;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var rolesResponse = await _authClient.GetRolesAsync<BOSRole>();
                var model = new RoleManagementViewModel
                {
                    Roles = rolesResponse.Roles,
                    NewRoleName = null
                };

                return View(model);
            }
            catch (Exception)
            {
                throw new Exception("Error while fetching Roles");
            }
        }
        public async Task<IActionResult> Remove(string roleId)
        {
            try
            {
                if (roleId != null)
                {
                    Guid _roleId = new Guid(roleId);
                    var deleteResponse = await _authClient.DeleteRoleAsync(_roleId);
                    var rolesResponse = await _authClient.GetRolesAsync<BOSRole>();
                    var model = new RoleManagementViewModel
                    {
                        Roles = rolesResponse.Roles,
                        NewRoleName = null
                    };
                    if (deleteResponse.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index", "RoleManagement");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Error");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Error");
                }

            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Error");
            }

        }

        public async Task<IActionResult> Add(RoleManagementViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.NewRoleName))
                {
                    return BadRequest();
                }
                var addRoleResponse = await _authClient.AddRoleAsync<BOSRole>(model.NewRoleName);

                if (addRoleResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "RoleManagement");
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