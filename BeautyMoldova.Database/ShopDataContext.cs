using System.Data.Entity;
using BeautyMoldova.Domain.Models;

namespace BeautyMoldova.Database
{
    public class ShopDataContext : DbContext
    {
        public ShopDataContext() : base("name=DefaultConnection")
        {
            // База данных инициализируется автоматически
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProductTag> ProductTags { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionProduct> PromotionProducts { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<CustomerRole> CustomerRoles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Banner> Banners { get; set; }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            // Конфигурация отношений между сущностями

            // Отношения для Customer
            builder.Entity<Customer>()
                .HasMany(c => c.Purchases)
                .WithRequired(p => p.Customer)
                .HasForeignKey(p => p.CustomerId);

            builder.Entity<Customer>()
                .HasMany(c => c.Reviews)
                .WithRequired(r => r.Customer)
                .HasForeignKey(r => r.CustomerId);

            builder.Entity<Customer>()
                .HasMany(c => c.WishlistItems)
                .WithRequired(w => w.Customer)
                .HasForeignKey(w => w.CustomerId);
                
            builder.Entity<Customer>()
                .HasMany(c => c.CustomerRoles)
                .WithRequired(cr => cr.Customer)
                .HasForeignKey(cr => cr.CustomerId);
                
            builder.Entity<Customer>()
                .HasMany(c => c.Notifications)
                .WithRequired(n => n.Customer)
                .HasForeignKey(n => n.CustomerId);

            // Отношения для Product
            builder.Entity<Product>()
                .HasRequired(p => p.Manufacturer)
                .WithMany(m => m.Products)
                .HasForeignKey(p => p.ManufacturerId);

            builder.Entity<Product>()
                .HasRequired(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            builder.Entity<Product>()
                .HasMany(p => p.AdditionalImages)
                .WithRequired(i => i.Product)
                .HasForeignKey(i => i.ProductId);

            builder.Entity<Product>()
                .HasMany(p => p.PurchaseItems)
                .WithRequired(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);

            builder.Entity<Product>()
                .HasMany(p => p.Reviews)
                .WithRequired(r => r.Product)
                .HasForeignKey(r => r.ProductId);

            builder.Entity<Product>()
                .HasMany(p => p.WishlistItems)
                .WithRequired(w => w.Product)
                .HasForeignKey(w => w.ProductId);

            builder.Entity<Product>()
                .HasMany(p => p.ProductTags)
                .WithRequired(pt => pt.Product)
                .HasForeignKey(pt => pt.ProductId);

            // Отношения для Category
            builder.Entity<Category>()
                .HasOptional(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId);
                
            builder.Entity<Category>()
                .HasMany(c => c.Banners)
                .WithOptional(b => b.TargetCategory)
                .HasForeignKey(b => b.TargetCategoryId);

            // Отношения для Manufacturer
            builder.Entity<Manufacturer>()
                .HasMany(m => m.Banners)
                .WithOptional(b => b.TargetManufacturer)
                .HasForeignKey(b => b.TargetManufacturerId);

            // Отношения для Purchase
            builder.Entity<Purchase>()
                .HasMany(p => p.PurchaseItems)
                .WithRequired(pi => pi.Purchase)
                .HasForeignKey(pi => pi.PurchaseId);

            // Отношения для Tag
            builder.Entity<Tag>()
                .HasMany(t => t.ProductTags)
                .WithRequired(pt => pt.Tag)
                .HasForeignKey(pt => pt.TagId);

            // Отношения для Promotion
            builder.Entity<Promotion>()
                .HasMany(pr => pr.PromotionProducts)
                .WithRequired(pp => pp.Promotion)
                .HasForeignKey(pp => pp.PromotionId);
                
            // Отношения для Role
            builder.Entity<Role>()
                .HasMany(r => r.CustomerRoles)
                .WithRequired(cr => cr.Role)
                .HasForeignKey(cr => cr.RoleId);

            base.OnModelCreating(builder);
        }
    }
} 