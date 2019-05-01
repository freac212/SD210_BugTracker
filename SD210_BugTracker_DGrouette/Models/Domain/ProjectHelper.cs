using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace SD210_BugTracker_DGrouette.Models.Domain
{
    public static class ProjectHelper
    {
        public static List<string> GetRoleList(IPrincipal user)
        {
            var userRoles = new List<string>();

            if (user.IsInRole(ProjectConstants.SubmitterRole))
            {
                userRoles.Add(ProjectConstants.SubmitterRole);
            }
            if (user.IsInRole(ProjectConstants.DeveloperRole))
            {
                userRoles.Add(ProjectConstants.DeveloperRole);
            }
            if (user.IsInRole(ProjectConstants.ManagerRole))
            {
                userRoles.Add(ProjectConstants.ManagerRole);
            }
            if (user.IsInRole(ProjectConstants.AdminRole))
            {
                userRoles.Add(ProjectConstants.AdminRole);
            }

            return userRoles;
        }

        public static bool IsAdminOrManager(IPrincipal user)
        {
            // Could use ternaries here eh?
            if (user.IsInRole(ProjectConstants.AdminRole) || user.IsInRole(ProjectConstants.ManagerRole))
                return true;
            else
                return false;
        }

        public static bool IsDevOrSubmitter(IPrincipal user)
        {
            if (user.IsInRole(ProjectConstants.DeveloperRole) || user.IsInRole(ProjectConstants.SubmitterRole))
                return true;
            else
                return false;
        }

        public static Project GetProjectById(ApplicationDbContext dbContext, int projectId)
        {
            return dbContext.Projects
                .Where(p => !p.IsArchived) // Ensures we don't grab archived projects
                .FirstOrDefault(proj => proj.Id == projectId);
        }
    }
}