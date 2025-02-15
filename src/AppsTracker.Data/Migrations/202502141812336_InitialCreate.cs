namespace AppsTracker.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AppCategories",
                c => new
                    {
                        AppCategoryID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                    })
                .PrimaryKey(t => t.AppCategoryID);
            
            CreateTable(
                "dbo.Aplications",
                c => new
                    {
                        ApplicationID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 250),
                        FileName = c.String(maxLength: 360),
                        Version = c.String(maxLength: 50),
                        Description = c.String(maxLength: 150),
                        Company = c.String(maxLength: 150),
                        UserID = c.Int(nullable: false),
                        WinName = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.ApplicationID)
                .ForeignKey("dbo.Uzers", t => t.UserID)
                .Index(t => new { t.Name, t.UserID }, unique: true, name: "UQ_Aplication_Name_UserID");
            
            CreateTable(
                "dbo.AppLimits",
                c => new
                    {
                        AppLimitID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        LimitSpan = c.Int(nullable: false),
                        Limit = c.Long(nullable: false),
                        LimitReachedAction = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AppLimitID)
                .ForeignKey("dbo.Aplications", t => t.ApplicationID)
                .Index(t => t.ApplicationID, name: "IX_AppLimit_ApplicationID");
            
            CreateTable(
                "dbo.Uzers",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateTable(
                "dbo.Usages",
                c => new
                    {
                        UsageID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(nullable: false),
                        UsageStart = c.DateTime(nullable: false),
                        UsageEnd = c.DateTime(nullable: false),
                        IsCurrent = c.Boolean(nullable: false),
                        UsageType = c.Byte(nullable: false),
                        SelfUsageID = c.Int(),
                    })
                .PrimaryKey(t => t.UsageID)
                .ForeignKey("dbo.Uzers", t => t.UserID)
                .Index(t => t.UserID);
            
            CreateTable(
                "dbo.Logs",
                c => new
                    {
                        LogID = c.Int(nullable: false, identity: true),
                        WindowID = c.Int(nullable: false),
                        Finished = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateEnded = c.DateTime(nullable: false),
                        UtcDateCreated = c.DateTime(nullable: false),
                        UtcDateEnded = c.DateTime(nullable: false),
                        UsageID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LogID)
                .ForeignKey("dbo.Usages", t => t.UsageID)
                .ForeignKey("dbo.Windows", t => t.WindowID)
                .Index(t => t.WindowID, name: "IX_Log_WindowID")
                .Index(t => t.DateCreated, name: "IX_Log_DateCreated")
                .Index(t => t.DateEnded, name: "IX_Log_DateEnded")
                .Index(t => t.UsageID);
            
            CreateTable(
                "dbo.Screenshots",
                c => new
                    {
                        ScreenshotID = c.Int(nullable: false, identity: true),
                        Date = c.DateTime(nullable: false),
                        Width = c.Int(nullable: false),
                        Height = c.Int(nullable: false),
                        LogID = c.Int(nullable: false),
                        Screensht = c.Binary(nullable: false),
                        PopupHeight = c.Double(nullable: false),
                        PopupWidth = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.ScreenshotID)
                .ForeignKey("dbo.Logs", t => t.LogID)
                .Index(t => t.LogID);
            
            CreateTable(
                "dbo.Windows",
                c => new
                    {
                        WindowID = c.Int(nullable: false, identity: true),
                        ApplicationID = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 1000),
                    })
                .PrimaryKey(t => t.WindowID)
                .ForeignKey("dbo.Aplications", t => t.ApplicationID)
                .Index(t => new { t.ApplicationID, t.Title }, unique: true, name: "UQ_Window_Title_ApplicationID");
            
            CreateTable(
                "dbo.Recap",
                c => new
                    {
                        RecapID = c.Int(nullable: false, identity: true),
                        Timestamp = c.DateTime(nullable: false),
                        UserID = c.Int(nullable: false),
                        ApplicationID = c.Int(nullable: false),
                        WindowID = c.Int(nullable: false),
                        Duration = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.RecapID);
            
            CreateTable(
                "dbo.Settings",
                c => new
                    {
                        SettingsID = c.Int(nullable: false, identity: true),
                        RunAtStartup = c.Boolean(nullable: false),
                        TakeScreenshots = c.Boolean(nullable: false),
                        IsMasterPasswordSet = c.Boolean(nullable: false),
                        DeleteOldLogs = c.Boolean(nullable: false),
                        LightTheme = c.Boolean(nullable: false),
                        FirstRun = c.Boolean(nullable: false),
                        TrackingEnabled = c.Boolean(nullable: false),
                        TimerInterval = c.Double(nullable: false),
                        OldLogDeleteDays = c.Short(nullable: false),
                        WindowOpen = c.String(maxLength: 64),
                        EnableIdle = c.Boolean(nullable: false),
                        IdleTimer = c.Long(nullable: false),
                        DefaultScreenshotSavePath = c.String(nullable: false, maxLength: 360),
                    })
                .PrimaryKey(t => t.SettingsID);
            
            CreateTable(
                "dbo.ApplicationCategories",
                c => new
                    {
                        ApplicationID = c.Int(nullable: false),
                        AppCategoryID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationID, t.AppCategoryID })
                .ForeignKey("dbo.Aplications", t => t.ApplicationID, cascadeDelete: true)
                .ForeignKey("dbo.AppCategories", t => t.AppCategoryID, cascadeDelete: true)
                .Index(t => t.ApplicationID)
                .Index(t => t.AppCategoryID);
            
            CreateTable(
                "dbo.UsageUsages",
                c => new
                    {
                        Usage_UsageID = c.Int(nullable: false),
                        Usage_UsageID1 = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Usage_UsageID, t.Usage_UsageID1 })
                .ForeignKey("dbo.Usages", t => t.Usage_UsageID)
                .ForeignKey("dbo.Usages", t => t.Usage_UsageID1)
                .Index(t => t.Usage_UsageID)
                .Index(t => t.Usage_UsageID1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Usages", "UserID", "dbo.Uzers");
            DropForeignKey("dbo.UsageUsages", "Usage_UsageID1", "dbo.Usages");
            DropForeignKey("dbo.UsageUsages", "Usage_UsageID", "dbo.Usages");
            DropForeignKey("dbo.Logs", "WindowID", "dbo.Windows");
            DropForeignKey("dbo.Windows", "ApplicationID", "dbo.Aplications");
            DropForeignKey("dbo.Logs", "UsageID", "dbo.Usages");
            DropForeignKey("dbo.Screenshots", "LogID", "dbo.Logs");
            DropForeignKey("dbo.Aplications", "UserID", "dbo.Uzers");
            DropForeignKey("dbo.AppLimits", "ApplicationID", "dbo.Aplications");
            DropForeignKey("dbo.ApplicationCategories", "AppCategoryID", "dbo.AppCategories");
            DropForeignKey("dbo.ApplicationCategories", "ApplicationID", "dbo.Aplications");
            DropIndex("dbo.UsageUsages", new[] { "Usage_UsageID1" });
            DropIndex("dbo.UsageUsages", new[] { "Usage_UsageID" });
            DropIndex("dbo.ApplicationCategories", new[] { "AppCategoryID" });
            DropIndex("dbo.ApplicationCategories", new[] { "ApplicationID" });
            DropIndex("dbo.Windows", "UQ_Window_Title_ApplicationID");
            DropIndex("dbo.Screenshots", new[] { "LogID" });
            DropIndex("dbo.Logs", new[] { "UsageID" });
            DropIndex("dbo.Logs", "IX_Log_DateEnded");
            DropIndex("dbo.Logs", "IX_Log_DateCreated");
            DropIndex("dbo.Logs", "IX_Log_WindowID");
            DropIndex("dbo.Usages", new[] { "UserID" });
            DropIndex("dbo.AppLimits", "IX_AppLimit_ApplicationID");
            DropIndex("dbo.Aplications", "UQ_Aplication_Name_UserID");
            DropTable("dbo.UsageUsages");
            DropTable("dbo.ApplicationCategories");
            DropTable("dbo.Settings");
            DropTable("dbo.Recap");
            DropTable("dbo.Windows");
            DropTable("dbo.Screenshots");
            DropTable("dbo.Logs");
            DropTable("dbo.Usages");
            DropTable("dbo.Uzers");
            DropTable("dbo.AppLimits");
            DropTable("dbo.Aplications");
            DropTable("dbo.AppCategories");
        }
    }
}
