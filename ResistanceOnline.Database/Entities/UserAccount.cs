using Microsoft.AspNet.Identity.EntityFramework;
using System;

namespace ResistanceOnline.Database.Entities
{
	public class UserAccount : IdentityUser
	{
		public Guid PlayerGuid { get; set; }
	}
}