using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GuestListManagerWeb.Startup))]
namespace GuestListManagerWeb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
