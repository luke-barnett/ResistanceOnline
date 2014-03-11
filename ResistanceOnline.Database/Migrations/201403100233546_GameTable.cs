namespace ResistanceOnline.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GameTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Actions",
                c => new
                    {
                        ActionId = c.Int(nullable: false, identity: true),
                        Type = c.String(),
                        Timestamp = c.DateTimeOffset(nullable: false, precision: 7),
                        Text = c.String(),
                        Game_GameId = c.Int(),
                        Owner_PlayerId = c.Int(),
                        Target_PlayerId = c.Int(),
                    })
                .PrimaryKey(t => t.ActionId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .ForeignKey("dbo.Players", t => t.Owner_PlayerId)
                .ForeignKey("dbo.Players", t => t.Target_PlayerId)
                .Index(t => t.Game_GameId)
                .Index(t => t.Owner_PlayerId)
                .Index(t => t.Target_PlayerId);
            
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Int(nullable: false, identity: true),
                        GameState = c.String(),
                        InitialHolderOfLadyOfTheLake = c.String(),
                        InitialLeader = c.String(),
                    })
                .PrimaryKey(t => t.GameId);
            
            CreateTable(
                "dbo.Characters",
                c => new
                    {
                        CharacterId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Game_GameId = c.Int(),
                    })
                .PrimaryKey(t => t.CharacterId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .Index(t => t.Game_GameId);
            
            CreateTable(
                "dbo.LoyaltyCards",
                c => new
                    {
                        LoyaltyCardId = c.Int(nullable: false, identity: true),
                        Card = c.String(),
                        Game_GameId = c.Int(),
                    })
                .PrimaryKey(t => t.LoyaltyCardId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .Index(t => t.Game_GameId);
            
            CreateTable(
                "dbo.Players",
                c => new
                    {
                        PlayerId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Character = c.String(),
                        Type = c.String(),
                        Guid = c.Guid(nullable: false),
                        Game_GameId = c.Int(),
                    })
                .PrimaryKey(t => t.PlayerId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .Index(t => t.Game_GameId);
            
            CreateTable(
                "dbo.Rules",
                c => new
                    {
                        RuleId = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Game_GameId = c.Int(),
                    })
                .PrimaryKey(t => t.RuleId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .Index(t => t.Game_GameId);                      
            
            CreateTable(
                "dbo.Rounds",
                c => new
                    {
                        RoundId = c.Int(nullable: false, identity: true),
                        Size = c.Int(nullable: false),
                        Fails = c.Int(nullable: false),
                        Game_GameId = c.Int(),
                    })
                .PrimaryKey(t => t.RoundId)
                .ForeignKey("dbo.Games", t => t.Game_GameId)
                .Index(t => t.Game_GameId);                       
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Rounds", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Actions", "Target_PlayerId", "dbo.Players");
            DropForeignKey("dbo.Actions", "Owner_PlayerId", "dbo.Players");
            DropForeignKey("dbo.Actions", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Rules", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Players", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.LoyaltyCards", "Game_GameId", "dbo.Games");
            DropForeignKey("dbo.Characters", "Game_GameId", "dbo.Games");          
            DropIndex("dbo.Rounds", new[] { "Game_GameId" });
            DropIndex("dbo.Actions", new[] { "Target_PlayerId" });
            DropIndex("dbo.Actions", new[] { "Owner_PlayerId" });
            DropIndex("dbo.Actions", new[] { "Game_GameId" });
            DropIndex("dbo.Rules", new[] { "Game_GameId" });
            DropIndex("dbo.Players", new[] { "Game_GameId" });
            DropIndex("dbo.LoyaltyCards", new[] { "Game_GameId" });
            DropIndex("dbo.Characters", new[] { "Game_GameId" });
            DropTable("dbo.Rounds");
            DropTable("dbo.Rules");
            DropTable("dbo.Players");
            DropTable("dbo.LoyaltyCards");
            DropTable("dbo.Characters");
            DropTable("dbo.Games");
            DropTable("dbo.Actions");
        }
    }
}
