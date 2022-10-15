using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class UserInvitation
    {
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string UserEmail { get; set; }

        [Required]
        [MaxLength(256)]
        public string InviteCode { get; set; }

        [Required]
        public DateTimeOffset InviteDate { get; set; }

        [Required]
        public DateTimeOffset ExpireDate { get; set; }

        [Required]
        public bool IsConfirmed { get; set; }

        [Required]
        public DateTimeOffset ConfirmDate { get; set; }
    }
}
