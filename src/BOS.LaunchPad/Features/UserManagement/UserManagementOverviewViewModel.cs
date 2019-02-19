using BOS.Auth.Client.ClientModels;
using BOS.LaunchPad.Models;
using System.Collections.Generic;

namespace BOS.LaunchPad.Features.UserManagement
{
    public class UserManagementOverviewViewModel
    {
        public List<BOSUser> Users { get; set; }
        public UserCreationInput NewUser { get; set; }
    }
}
