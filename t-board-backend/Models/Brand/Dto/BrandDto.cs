namespace t_board_backend.Models.Brand.Dto
{
    public class BrandDto
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string LogoURL { get; set; }
        public string Keywords { get; set; }
        public string Design { get; set; }
    }
}
