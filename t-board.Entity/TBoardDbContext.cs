using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;

namespace t_board.Entity;

public class TBoardDbContext : IdentityDbContext
{
    private readonly IConfiguration Configuration;

    public TBoardDbContext(
        IConfiguration configuration,
        DbContextOptions<TBoardDbContext> options)
        : base(options)
    {
        Configuration = configuration;
    }

    public DbSet<Board> Boards { get; set; }
    public DbSet<BoardItem> BoardItems { get; set; }
    public DbSet<BoardItemType> BoardItemTypes { get; set; }

    public DbSet<TBoardUser> TBoardUsers { get; set; }

    public DbSet<UserInvitation> UserInvitations { get; set; }

    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyType> CompanyTypes { get; set; }
    public DbSet<CompanyUser> CompanyUsers { get; set; }

    public DbSet<Brand> Brands { get; set; }
    public DbSet<BrandUser> BrandUsers { get; set; }
    public DbSet<BrandFile> BrandFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Board>(e =>
        {
            e.Property(b => b.Id).IsRequired().HasMaxLength(450);
            e.Property(b => b.BrandId).IsRequired();
            e.Property(b => b.Name).IsRequired().HasMaxLength(256);
            e.Property(b => b.Description).IsRequired().HasMaxLength(512);
            e.Property(b => b.Status).IsRequired();
            e.Property(b => b.Design).IsRequired().HasMaxLength(512);
            e.Property(b => b.CreateDate).IsRequired();
            e.Property(b => b.UpdateDate);
            e.Property(b => b.CreateUser).IsRequired().HasMaxLength(450);
            e.Property(b => b.UpdateUser).HasMaxLength(450);

            e.HasOne(b => b.Brand)
             .WithMany(c => c.Boards)
             .HasForeignKey(b => b.BrandId)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_Board_Brand");
        });

        builder.Entity<BoardItem>(e =>
        {
            e.Property(b => b.BoardId).IsRequired().HasMaxLength(450);
            e.Property(i => i.Title).IsRequired().HasMaxLength(256);
            e.Property(i => i.Type).IsRequired().HasMaxLength(256);
            e.Property(i => i.GridData).IsRequired().HasMaxLength(512);
            e.Property(i => i.CustomGridData).IsRequired().HasMaxLength(512);
            e.Property(i => i.Data).IsRequired();
            e.Property(i => i.CreateDate).IsRequired();
            e.Property(i => i.UpdateDate);
            e.Property(i => i.CreateUser).IsRequired().HasMaxLength(450);
            e.Property(i => i.UpdateUser).HasMaxLength(450);

            e.HasOne(b => b.Board)
             .WithMany(c => c.BoardItems)
             .HasForeignKey(b => b.BoardId)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_BoardItem_Board");
        });

        builder.Entity<BoardItemType>(e =>
        {
            e.Property(t => t.Name).IsRequired().HasMaxLength(256);
            e.Property(t => t.Code).IsRequired().HasMaxLength(256);
        });

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
            e.Property(c => c.LogoURL).HasMaxLength(256);
            e.Property(c => c.CreateDate).IsRequired();
            e.Property(c => c.UpdateDate);
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
            e.Property(b => b.LogoURL).HasMaxLength(256);
            e.Property(b => b.Keywords).HasMaxLength(512);
            e.Property(b => b.Design).HasMaxLength(512);
            e.Property(b => b.CreateDate).IsRequired();
            e.Property(b => b.UpdateDate);

            e.HasOne(b => b.Company)
             .WithMany(c => c.Brands)
             .HasForeignKey(b => b.CompanyId)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_Brand_Company");
        });

        builder.Entity<BrandUser>(e =>
        {
            e.Property(bu => bu.BrandId).IsRequired();
            e.Property(bu => bu.UserId).IsRequired().HasMaxLength(450);
        });

        builder.Entity<BrandFile>(e =>
        {
            e.Property(bu => bu.BrandId).IsRequired();
            e.Property(bu => bu.Name).IsRequired().HasMaxLength(256);
            e.Property(bu => bu.URL).IsRequired().HasMaxLength(256);
            e.Property(bu => bu.Status).IsRequired();

            e.HasOne(b => b.Brand)
             .WithMany(c => c.BrandFiles)
             .HasForeignKey(b => b.BrandId)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_BrandFile_Brand");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = Configuration.GetConnectionString("SQL_SERVER") ??
            throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");

        optionsBuilder.UseSqlServer(connectionString);
    }
}
