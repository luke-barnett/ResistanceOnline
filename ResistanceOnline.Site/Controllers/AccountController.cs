using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Ninject;
using ResistanceOnline.Database;
using ResistanceOnline.Site.Infrastructure;
using ResistanceOnline.Site.Models;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ResistanceOnline.Site.Controllers
{
	[Authorize]
	public class AccountController : Controller
	{
		readonly UserManager<UserAccount> _userManager;

		[Inject]
		public AccountController(ResistanceOnlineDbContext dbContext)
			: this(new UserManager<UserAccount>(new UserStore<UserAccount>(dbContext)))
		{
		}

		public AccountController(UserManager<UserAccount> userManager)
		{
			_userManager = userManager;
		}

		private IAuthenticationManager AuthenticationManager
		{
			get
			{
				return HttpContext.GetOwinContext().Authentication;
			}
		}

		[AllowAnonymous]
		public ActionResult Login()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public ActionResult Login(string provider)
		{
			// Request a redirect to the external login provider
			return new ChallengeResult(provider, Url.Action("LoginCallback", "Account"));
		}

		public ActionResult Logout()
		{
			AuthenticationManager.SignOut();
			
			return Redirect();
		}

		[AllowAnonymous]
		public async Task<ActionResult> LoginCallback()
		{
			var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
			if (loginInfo == null)
			{
				return RedirectToAction("Login");
			}

			// Sign in the user with this external login provider if the user already has a login
			var user = await _userManager.FindAsync(loginInfo.Login);
			if (user != null)
			{
				await SignInAsync(user);
				return Redirect();
			}
			else
			{
				// If the user does not have an account, then prompt the user to create an account
				ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
				return View("LoginConfirmation", new LoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
			}
		}

		[HttpPost]
		[AllowAnonymous]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> LoginConfirmation(LoginConfirmationViewModel model, string returnUrl)
		{
			if (User.Identity.IsAuthenticated)
			{
				return Redirect();
				//return RedirectToAction("Manage");
			}

			if (ModelState.IsValid)
			{
				// Get the information about the user from the external login provider
				var info = await AuthenticationManager.GetExternalLoginInfoAsync();
				if (info == null)
				{
					return View("LoginFailure");
				}
				var user = new UserAccount { UserName = model.UserName, PlayerGuid = Guid.NewGuid() };
				var result = await _userManager.CreateAsync(user);
				if (result.Succeeded)
				{
					result = await _userManager.AddLoginAsync(user.Id, info.Login);
					if (result.Succeeded)
					{
						await SignInAsync(user);
						return Redirect();
					}
				}
				AddErrors(result);
			}

			ViewBag.ReturnUrl = returnUrl;
			return View(model);
		}

		[AllowAnonymous]
		public ActionResult LoginFailure()
		{
			return View();
		}

		private async Task SignInAsync(UserAccount user)
		{
			AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
			var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
			AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
		}

		private ActionResult Redirect()
		{
			return RedirectToAction("index", "game");
		}

		private void AddErrors(IdentityResult result)
		{
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error);
			}
		}
	}
}