using System.Web.Mvc;
using System.Web.Routing;

namespace ResistanceOnline.Site
{
	public class RouteConfig
	{
		public static void RegisterRoutes(RouteCollection routes)
		{
			routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
			
			routes.MapRoute(
				name: "login",
				url: "login",
				defaults: new { controller = "Account", action = "Login" }
				);

			routes.MapRoute(
					name: "logout",
					url: "logout",
					defaults: new { controller = "Account", action = "Logout" }
				);

			routes.MapRoute(
				name: "account",
				url: "account/{action}",
				defaults: new { controller = "Account" }
				);

			routes.MapRoute(
				name: "game",
				url: "{action}/{gameid}",
				defaults: new { controller = "Game", action = "Index", gameid = UrlParameter.Optional }
			);
		}
	}
}