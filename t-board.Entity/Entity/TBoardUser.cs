using Microsoft.AspNetCore.Identity;

namespace t_board.Entity;

public class TBoardUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

