using System.Web;
using System.Web.Mvc;

namespace SD210_BugTracker_DGrouette
{
    public class FilterConfig
    {
        // Global errors are placed here.
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
