﻿using ResistanceOnline.Database;
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

        public void AddAction(int gameId, Core.Action action)
        {
            action.GameId = gameId;

            var actionDb = new Database.Entities.Action
            {
                Game = new Database.Entities.Game { GameId = gameId },
                Owner = _context.Players.First(x => x.Game.GameId == gameId && x.Guid == action.Owner.Guid),
                Timestamp = action.Timestamp,
                Type = action.ActionType.ToString(),
                Text = action.Text
            };
            if (action.TargetPlayer != null)
            {
                actionDb.Target = _context.Players.FirstOrDefault(x => x.Game.GameId == gameId && x.Guid == action.TargetPlayer.Guid);
            }

            _context.Actions.Add(actionDb);
            _context.SaveChanges();
        }

        public List<Core.Action> GetActionsForGame(int? gameId)
        {
            return _context.Actions.Include("Owner").Include("Target").Where(a => a.Game.GameId == gameId).ToList().Select(db => new Core.Action(db)).ToList();
        }

        public Core.Game GetGame(int gameId)
        {
            return new Core.Game(_context.Games.First(game => game.GameId == gameId));
        }

        public Core.GamePlay GetGamePlay(int gameId)
        {
            var gameplay = new Core.GamePlay(GetGame(gameId));
            var actions = GetActionsForGame(gameId);
            gameplay.DoActions(actions);
            return gameplay;
        }

        public void SaveGame(Core.Game game)
        {
            if (game.GameId==0) 
            {
                _context.Games.Add(new Database.Entities.Game());
            }
            _context.SaveChanges();
        }
    }
}