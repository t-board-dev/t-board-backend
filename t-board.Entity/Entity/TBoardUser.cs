using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace t_board.Entity;

public class TBoardUser : IdentityUser
{
    [Required]
    [MaxLength(256)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(256)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; }
}

