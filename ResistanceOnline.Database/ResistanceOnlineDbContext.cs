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

        public DbSet<Game> Games { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<LoyaltyCard> LoyaltyCards { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<Action> Actions { get; set; }
    }
}