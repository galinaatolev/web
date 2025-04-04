namespace BeautyMoldova.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dbbbbbb : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Products", "ShortDescription", c => c.String());
            AddColumn("dbo.Products", "ApplicationArea", c => c.String());
            AddColumn("dbo.Products", "VolumeWeight", c => c.String());
            AddColumn("dbo.Promotions", "ImageUrl", c => c.String());
            AddColumn("dbo.Promotions", "DiscountPercent", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "DiscountPercent");
            DropColumn("dbo.Promotions", "ImageUrl");
            DropColumn("dbo.Products", "VolumeWeight");
            DropColumn("dbo.Products", "ApplicationArea");
            DropColumn("dbo.Products", "ShortDescription");
        }
    }
}
