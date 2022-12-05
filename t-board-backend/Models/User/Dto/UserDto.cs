namespace t_board_backend.Models.User.Dto
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarURL { get; set; }
        public string[] Roles { get; set; }
        public int? CompanyId { get; set; }
    }
}
