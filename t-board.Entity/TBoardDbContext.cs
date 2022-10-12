using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using t_board.Entity.Entity;

namespace t_board.Entity;

public class TBoardDbContext : IdentityDbContext
{
    public TBoardDbContext(DbContextOptions<TBoardDbContext> options)
        : base(options)
    {
    }

    public DbSet<TBoardUser> BoardUsers { get; set; }
    public DbSet<UserInvitation> UserInvitations { get; set; }

    public DbSet<Company> Companies { get; set; }
    public DbSet<CompanyType> CompanyTypes { get; set; }
}
