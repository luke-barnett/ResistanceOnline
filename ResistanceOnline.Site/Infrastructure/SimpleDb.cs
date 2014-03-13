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

        public List<Core.Action> GetActions(int gameId)
        {
            var actions = _context.Actions.Where(a=>a.GameId == gameId).ToList();
            return actions.Select(
                a=> new Core.Action(a.Owner, (Core.Action.Type)Enum.Parse(typeof(Core.Action.Type), a.Type), a.Text)
            ).ToList();
		}

        public int NextGameId()
        {
            var gameId = _context.Actions.Max(a => a.GameId) + 1;
            return gameId;
        }
    }
}