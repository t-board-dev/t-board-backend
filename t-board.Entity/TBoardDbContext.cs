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

    public DbSet<UserInvitation> UserInvitations { get; set; }
    public DbSet<TBoardUser> BoardUsers { get; set; }
}
