using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(SmartFLEET.Web.Startup))]
namespace SmartFLEET.Web
{

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuthentication(app);

            app.MapSignalR();
        }

    
    }
}
