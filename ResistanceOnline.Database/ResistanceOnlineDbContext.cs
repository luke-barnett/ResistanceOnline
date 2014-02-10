using Microsoft.AspNet.Identity.EntityFramework;
using Ninject;
using System.Data.Entity;

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