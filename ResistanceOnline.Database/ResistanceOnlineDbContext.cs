using Microsoft.AspNet.Identity.EntityFramework;
using Ninject;
using ResistanceOnline.Database.Entities;
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

        public DbSet<Action> Actions { get; set; }
    }
}