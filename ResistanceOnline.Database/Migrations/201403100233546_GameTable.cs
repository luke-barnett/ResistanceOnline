namespace ResistanceOnline.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GameTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Games",
                c => new
                    {
                        GameId = c.Int(nullable: false, identity: true),
                    })
                .PrimaryKey(t => t.GameId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Games");
        }
    }
}
