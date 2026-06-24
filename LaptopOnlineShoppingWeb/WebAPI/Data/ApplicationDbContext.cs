using Microsoft.EntityFrameworkCore;
using WebAPI.Entities;
using WebAPI.Enums;

namespace WebAPI.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() 
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartItem> CartItems { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<PhysicalProduct> PhysicalProducts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    string GetConnectionString()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        return config["ConnectionStrings:DefaultConnection"]
        ?? throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured.");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__Branches__A1682FC51847B8B2");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.BranchName).HasMaxLength(150);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Carts__51BCD7B7A63EC4CF");

            entity.HasIndex(e => e.CustomerId, "UQ__Carts__A4AE64D91BDE734F").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithOne(p => p.Cart)
                .HasForeignKey<Cart>(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Carts__CustomerI__693CA210");
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => new { e.CartId, e.VariantId }).HasName("PK__CartItem__0156F48F142D23F4");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.CartId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__CartI__6D0D32F4");

            entity.HasOne(d => d.Variant).WithMany(p => p.CartItems)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartItems__Varia__6E01572D");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B44F28933");

            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D81291655B");

            entity.HasIndex(e => e.Username, "UQ__Customer__536C85E4A09DA74B").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Customer__A9D1053492BE9B64").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ResetPasswordExpiry).HasColumnType("datetime");
            entity.Property(e => e.ResetPasswordToken)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);
            entity.Property(e => e.IsGoogleAccount)
                .HasDefaultValue(false);
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__Feedback__6A4BEDD6EE12FBEB");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsHidden).HasDefaultValue(false);

            entity.HasOne(d => d.Customer).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Custo__73BA3083");

            entity.HasOne(d => d.Variant).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Feedbacks__Varia__74AE54BC");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF38985D99");

            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasConversion<string>()
                .HasDefaultValue(OrderStatus.Pending);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).HasConversion<string>();
            entity.Property(e => e.PaymentStatus).HasDefaultValue(false);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Branch).WithMany(p => p.Orders)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__Orders__BranchId__5812160E");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__Customer__571DF1D5");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.VariantId }).HasName("PK__OrderDet__937A78F7EBD2B470");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__5CD6CB2B");

            entity.HasOne(d => d.Variant).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Varia__5DCAEF64");
        });

        modelBuilder.Entity<PhysicalProduct>(entity =>
        {
            entity.HasKey(e => e.PhysicalId).HasName("PK__Physical__4FF14A03E7D8CA4C");

            entity.HasIndex(e => e.SerialNumber, "UQ__Physical__048A000865E42039").IsUnique();

            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("InStock");

            entity.HasOne(d => d.Branch).WithMany(p => p.PhysicalProducts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhysicalP__Branc__6383C8BA");

            entity.HasOne(d => d.Order).WithMany(p => p.PhysicalProducts)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK__PhysicalP__Order__6477ECF3");

            entity.HasOne(d => d.Variant).WithMany(p => p.PhysicalProducts)
                .HasForeignKey(d => d.VariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PhysicalP__Varia__628FA481");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD6AFFD206");

            entity.HasIndex(e => e.ProductCode, "UQ__Products__2F4E024F7D116448").IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.ProductName).HasMaxLength(150);
            entity.Property(e => e.Status).HasDefaultValue(true);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Products__Catego__4CA06362");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("PK__ProductV__0EA233844E485A34");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Cpu)
                .HasMaxLength(100)
                .HasColumnName("CPU");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Ram)
                .HasMaxLength(50)
                .HasColumnName("RAM");
            entity.Property(e => e.Ssd)
                .HasMaxLength(50)
                .HasColumnName("SSD");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ProductVa__Produ__5070F446");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A88AFBEBB");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160716D8DAD").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C8FAA5B3B");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E48CD1D7DF").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105346FAC97C5").IsUnique();

            entity.Property(e => e.AvatarUrl).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.ResetPasswordExpiry).HasColumnType("datetime");
            entity.Property(e => e.ResetPasswordToken)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__Users__BranchId__403A8C7D");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
