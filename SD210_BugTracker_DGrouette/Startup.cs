using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SD210_BugTracker_DGrouette.Startup))]
namespace SD210_BugTracker_DGrouette
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
