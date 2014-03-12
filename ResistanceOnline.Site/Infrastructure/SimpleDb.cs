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

        public List<Core.Action> GetActionsForGame(int? gameId)
        {
            return _context.Actions.Where(a => a.GameId == gameId).ToList().Select(db => new Core.Action(db)).ToList();
        }

        public Core.Game GetGame(int gameId)
        {
            return new Core.Game(GetActionsForGame(gameId));
        }

        internal Core.Game CreateGame(Guid owner, string name)
        {
            var gameId = _context.Actions.Max(a => a.GameId) + 1;
            var action = new Core.Action(owner, Core.Action.Type.Join, name);
            AddAction(action);
            return GetGame(gameId);
        }
    }
}