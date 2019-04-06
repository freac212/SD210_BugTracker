using Microsoft.AspNet.Identity.EntityFramework;
using SD210_BugTracker_DGrouette.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models
{
    public class RoleEditorViewModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public List<SelectedRoles> RolesUserIsIn { get; set; }
        // This list of roles is here so I can use the same view model for both viewing and editing, even though this
        // list isn't sent while viewing the roles.
        public List<IdentityRole> Roles { get; set; }

    }
}