using BOS.Auth.Client.ClientModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.Features.Profile
{
    public class ProfileViewModel
    {
        public BOSUser User { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string NewEmail { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [Display(Name = "Username")]
        public string NewUsername { get; set; }
    }
}
