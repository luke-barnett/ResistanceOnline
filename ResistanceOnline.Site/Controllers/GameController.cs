﻿using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using ResistanceOnline.Core;
using ResistanceOnline.Database;
using ResistanceOnline.Database.Entities;
using ResistanceOnline.Site.ComputerPlayers;
using ResistanceOnline.Site.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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