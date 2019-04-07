using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public static class ProjectHelper
    {
        public static bool UserIsAdminOrManager(IPrincipal user)
        {
            if (user.IsInRole(ProjectConstants.AdminRole) || user.IsInRole(ProjectConstants.ManagerRole))
                return true;
            else
                return false;
        }
    }
}