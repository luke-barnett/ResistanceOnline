using ResistanceOnline.Database.Entities;
using System.Threading.Tasks;

namespace ResistanceOnline.Site.Infrastructure.Messaging
{
	public interface IMessageService
	{
		Task NotifyPlayerForAttention(string userId, string subject, string message, string gameId);
	}
}