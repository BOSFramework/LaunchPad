using BOS.Auth.Client.ClientModels;
using System.Collections.Generic;

namespace BOS.LaunchPad.Features.RoleManagement
{
    public class RoleManagementViewModel
    {
        public List<BOSRole> Roles { get; set; }    //list to fetch existing roles
        public string NewRoleName { get; set; }   //to add new role name
    }
}
