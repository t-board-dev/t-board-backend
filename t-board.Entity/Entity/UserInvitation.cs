using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace t_board.Entity
{
    public class UserInvitation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string UserEmail { get; set; }

        public string InviteCode { get; set; }

        public DateTimeOffset InviteDate { get; set; }

        public DateTimeOffset ExpireDate { get; set; }

        public bool IsConfirmed { get; set; }

        public DateTimeOffset ConfirmDate { get; set; }
    }
}
