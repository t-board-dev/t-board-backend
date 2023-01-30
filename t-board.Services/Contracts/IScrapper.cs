namespace t_board.Services.Contracts
{
    public interface IScrapper
    {
        public IScrappedModel Scrap(string url);
    }

    public interface IScrappedModel
    {
    }
}
