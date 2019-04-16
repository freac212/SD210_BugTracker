using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class UsersAndRolesViewModel
    {
        public List<RoleEditorViewModel> UserModels { get; set; }

        public List<IdentityRole> Roles { get; set; }
    }
}