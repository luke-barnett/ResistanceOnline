using Microsoft.AspNet.Identity;
using Postal;
using ResistanceOnline.Database.Entities;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ResistanceOnline.Site.Infrastructure.Messaging.Implementations
{
	public class EmailService : IMessageService
	{
		readonly UserManager<UserAccount> _userManager;

		public EmailService(UserManager<UserAccount> userManager)
		{
			_userManager = userManager;
		}

		public async Task NotifyPlayerForAttention(string userId, string subject, string message)
		{
			var userClaims = await _userManager.GetClaimsAsync(userId);

			var emailClaim = userClaims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);

			if (emailClaim == null)
				return;

			dynamic email = new Email("HailUser");
			email.To = emailClaim.Value;
			email.Subject = subject;
			email.Message = message;
			email.Send();
		}
	}
}