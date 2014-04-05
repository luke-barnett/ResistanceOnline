using ResistanceOnline.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResistanceOnline.Site.Infrastructure
{
    public class SimpleDb
    {
        ResistanceOnlineDbContext _context;

        public SimpleDb(ResistanceOnlineDbContext dbContext)
        {
            _context = dbContext;
        }

        public void AddAction(Core.Action action)
        {
            //debug game is always in memory
            if (action.GameId == 0)
                return;

            var actionDb = new Database.Entities.Action
            {
                GameId = action.GameId,
                Owner = action.Owner,
                Timestamp = action.Timestamp,
                Type = action.ActionType.ToString(),
                Text = action.Text
            };

            _context.Actions.Add(actionDb);
            _context.SaveChanges();
        }

        public void DeleteActions(int gameId)
        {
            //debug game is always in memory
            if (gameId == 0)
                return;

            _context.Actions.RemoveRange(_context.Actions.Where(a => a.GameId == gameId));
            _context.SaveChanges();
        }

        public List<Core.Action> GetActions(int gameId)
        {
            //debug game is always in memory
            if (gameId == 0)
                return new List<Core.Action>();

            var actions = _context.Actions.Where(a=>a.GameId == gameId).ToList();
            return actions.Select(
                a=> new Core.Action(gameId, a.Owner, (Core.Action.Type)Enum.Parse(typeof(Core.Action.Type), a.Type), a.Text)
            ).OrderBy(a=>a.Timestamp).ToList();
		}

        public int NextGameId()
        {
            var gameId = (_context.Actions.Max(a => (int?)a.GameId)??0) + 1;
            return gameId;
        }

        public List<int> GameIds()
        {
            return _context.Actions.Where(a=>a.GameId!=0).Select(a => a.GameId).Distinct().ToList();
        }

        public Database.Entities.UserAccount GetUser(string userId)
        {
            return _context.Users.FirstOrDefault(user => user.Id == userId);
        }

		public Database.Entities.UserAccount GetUser(Guid playerGuid)
		{
			return _context.Users.FirstOrDefault(user => user.PlayerGuid == playerGuid);
		}
    }
}