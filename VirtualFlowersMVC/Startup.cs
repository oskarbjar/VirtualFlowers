using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(VirtualFlowersMVC.Startup))]
namespace VirtualFlowersMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
