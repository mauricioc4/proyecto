using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Ucaldas.Startup))]
namespace Ucaldas
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
