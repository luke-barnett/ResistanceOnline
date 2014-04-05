using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class GameController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}
	}
}