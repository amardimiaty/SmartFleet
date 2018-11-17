using Owin;

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
