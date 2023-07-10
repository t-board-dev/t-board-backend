namespace t_board_backend.Models.Brand.Dto
{
    public class BrandUserDto
    {
        public string UserId { get; set; }
        public int BrandId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Title { get; set; }
        public string AvatarURL { get; set; }
        public bool AccountLocked { get; set; }
    }
}
