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
			Bind<ResistanceOnlineDbContext>().ToSelf();
			Bind<UserManager<UserAccount>>().ToSelf();
			Bind<IUserStore<UserAccount>>().ToConstructor<UserStore<UserAccount>>(args => new UserStore<UserAccount>(args.Inject<ResistanceOnlineDbContext>()));
		}
	}
}