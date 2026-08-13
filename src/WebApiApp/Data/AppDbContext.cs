using Microsoft.EntityFrameworkCore;
using WebApiApp.Models;

namespace WebApiApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProductCatalog> ProductCatalogs => Set<ProductCatalog>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<User> Users => Set<User>();

    public DbSet<OperationLog> OperationLogs => Set<OperationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");

            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleId)
                .HasColumnName("roleId")
                .UseIdentityColumn();

            entity.Property(e => e.RoleDescription)
                .HasColumnName("roleDescription");

            entity.Property(e => e.RoleName)
                .HasColumnName("roleName")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.RoleName)
                .IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(e => e.UserId);

            entity.Property(e => e.UserId)
                .HasColumnName("userId")
                .UseIdentityColumn();

            entity.Property(e => e.UserLogin)
                .HasColumnName("userLogin")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.PasswordHash)
                .HasColumnName("passwordHash")
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.UserRole)
                .HasColumnName("userRole");

            entity.Property(e => e.IsBlocked)
                .HasColumnName("IsBlocked")
                .HasDefaultValue(false);

            entity.HasIndex(e => e.UserLogin)
                .IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.UserRole)
                .HasPrincipalKey(r => r.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");

            entity.HasKey(e => e.CategoryId);

            entity.Property(e => e.CategoryId)
                .HasColumnName("category_id")
                .UseIdentityColumn();

            entity.Property(e => e.CategoryName)
                .HasColumnName("category_name")
                .HasMaxLength(20)
                .IsRequired();

            entity.HasIndex(e => e.CategoryName)
                .IsUnique();
        });

        modelBuilder.Entity<ProductCatalog>(entity =>
        {
            entity.ToTable("product_catalog");

            entity.HasKey(e => e.ProductId);

            entity.Property(e => e.ProductId)
                .HasColumnName("product_id")
                .UseIdentityColumn();

            entity.Property(e => e.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(20);

            entity.Property(e => e.ProductCathegory)
                .HasColumnName("product_cathegory");

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.ProductCathegory)
                .HasPrincipalKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.ProductDescription)
                .HasColumnName("product_description")
                .HasMaxLength(20);

            entity.Property(e => e.ProductCost)
                .HasColumnName("product_cost")
                .HasColumnType("numeric(18,2)");

            entity.Property(e => e.ProductNote)
                .HasColumnName("product_note")
                .HasMaxLength(20);

            entity.Property(e => e.ProductNoteSpecial)
                .HasColumnName("product_note_special")
                .HasMaxLength(20);
        });

        modelBuilder.Entity<OperationLog>(entity =>
        {
            entity.ToTable("operation_logs");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.EntityType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Operation)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.UserName)
                .HasMaxLength(200);

            entity.Property(e => e.Payload)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAtUtc)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");
        });
    }
}
