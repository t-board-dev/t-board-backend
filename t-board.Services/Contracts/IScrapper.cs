namespace t_board.Services.Contracts
{
    public interface IScrapper
    {
        public IScrappedModel Scrape(string url);
    }

    public interface IScrappedModel
    {
    }

    public delegate IScrapper ServiceResolver(string key);
}
