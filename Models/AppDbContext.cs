using App.Models.Orders;
using App.Models.Product;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MVC.Models;
using MVC.Models.Product;

namespace App.Models
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName.StartsWith("AspNet"))
                {
                    entityType.SetTableName(tableName.Substring(6));
                }
            }
            ;
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(e => e.Slug).IsUnique();
            });
            modelBuilder.Entity<PostCategory>(entity =>
            {
                entity.HasKey(e => new { e.PostID, e.CategoryID });
            });
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });

            modelBuilder.Entity<CategoryProduct>(entity =>
            {
                entity.HasIndex(e => e.Slug).IsUnique();
            });
            modelBuilder.Entity<ProductCategoryProduct>(entity =>
            {
                entity.HasKey(e => new { e.ProductId, e.CategoryId });
            });
            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.HasIndex(p => p.Slug).IsUnique();
            });

            modelBuilder.Entity<PostComment>(entity =>
            {
                entity.HasKey(c => c.CommentId);

                entity.HasOne(c => c.Post)
                        .WithMany(p => p.Comments)
                        .HasForeignKey(c => c.PostId)
                        .OnDelete(DeleteBehavior.Cascade); // Xóa bài viết sẽ xóa luôn bình luận

                entity.HasOne(c => c.Author)
                        .WithMany()
                        .HasForeignKey(c => c.AuthorId)
                        .OnDelete(DeleteBehavior.SetNull); // Khi xóa tác giả, giữ lại bình luận với AuthorId = NULL

            });

            // Cấu hình mối quan hệ với OrderModel và User
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.User)                // Một Order thuộc về một User
                .WithMany()                          // Một User có thể có nhiều Orders
                .HasForeignKey(o => o.UserId)        // Khóa ngoại là UserId
                .IsRequired();                       // Khóa ngoại UserId là bắt buộc

            // Cấu hình mối quan hệ với OrderItem và Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)              // Một OrderItem thuộc về một Order
                .WithMany(o => o.OrderItems)         // Một Order có thể có nhiều OrderItems
                .HasForeignKey(oi => oi.OrderId)     // Khóa ngoại là OrderId
                .OnDelete(DeleteBehavior.Cascade);   // Khi xóa Order thì các OrderItems liên quan cũng bị xóa

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        public DbSet<ContactModel> Contacts { set; get; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<CategoryProduct> CategoryProducts { get; set; }
        public DbSet<ProductCategoryProduct> ProductCategoryProducts { get; set; }
        public DbSet<ProductPhoto> productPhotos { set; get; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<OrderModel> Order { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

    }
}

