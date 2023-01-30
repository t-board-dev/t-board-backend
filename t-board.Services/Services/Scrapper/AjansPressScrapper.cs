using HtmlAgilityPack;
using System;
using t_board.Services.Contracts;

namespace t_board.Services.Services.Scrapper
{
    public class AjansPressScrapper : IScrapper
    {
        public AjansPressScrapper()
        {
        }

        public IScrappedModel Scrape(string url)
        {
            if (url.Contains("gold.ajanspress.com.tr") is false)
                throw new Exception("URL is not recognized!");

            var web = new HtmlWeb();
            var doc = web.Load(url);

            if (url.Contains("/linki/"))
                return ScrapeOnlineBasin(doc);

            if (url.Contains("/popuptv/"))
                return ScrapeTV(doc);

            if (url.Contains("/linkpress/"))
                return ScrapeYaziliBasin(doc);

            throw new Exception("Content could not scrapped!");
        }

        private AjansPressOnlineBasinModel ScrapeOnlineBasin(HtmlDocument doc)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//*[@id='wrapperpin']/div/div[1]/div[1]/h1");
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='collapsibleContent']");

            var model = new AjansPressOnlineBasinModel()
            {
                Title = titleNode?.InnerHtml ?? string.Empty,
                PublishName = GetItemByIndex(1, 1),
                Reference = GetItemByIndex(1, 2),
                PublishDateStr = GetItemByIndex(1, 3),
                Effect = GetItemByIndex(1, 4),
                Url = GetUrl(),
                Subject = GetItemByIndex(3, 1),
                Keyword = GetItemByIndex(3, 2)
            };

            return model;

            string GetItemByIndex(int rowIndex, int colIndex)
            {
                return contentNode.SelectSingleNode($"div[{rowIndex}]/div[{colIndex}]/div[1]/div[2]/strong")?.InnerText ?? string.Empty;
            }

            string GetUrl()
            {
                return contentNode.SelectSingleNode("div[2]/div[2]/a/strong")?.InnerText ?? string.Empty;
            }
        }

        private AjansPressTVModel ScrapeTV(HtmlDocument doc)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//*[@id='wrapperpin']/div/section[1]/div/div/div[3]");
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='pageHeaderTable']");

            var model = new AjansPressTVModel()
            {
                Title = titleNode?.InnerText.Replace("\r\n", string.Empty).Trim() ?? string.Empty,
                Reference = GetItemByIndex(3, 1),
                BroadcastName = GetItemByIndex(1, 1),
                ProgramName = GetItemByIndex(4, 1),
                BroadcastDateStr = GetItemByIndex(1, 2),
                BroadcastTimeStr = GetItemByIndex(2, 2),
                BroadcastLengthStr = GetItemByIndex(3, 2),
                Effect = GetItemByIndex(2, 1),
                AdEquivalent = Convert.ToDecimal(GetItemByIndex(4, 2).Replace(',', '.').Replace("$", string.Empty)),    // 37038,00 $
                AccessCount = Convert.ToInt32(GetItemByIndex(4, 3))
            };

            return model;

            string GetItemByIndex(int colIndex, int rowIndex)
            {
                return contentNode.SelectSingleNode($"div[{colIndex}]/div[{rowIndex}]/div[2]/strong")?.InnerText ?? string.Empty;
            }
        }

        private AjansPressYaziliBasinModel ScrapeYaziliBasin(HtmlDocument doc)
        {
            var titleNode = doc.DocumentNode.SelectSingleNode("//*[@id='wrapperpin']/div/div[1]/div[1]/h1");
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='pageHeaderTable']");

            var model = new AjansPressYaziliBasinModel()
            {
                Title = titleNode?.InnerText ?? string.Empty,
                Reference = GetItemByIndex(3, 2),
                PublishName = GetItemByIndex(2, 1),
                Period = GetItemByIndex(1, 2),
                PageNo = Convert.ToInt32(GetItemByIndex(1, 3)),
                PublishDateStr = GetItemByIndex(1, 4),
                ReportDateStr = GetItemByIndex(2, 4),
                Subject = contentNode.SelectSingleNode("//*[@id='toptable']/div[1]/div/div[2]/strong")?.InnerText ?? string.Empty,
                Effect = GetItemByIndex(2, 2),
                Keyword = contentNode.SelectSingleNode("//*[@id='toptable']/div[2]/div/div[2]/strong")?.InnerText ?? string.Empty,
                AdEquivalent = Convert.ToDecimal(GetItemByIndex(1, 1).Replace(',', '.').Replace("$", string.Empty)), // 37038,00 $,
                City = GetItemByIndex(3, 1),
                Distribution = GetItemByIndex(2, 3),
                Circulation = Convert.ToInt32(GetItemByIndex(3, 3)),
                Access = Convert.ToInt32(GetItemByIndex(3, 4)),
                Fax = GetItemByIndex(4, 1),
                Phone = GetItemByIndex(4, 2),
                StxCm = Convert.ToDecimal(GetItemByIndex(4, 3).Replace(',', '.'))
            };

            return model;

            string GetItemByIndex(int colIndex, int rowIndex)
            {
                return contentNode.SelectSingleNode($"div[{colIndex}]/div[{rowIndex}]/div[2]/strong")?.InnerText ?? string.Empty;
            }
        }
    }

    public class AjansPressOnlineBasinModel : IScrappedModel
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Reference { get; set; }
        public string PublishName { get; set; }
        public string PublishDateStr { get; set; }
        public DateTime PublishDate { get => DateTime.Parse(this.PublishDateStr); }
        public string Subject { get; set; }
        public string Effect { get; set; }
        public string Keyword { get; set; }
    }

    public class AjansPressTVModel : IScrappedModel
    {
        public string Title { get; set; }
        public string Reference { get; set; }
        public string BroadcastName { get; set; }
        public string ProgramName { get; set; }
        public string BroadcastDateStr { get; set; }
        public DateTime BroadcastDate { get => DateTime.Parse(this.BroadcastDateStr); }
        public string BroadcastTimeStr { get; set; }
        public TimeSpan BroadcastTime { get => TimeSpan.Parse(this.BroadcastTimeStr); }
        public string BroadcastLengthStr { get; set; }
        public TimeSpan BroadcastLength { get => TimeSpan.Parse(this.BroadcastLengthStr); }
        public string Effect { get; set; }
        public decimal AdEquivalent { get; set; }
        public int AccessCount { get; set; }
    }

    public class AjansPressYaziliBasinModel : IScrappedModel
    {
        public string Title { get; set; }
        public string Reference { get; set; }
        public string PublishName { get; set; }
        public string Period { get; set; }
        public int PageNo { get; set; }
        public string PublishDateStr { get; set; }
        public DateTime PublishDate { get => DateTime.Parse(this.PublishDateStr); }
        public string ReportDateStr { get; set; }
        public DateTime ReportDate { get => DateTime.Parse(this.ReportDateStr); }
        public string Subject { get; set; }
        public string Effect { get; set; }
        public string Keyword { get; set; }
        public decimal AdEquivalent { get; set; }
        public string City { get; set; }
        public string Distribution { get; set; }
        public int Circulation { get; set; }
        public int Access { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public decimal StxCm { get; set; }
    }
}
