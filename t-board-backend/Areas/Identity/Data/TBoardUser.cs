using Microsoft.AspNetCore.Identity;

namespace t_board_backend.Areas.Identity.Data;

public class TBoardUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

