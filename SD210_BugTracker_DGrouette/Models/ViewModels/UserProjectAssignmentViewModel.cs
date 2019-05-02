using System.Collections.Generic;

namespace SD210_BugTracker_DGrouette.Models
{
    public class UserProjectAssignmentViewModel
    {
        public string Title { get; set; }
        public int projectId { get; set; }
        public List<UserInProjectViewModel> Users { get; set; }
    }
}