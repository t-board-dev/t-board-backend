using t_board_backend.Models.Brand.Dto;

namespace t_board_backend.Models.Company.Dto
{
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string LogoURL { get; set; }
        public BrandDto[] Brands { get; set; }
    }
}
