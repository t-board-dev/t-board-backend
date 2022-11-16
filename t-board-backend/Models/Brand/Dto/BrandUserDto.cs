using t_board.Entity;

namespace t_board_backend.Models.Brand.Dto
{
    public class BrandUserDto
    {
        public int BrandId { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
    }
}
