using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WebstoreProject.Models;

namespace WebstoreProject.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Brand> Brands => Set<Brand>();

        public DbSet<DeliveryType> DeliveryTypes => Set<DeliveryType>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // BRAND
            modelBuilder.Entity<Brand>(e =>
            {
                e.ToTable("brand");
                e.HasKey(x => x.BrandId);

                e.Property(x => x.BrandId).HasColumnName("brand_id");
                e.Property(x => x.Name).HasColumnName("name");
            });

            // CATEGORY 
            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable("category");
                e.HasKey(x => x.CategoryId);

                e.Property(x => x.CategoryId).HasColumnName("category_id");
                e.Property(x => x.ParentId).HasColumnName("parent_id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.SortOrder).HasColumnName("sort_order");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.IconUrl).HasColumnName("icon_url");
            });

            // PRODUCT
            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable("product");

                e.HasKey(x => x.ProductId);
                e.Property(x => x.ProductId).HasColumnName("product_id");

                e.Property(x => x.CategoryId).HasColumnName("category_id");
                e.Property(x => x.BrandId).HasColumnName("brand_id");

                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Description).HasColumnName("description");
                e.Property(x => x.ImageUrl).HasColumnName("image_url");

                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.StockQty).HasColumnName("stock_qty");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.DiscountPercent).HasColumnName("discount_percent");

                //e.Property(x => x.AmountText).HasColumnName("amount_text");

                // Nutrition
                e.Property(x => x.EnergyKj).HasColumnName("energy_kj");
                e.Property(x => x.EnergyKcal).HasColumnName("energy_kcal");
                e.Property(x => x.FatG).HasColumnName("fat_g");
                e.Property(x => x.SatFatG).HasColumnName("sat_fat_g");
                e.Property(x => x.CarbsG).HasColumnName("carbs_g");
                e.Property(x => x.SugarG).HasColumnName("sugar_g");
                e.Property(x => x.ProteinG).HasColumnName("protein_g");
                e.Property(x => x.SaltG).HasColumnName("salt_g");

                // Unit fields
                e.Property(x => x.UnitType).HasColumnName("unit_type");
                e.Property(x => x.UnitSize).HasColumnName("unit_size");

                // Relationships
                e.HasOne(x => x.Category)
                    .WithMany(x => x.Products)
                    .HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                
                e.HasOne(x => x.Brand)
                    .WithMany(b => b.Products)
                    .HasForeignKey(x => x.BrandId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // DELIVERY TYPE
            modelBuilder.Entity<DeliveryType>(e =>
            {
                e.ToTable("delivery_type");
                e.HasKey(x => x.DeliveryTypeId);

                e.Property(x => x.DeliveryTypeId).HasColumnName("delivery_type_id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.IsActive).HasColumnName("is_active");
            });

            // ORDERS
            modelBuilder.Entity<Order>(e =>
            {
                e.ToTable("orders");
                e.HasKey(x => x.OrderId);

                e.Property(x => x.OrderId).HasColumnName("order_id");
                e.Property(x => x.UserId).HasColumnName("user_id");
                e.Property(x => x.OrderDate).HasColumnName("order_date");
                e.Property(x => x.Status).HasColumnName("status");
                e.Property(x => x.DeliveryTypeId).HasColumnName("delivery_type_id");
                e.Property(x => x.DeliveryFee).HasColumnName("delivery_fee");
                e.Property(x => x.TotalPrice).HasColumnName("total_price");
                e.Property(x => x.DeliveryAddress).HasColumnName("delivery_address");
                e.Property(x => x.PaymentMethod).HasColumnName("payment_method");

                e.HasMany(x => x.Items)
                    .WithOne(x => x.Order)
                    .HasForeignKey(x => x.OrderId);
            });

            //Delivery TYPE
            modelBuilder.Entity<DeliveryType>(e =>
            {
                e.ToTable("delivery_type");
                e.HasKey(x => x.DeliveryTypeId);

                e.Property(x => x.DeliveryTypeId).HasColumnName("delivery_type_id");
                e.Property(x => x.Name).HasColumnName("name");
                e.Property(x => x.Price).HasColumnName("price");
                e.Property(x => x.IsActive).HasColumnName("is_active");
                e.Property(x => x.Description).HasColumnName("description");
            });


            // ORDER ITEM
            modelBuilder.Entity<OrderItem>(e =>
            {
                e.ToTable("order_item");
                e.HasKey(x => x.OrderItemId);

                e.Property(x => x.OrderItemId).HasColumnName("order_item_id");
                e.Property(x => x.OrderId).HasColumnName("order_id");
                e.Property(x => x.ProductId).HasColumnName("product_id");
                e.Property(x => x.Quantity).HasColumnName("quantity");
                e.Property(x => x.UnitPrice).HasColumnName("unit_price");
                e.Property(x => x.LineTotal).HasColumnName("line_total");

                e.HasOne(x => x.Product)
                    .WithMany()
                    .HasForeignKey(x => x.ProductId);
            });
        }
    }
}
