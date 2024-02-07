namespace t_board.Services.Contracts
{
    public interface IScraper
    {
        public IScrapedModel Scrape(string url);
    }

    public interface IScrapedModel
    {
    }

    public delegate IScraper ServiceResolver(string key);
}
