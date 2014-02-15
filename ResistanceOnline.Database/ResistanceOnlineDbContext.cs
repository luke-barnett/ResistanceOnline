using Microsoft.AspNet.Identity.EntityFramework;
using Ninject;
using ResistanceOnline.Database.Entities;

namespace ResistanceOnline.Database
{
	public class ResistanceOnlineDbContext : IdentityDbContext<UserAccount>
	{
		[Inject]
		public ResistanceOnlineDbContext()
			: base("ResistanceOnline")
		{
		}
	}
}