namespace t_board.Entity
{
    public class BrandFile
    {
        public string Id { get; set; }
        public int BrandId { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }

        public virtual Brand Brand { get; set; }
    }
}
