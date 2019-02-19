using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BOS.LaunchPad.HttpClients;
using BOS.LaunchPad.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BOS.LaunchPad.Features.Home
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IIAHttpClient _iaClient;

        public HomeController(IIAHttpClient iaClient)
        {
            _iaClient = iaClient;
        }

        public async Task<IActionResult> Index()
        {
            var modules = await _iaClient.GetModulesAsync();
            var userPermissionsSets = await GetUserPermissionsSets();

            var model = new HomeViewModel
            {
                Id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value,
                Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email).Value,
                Username = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name).Value,
                Modules = modules,
                Permissions = userPermissionsSets,
                IsAdmin = User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.ToLower().Trim() == "admin")
            };
            return View(model);
        }

        private async Task<List<PermissionsSet>> GetUserPermissionsSets()
        {
            List<PermissionsSet> allPermSets = new List<PermissionsSet>();

            foreach (var claim in User.Claims.Where(c => c.Type == ClaimTypes.Role))
            {
                var rolePerms = await _iaClient.GetPermissionsForOwner(new Guid(claim.Properties["roleId"]));

                allPermSets.AddRange(rolePerms);
            }
            return allPermSets;
        }
    }
}