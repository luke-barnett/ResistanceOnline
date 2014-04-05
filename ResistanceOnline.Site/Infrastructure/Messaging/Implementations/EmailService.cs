using Microsoft.AspNet.Identity;
using Postal;
using ResistanceOnline.Database.Entities;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

			var service = new Postal.EmailService(System.Web.Mvc.ViewEngines.Engines, () =>
			{
				if (string.IsNullOrWhiteSpace(SMTPServer) || string.IsNullOrWhiteSpace(SMTPUsername) || string.IsNullOrWhiteSpace(SMTPPassword) || string.IsNullOrWhiteSpace(SMTPPort))
				{
					return new SmtpClient();
				}

				return new SmtpClient(SMTPServer, int.Parse(SMTPPort))
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(SMTPUsername, SMTPPassword)
				};
			});

			await service.SendAsync(email);
		}

		string SMTPServer
		{
			get
			{
				return ConfigurationManager.AppSettings["SMTPServer"];
			}
		}

		string SMTPPort
		{
			get
			{
				return ConfigurationManager.AppSettings["SMTPPort"];
			}
		}

		string SMTPUsername
		{
			get
			{
				return ConfigurationManager.AppSettings["SMTPUsername"];
			}
		}

		string SMTPPassword
		{
			get
			{
				return ConfigurationManager.AppSettings["SMTPPassword"];
			}
		}
	}
}