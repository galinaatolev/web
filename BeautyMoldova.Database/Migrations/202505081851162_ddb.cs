namespace BeautyMoldova.Database.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ddb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Banners",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Description = c.String(),
                        ImageUrl = c.String(),
                        LinkUrl = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                        Position = c.String(),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        TargetCategoryId = c.Int(),
                        TargetManufacturerId = c.Int(),
                        ViewCount = c.Int(nullable: false),
                        ClickCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.TargetCategoryId)
                .ForeignKey("dbo.Manufacturers", t => t.TargetManufacturerId)
                .Index(t => t.TargetCategoryId)
                .Index(t => t.TargetManufacturerId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        ImageUrl = c.String(),
                        ParentCategoryId = c.Int(),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        Slug = c.String(),
                        MetaTitle = c.String(),
                        MetaDescription = c.String(),
                        MetaKeywords = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.ParentCategoryId)
                .Index(t => t.ParentCategoryId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SKU = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPrice = c.Decimal(precision: 18, scale: 2),
                        StockQuantity = c.Int(nullable: false),
                        IsAvailable = c.Boolean(nullable: false),
                        IsFeatured = c.Boolean(nullable: false),
                        ManufacturerId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        MainImage = c.String(),
                        AddedDate = c.DateTime(nullable: false),
                        LastModifiedDate = c.DateTime(),
                        Ingredients = c.String(),
                        HowToUse = c.String(),
                        Size = c.Int(),
                        SizeUnit = c.String(),
                        Weight = c.Decimal(precision: 18, scale: 2),
                        SkinType = c.String(),
                        ViewCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .ForeignKey("dbo.Manufacturers", t => t.ManufacturerId, cascadeDelete: true)
                .Index(t => t.ManufacturerId)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.ProductImages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        ImageUrl = c.String(),
                        AltText = c.String(),
                        DisplayOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Manufacturers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Logo = c.String(),
                        Website = c.String(),
                        Country = c.String(),
                        IsLuxury = c.Boolean(nullable: false),
                        EstablishedYear = c.DateTime(nullable: false),
                        SortOrder = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductTags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tags", t => t.TagId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.TagId);
            
            CreateTable(
                "dbo.Tags",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Slug = c.String(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PurchaseItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(),
                        ProductSKU = c.String(),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountedPrice = c.Decimal(precision: 18, scale: 2),
                        Quantity = c.Int(nullable: false),
                        Subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsGift = c.Boolean(nullable: false),
                        GiftMessage = c.String(),
                        ReturnRequested = c.Boolean(nullable: false),
                        ReturnRequestDate = c.DateTime(),
                        ReturnReason = c.String(),
                        ReturnStatus = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Purchases", t => t.PurchaseId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.PurchaseId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Purchases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        OrderNumber = c.String(),
                        OrderDate = c.DateTime(nullable: false),
                        ShippingDate = c.DateTime(),
                        DeliveryDate = c.DateTime(),
                        Status = c.String(),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ShippingCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PromoCode = c.String(),
                        PaymentMethod = c.String(),
                        PaymentStatus = c.String(),
                        ShippingMethod = c.String(),
                        TrackingNumber = c.String(),
                        ShippingAddress = c.String(),
                        BillingAddress = c.String(),
                        CustomerNotes = c.String(),
                        AdminNotes = c.String(),
                        AssignedEmployeeId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);
            
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstName = c.String(),
                        LastName = c.String(),
                        Email = c.String(),
                        PhoneNumber = c.String(),
                        Username = c.String(),
                        PasswordHash = c.Binary(),
                        PasswordSalt = c.Binary(),
                        AccessLevel = c.String(),
                        RegistrationDate = c.DateTime(nullable: false),
                        LastLoginDate = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                        ShippingAddress = c.String(),
                        BillingAddress = c.String(),
                        LoyaltyPoints = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.CustomerRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        RoleId = c.Int(nullable: false),
                        AssignedDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.Roles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        Permissions = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Reviews",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductId = c.Int(nullable: false),
                        CustomerId = c.Int(nullable: false),
                        Rating = c.Int(nullable: false),
                        Title = c.String(),
                        Comment = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        IsVerifiedPurchase = c.Boolean(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        IsDeleted = c.Boolean(nullable: false),
                        AdminResponse = c.String(),
                        AdminResponseDate = c.DateTime(),
                        HelpfulVotes = c.Int(),
                        UnhelpfulVotes = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.ProductId)
                .Index(t => t.CustomerId);
            
            CreateTable(
                "dbo.WishlistItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        AddedAt = c.DateTime(nullable: false),
                        Notes = c.String(),
                        Priority = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .Index(t => t.CustomerId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CustomerId = c.Int(nullable: false),
                        Title = c.String(),
                        Message = c.String(),
                        Type = c.String(),
                        IsRead = c.Boolean(nullable: false),
                        CreatedDate = c.DateTime(nullable: false),
                        ReadDate = c.DateTime(),
                        RelatedEntityType = c.String(),
                        RelatedEntityId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Customers", t => t.CustomerId, cascadeDelete: true)
                .Index(t => t.CustomerId);
            
            CreateTable(
                "dbo.Permissions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Code = c.String(),
                        Description = c.String(),
                        Category = c.String(),
                        CreatedDate = c.DateTime(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PromotionProducts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PromotionId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId, cascadeDelete: true)
                .ForeignKey("dbo.Promotions", t => t.PromotionId, cascadeDelete: true)
                .Index(t => t.PromotionId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Promotions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(),
                        Name = c.String(),
                        Description = c.String(),
                        DiscountType = c.String(),
                        DiscountValue = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumOrderAmount = c.Decimal(precision: 18, scale: 2),
                        MaximumUses = c.Int(),
                        UsageCount = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(),
                        ApplyToAllProducts = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PromotionProducts", "PromotionId", "dbo.Promotions");
            DropForeignKey("dbo.PromotionProducts", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Notifications", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.Banners", "TargetManufacturerId", "dbo.Manufacturers");
            DropForeignKey("dbo.Banners", "TargetCategoryId", "dbo.Categories");
            DropForeignKey("dbo.WishlistItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Reviews", "ProductId", "dbo.Products");
            DropForeignKey("dbo.PurchaseItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.PurchaseItems", "PurchaseId", "dbo.Purchases");
            DropForeignKey("dbo.WishlistItems", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.Reviews", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.Purchases", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.CustomerRoles", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.CustomerRoles", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.ProductTags", "ProductId", "dbo.Products");
            DropForeignKey("dbo.ProductTags", "TagId", "dbo.Tags");
            DropForeignKey("dbo.Products", "ManufacturerId", "dbo.Manufacturers");
            DropForeignKey("dbo.Products", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.ProductImages", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Categories", "ParentCategoryId", "dbo.Categories");
            DropIndex("dbo.PromotionProducts", new[] { "ProductId" });
            DropIndex("dbo.PromotionProducts", new[] { "PromotionId" });
            DropIndex("dbo.Notifications", new[] { "CustomerId" });
            DropIndex("dbo.WishlistItems", new[] { "ProductId" });
            DropIndex("dbo.WishlistItems", new[] { "CustomerId" });
            DropIndex("dbo.Reviews", new[] { "CustomerId" });
            DropIndex("dbo.Reviews", new[] { "ProductId" });
            DropIndex("dbo.CustomerRoles", new[] { "RoleId" });
            DropIndex("dbo.CustomerRoles", new[] { "CustomerId" });
            DropIndex("dbo.Purchases", new[] { "CustomerId" });
            DropIndex("dbo.PurchaseItems", new[] { "ProductId" });
            DropIndex("dbo.PurchaseItems", new[] { "PurchaseId" });
            DropIndex("dbo.ProductTags", new[] { "TagId" });
            DropIndex("dbo.ProductTags", new[] { "ProductId" });
            DropIndex("dbo.ProductImages", new[] { "ProductId" });
            DropIndex("dbo.Products", new[] { "CategoryId" });
            DropIndex("dbo.Products", new[] { "ManufacturerId" });
            DropIndex("dbo.Categories", new[] { "ParentCategoryId" });
            DropIndex("dbo.Banners", new[] { "TargetManufacturerId" });
            DropIndex("dbo.Banners", new[] { "TargetCategoryId" });
            DropTable("dbo.Promotions");
            DropTable("dbo.PromotionProducts");
            DropTable("dbo.Permissions");
            DropTable("dbo.Notifications");
            DropTable("dbo.WishlistItems");
            DropTable("dbo.Reviews");
            DropTable("dbo.Roles");
            DropTable("dbo.CustomerRoles");
            DropTable("dbo.Customers");
            DropTable("dbo.Purchases");
            DropTable("dbo.PurchaseItems");
            DropTable("dbo.Tags");
            DropTable("dbo.ProductTags");
            DropTable("dbo.Manufacturers");
            DropTable("dbo.ProductImages");
            DropTable("dbo.Products");
            DropTable("dbo.Categories");
            DropTable("dbo.Banners");
        }
    }
}
