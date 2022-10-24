using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace t_board.Entity;

public class TBoardDbContext : IdentityDbContext
{
    private readonly IConfiguration _configuration;

    public TBoardDbContext(
        IConfiguration configuration,
        DbContextOptions<TBoardDbContext> options)
        : base(options)
    {
        _configuration = configuration;
    }

    public DbSet<TBoardUser> BoardUsers { get; set; }

    public DbSet<UserInvitation> UserInvitations { get; set; }

    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyType> CompanyTypes { get; set; }
    public DbSet<CompanyUser> CompanyUsers { get; set; }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<BrandUser> BrandUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserInvitation>(e =>
        {
            e.Property(i => i.UserEmail).IsRequired().HasMaxLength(256);
            e.Property(i => i.InviteCode).IsRequired().HasMaxLength(512);
            e.Property(i => i.InviteDate).IsRequired();
            e.Property(i => i.ExpireDate).IsRequired();
            e.Property(i => i.IsConfirmed).IsRequired();
            e.Property(i => i.ConfirmDate).IsRequired();

        });

        builder.Entity<Company>(e =>
        {
            e.Property(c => c.Name).IsRequired().HasMaxLength(256);
            e.Property(c => c.Type).IsRequired();
            e.Property(c => c.Url).HasMaxLength(512);
        });

        builder.Entity<CompanyType>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(256);
            e.Property(t => t.Code).IsRequired().HasMaxLength(256);
        });

        builder.Entity<CompanyUser>(e =>
        {
            e.Property(cu => cu.CompanyId).IsRequired();
            e.Property(cu => cu.UserId).IsRequired().HasMaxLength(450);
        });

        builder.Entity<Brand>(e =>
        {
            e.Property(b => b.CompanyId).IsRequired();
            e.Property(b => b.Name).IsRequired().HasMaxLength(256);
            e.Property(b => b.Keywords).HasMaxLength(512);
            e.Property(b => b.LogoUrl).HasMaxLength(512);

            e.HasOne(b => b.Company)
             .WithMany(c => c.Brand)
             .HasForeignKey(b => b.CompanyId)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_Brand_Company");
        });

        builder.Entity<BrandUser>(e =>
        {
            e.Property(bu => bu.BrandId).IsRequired();
            e.Property(bu => bu.UserId).IsRequired().HasMaxLength(450);
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _configuration.GetConnectionString("SQL_SERVER") ??
            throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");

        optionsBuilder.UseSqlServer(connectionString);
    }
}
