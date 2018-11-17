using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Owin;

namespace SmartFLEET.Web
{
    public partial class Startup
    {
        public void ConfigureAuthentication(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
               // AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                CookieName = "smart-fleet",
                CookieHttpOnly = true,
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie

            });
            //app.UseStageMarker(DefaultAuthenticationTypes.ExternalCookie);
        }
    }
}