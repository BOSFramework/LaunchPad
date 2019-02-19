using BOS.Auth.Client.ClientModels;
using BOS.LaunchPad.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.UserManagement
{
    public class EditUserViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string UsernameRecord { get; set; } //used for comparison on update button click to only update if the value changed

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string EmailRecord { get; set; } //used for comparison on update button click to only update if the value changed
        public List<BOSRole> Roles { get; set; }
        public List<BOSRole> AssignedRoles { get; set; }
        public List<BOSRole> AllRoles { get; set; }
        public string Password { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        public EditUserViewModel()
        {
            AssignedRoles = new List<BOSRole>();
            AllRoles = new List<BOSRole>();
        }
    }
}
