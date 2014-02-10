using ResistanceOnline.Core;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	public class GameController : Controller
	{
        public ActionResult Index()
        {
            return View();
        }
	}
}