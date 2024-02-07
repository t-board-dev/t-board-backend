using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using t_board.Services.Contracts;

namespace t_board.Services.Services.Scraper
{
    public class MedyaTakipScraper : IScraper
    {
        public MedyaTakipScraper()
        {

        }

        public IScrapedModel Scrape(string url)
        {
            if (Helpers.Validator.IsValidUri(url) is false)
                throw new ArgumentException("URL is not valid. Content could not scraped.");

            if (url.Contains("clips.medyatakip.com") is false)
                throw new ArgumentException("URL is not valid. Content could not scraped.");

            var web = new HtmlWeb();
            var doc = web.Load(url);

            if (url.Contains("/pm/clip/"))
                return ScrapeYaziliBasin(doc);

            if (url.Contains("/dm/clip/"))
                return ScrapeOnlineBasin(doc);

            throw new ArgumentException("URL is not valid. Content could not scraped.");
        }

        private MedyaTakipOnlineBasinModel ScrapeOnlineBasin(HtmlDocument doc)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//*[@id='nav-tabContent']/div/center/h4");
            var dateNode = doc.DocumentNode.SelectSingleNode("//html/body/div/div[1]/div/div[1]/div/div/div/div/div[1]/div[2]/div");
            var webNode = doc.DocumentNode.SelectSingleNode("//html/body/div/div[1]/div/div[1]/div/div/div/div/div[2]/div[2]/div");
            var keywordNodes = doc.DocumentNode.SelectNodes("//*[@id='words']/div/*[@class='brandName']/a");

            var model = new MedyaTakipOnlineBasinModel()
            {
                Url = webNode.InnerText.Replace("\n", "").Trim(),
                Title = titleNode.InnerText.Replace("\n", "").Trim().Replace("&quot;", ""),
                DateStr = dateNode.InnerText.Replace("\n", "").Trim(),
                Keywords = keywordNodes.Select(n => n.InnerText.Split("(")[0].Trim()).Distinct()
            };

            return model;
        }

        private MedyaTakipYaziliBasinModel ScrapeYaziliBasin(HtmlDocument doc)
        {
            var keywordNodes = doc.DocumentNode.SelectNodes("//*[@id='words']/div/*[@class='brandName']/a");

            var model = new MedyaTakipYaziliBasinModel()
            {
                PublishName = GetItemByIndex(2, 1),
                Period = GetItemByIndex(1, 2),
                PageNo = Convert.ToInt32(GetItemByIndex(3, 1)),
                PublishDateStr = GetItemByIndex(1, 1),
                AdEquivalent = Convert.ToDecimal(GetItemByIndex(3, 2).Replace(',', '.').Replace("₺", string.Empty)),
                City = GetItemByIndex(4, 1),
                Circulation = Convert.ToInt32(GetItemByIndex(2, 2).Replace(".", "")),
                Keywords = keywordNodes.Select(n => n.InnerText.Split("(")[0].Trim()).Distinct()
            };

            return model;

            string GetItemByIndex(int colIndex, int rowIndex)
            {
                var node = doc.DocumentNode.SelectSingleNode($"//html/body/div/div[1]/div/div[4]/div/div/div/div[{rowIndex}]/div[{colIndex}]/div");
                return node?.InnerText.Replace("\n", "").Split(":")[1].Trim() ?? string.Empty;
            }
        }
    }

    public class MedyaTakipOnlineBasinModel : IScrapedModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string DateStr { get; set; }
        public DateTime Date { get => DateTime.ParseExact(this.DateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture); }
        public IEnumerable<string> Keywords { get; set; }
    }

    public class MedyaTakipYaziliBasinModel : IScrapedModel
    {
        public string PublishName { get; set; }
        public string Period { get; set; }
        public int PageNo { get; set; }
        public string PublishDateStr { get; set; }
        public DateTime PublishDate { get => DateTime.ParseExact(this.PublishDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture); }
        public IEnumerable<string> Keywords { get; set; }
        public decimal AdEquivalent { get; set; }
        public string City { get; set; }
        public int Circulation { get; set; }
    }
}
