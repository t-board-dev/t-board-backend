namespace t_board_backend.Models.Company.Dto
{
    public class CompanyUserDto
    {
        public int CompanyId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Title { get; set; }
        public bool AccountLocked { get; set; }
    }
}
