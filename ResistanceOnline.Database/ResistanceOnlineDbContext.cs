using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace ResistanceOnline.Database
{
	public class ResistanceOnlineDbContext : IdentityDbContext<UserAccount>
	{
		public ResistanceOnlineDbContext()
			: base("ResistanceOnline")
		{
		}
	}
}