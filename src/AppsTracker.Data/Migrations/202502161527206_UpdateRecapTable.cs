namespace AppsTracker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateRecapTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Recap", "UserName", c => c.String(maxLength: 4000));
            AddColumn("dbo.Recap", "ApplicationName", c => c.String(maxLength: 4000));
            AddColumn("dbo.Recap", "WindowTitle", c => c.String(maxLength: 4000));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Recap", "WindowTitle");
            DropColumn("dbo.Recap", "ApplicationName");
            DropColumn("dbo.Recap", "UserName");
        }
    }
}
