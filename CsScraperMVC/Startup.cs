using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CsScraperMVC.Startup))]
namespace CsScraperMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
