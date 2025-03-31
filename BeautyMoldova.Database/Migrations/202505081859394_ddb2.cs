namespace BeautyMoldova.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ddb2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PurchaseItems", "TotalPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.Purchases", "PurchaseDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Purchases", "Notes", c => c.String());
            AddColumn("dbo.Reviews", "CreatedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.WishlistItems", "AddedDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.Promotions", "Title", c => c.String());
            AddColumn("dbo.Promotions", "PromoCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Promotions", "PromoCode");
            DropColumn("dbo.Promotions", "Title");
            DropColumn("dbo.WishlistItems", "AddedDate");
            DropColumn("dbo.Reviews", "CreatedDate");
            DropColumn("dbo.Purchases", "Notes");
            DropColumn("dbo.Purchases", "PurchaseDate");
            DropColumn("dbo.PurchaseItems", "TotalPrice");
        }
    }
}
