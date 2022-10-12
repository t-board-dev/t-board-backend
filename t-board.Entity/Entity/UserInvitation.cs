using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace t_board.Entity.Entity
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
        public DateTimeOffset InviteDate{ get; set; }
        
        [Required]
        public DateTimeOffset ExpireDate { get; set; }
        
        [Required]
        public bool IsConfirmed { get; set; }
        
        [Required]
        public DateTimeOffset ConfirmDate { get; set; }
    }
}
