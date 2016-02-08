using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ArogyaParivar.Entity
{

    public class RolePrivileges
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string RolePrivilege { get; set; }
        public string Controler { get; set; }
        public string ActionMethod { get; set; }
        public string MenuText { get; set; }
    }

    public class GetUserPrivileges
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<RolePrivileges> RolePrives { get; set; }
        public string UserDescription { get; set; }
        public string UserEmail { get; set; }
        public string UserPhoneNumber { get; set; }
        public string ErrorMessage { get; set; }
        public bool UserExist { get; set; }
        public string IPAddress { get; set; }
        public string Display { get; set; }
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Error Message")]
        public string ErrorMessage { get; set; }
    }

}