using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.SignalR;
using Owin;

[assembly: OwinStartup(typeof(ResistanceOnline.Site.Startup))]
namespace ResistanceOnline.Site
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			ConfigureAuth(app);
		}

		public void ConfigureAuth(IAppBuilder app)
		{
			app.UseCookieAuthentication(new CookieAuthenticationOptions
			{
				AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
				LoginPath = new PathString("/Account/Login")
			});

            app.MapSignalR();

			app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

			app.UseGoogleAuthentication();
		}
	}
}