using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Ninject.Modules;
using Ninject;

namespace ResistanceOnline.Database
{
	public class InjectionModule : NinjectModule
	{
		public override void Load()
		{
			Bind<ResistanceOnlineDbContext>().ToSelf().InTransientScope();
			Bind<System.Data.Entity.DbContext>().To<ResistanceOnlineDbContext>().InTransientScope();
			Bind<UserManager<UserAccount>>().ToSelf();
			Bind<IUserStore<UserAccount>>().To<UserStore<UserAccount>>();
		}
	}
}