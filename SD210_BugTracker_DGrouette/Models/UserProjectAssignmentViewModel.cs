using System.Collections.Generic;

namespace SD210_BugTracker_DGrouette.Models
{
    public class UserProjectAssignmentViewModel
    {
        public string Title { get; set; }
        public int Id { get; set; }
        public List<UserInProject> Users { get; set; }
    }
}