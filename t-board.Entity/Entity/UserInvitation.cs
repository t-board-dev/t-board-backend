using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace t_board.Entity.Entity
{
    public class UserInvitation
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string InviteCode { get; set; }
        public DateTimeOffset InviteDate{ get; set; }
        public DateTimeOffset ExpireDate { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTimeOffset ConfirmDate { get; set; }
    }
}
