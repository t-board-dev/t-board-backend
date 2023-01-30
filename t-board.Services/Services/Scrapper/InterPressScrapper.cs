using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using t_board.Services.Contracts;

namespace t_board.Services.Services.Scrapper
{
    public class InterPressScrapper : IScrapper
    {
        public InterPressScrapper()
        {

        }

        public IScrappedModel Scrap(string url)
        {
            var options = new ChromeOptions();

            options.AddArguments("headless");

            var chrome = new ChromeDriver(options);
            chrome.Navigate().GoToUrl(url);

            Task.Delay(5000).Wait();

            var pageSource = chrome.PageSource;

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(pageSource);

            var videoNode = doc.DocumentNode.SelectSingleNode("//*[@id='singleVideo']");
            if (videoNode != null)
                return ScrapTv(doc);

            var imageNode = doc.DocumentNode.SelectSingleNode("//*[@id='imageMain']");
            if (imageNode != null)
                return ScrapYaziliBasin(doc);

            var iframeNode = doc.DocumentNode.SelectSingleNode("//*[@id='iframe']");
            if (iframeNode != null)
                return ScrapOnlineBasin(doc);

            throw new Exception("Content could not scrapped!");
        }

        private InterPressOnlineBasinModel ScrapOnlineBasin(HtmlDocument doc)
        {
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='supportedContent']/app-root/doc-viewer-tab/div/div[2]/app-doc-viewer/app-doc-strem-container/div/div[1]/div[1]/app-doc-meta-data");

            var titleNode = contentNode.SelectSingleNode("//div/div[1]/table/tr/td/a");
            var publishNameNode = contentNode.SelectSingleNode("//div/div[2]/div[2]/table/tr[1]/td/div");
            var publishDateNode = contentNode.SelectSingleNode("//div/div[2]/div[3]/table/tr/td[2]");
            var cNode = contentNode.SelectSingleNode("//div/div[2]/div[5]/table/tr/td[2]");
            var keywordNodes = doc.DocumentNode.SelectNodes("//*[@id='ngb-nav-3-panel']/div/table/tbody/tr/td[2]");

            var keywords = keywordNodes?.Select(n => n.InnerText) ?? Array.Empty<string>();

            return new InterPressOnlineBasinModel()
            {
                Title = titleNode.InnerText,
                PublishName = publishNameNode.InnerText,
                PublishDateStr = publishDateNode.InnerText,
                Content = cNode.InnerText,
                Keywords = keywords

            };
        }

        private InterPressYaziliBasinModel ScrapYaziliBasin(HtmlDocument doc)
        {
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='supportedContent']/app-root/doc-viewer-tab/div/div[2]/app-doc-viewer/app-doc-strem-container/div/div[1]/div[1]/app-doc-meta-data");

            var publishNameNode = contentNode.SelectSingleNode("//div/div/div[2]/table/tr[1]/td/div");
            var publishDateNode = contentNode.SelectSingleNode("//div/div/div[3]/table/tr[1]/td[2]");
            var circulationNode = contentNode.SelectSingleNode("//div/div/div[5]/table/tr[1]/td[2]");
            var locationNode = contentNode.SelectSingleNode("//div/div/div[2]/table/tr[2]/td");
            var adEqNode = contentNode.SelectSingleNode("//div/div/div[3]/table/tr[2]/td[2]");
            var pageNode = contentNode.SelectSingleNode("//div/div/div[4]/table/tr[2]/td[2]");
            var cNode = contentNode.SelectSingleNode("//div/div/div[5]/table/tr[2]/td[2]");
            var keywordNodes = doc.DocumentNode.SelectNodes("//*[@id='ngb-nav-4-panel']/div/table/tbody/tr/td[2]");

            var keywords = keywordNodes?.Select(n => n.InnerText) ?? Array.Empty<string>();

            return new InterPressYaziliBasinModel()
            {
                Title = doc.DocumentNode.SelectSingleNode("//title").InnerText,
                PublishName = publishNameNode.InnerText,
                PublishDateStr = publishDateNode.InnerText,
                Location = locationNode.InnerText,
                Content = cNode.InnerText,
                AdEquivalent = Convert.ToDecimal(adEqNode.InnerText),
                Circulation = Convert.ToInt32(circulationNode.InnerText),
                Page = Convert.ToInt32(pageNode.InnerText),
                Keywords = keywords
            };
        }

        private InterPressTvModel ScrapTv(HtmlDocument doc)
        {
            var contentNode = doc.DocumentNode.SelectSingleNode("//*[@id='supportedContent']/app-root/doc-viewer-tab/div/div[2]/app-doc-viewer/app-doc-strem-container/div/div[1]/div[1]/app-doc-meta-data");

            var titleNode = contentNode.SelectSingleNode("//div/div[1]/table/tr/td/div");
            var broadcastNameNode = contentNode.SelectSingleNode("//div/div[2]/div[2]/table/tr[1]/td/div");
            var programNameNode = contentNode.SelectSingleNode("//div/div[2]/div[2]/table/tr[3]/td[2]");
            var broadcastDateNode = contentNode.SelectSingleNode("//div/div[2]/div[3]/table/tr[1]/td[2]");
            var broadcastLengthNode = contentNode.SelectSingleNode("//div/div[2]/div[3]/table/tr[2]/td[2]");
            var keywordNodes = doc.DocumentNode.SelectNodes("//*[@id='ngb-nav-0-panel']/div/table/tbody/tr/td[2]");

            var keywords = keywordNodes?.Select(n => n.InnerText) ?? Array.Empty<string>();

            return new InterPressTvModel()
            {
                Title = titleNode.InnerText,
                BroadcastName = broadcastNameNode.InnerText,
                ProgramName = programNameNode.InnerText,
                BroadcastDateStr = broadcastDateNode.InnerText,
                BroadcastLengthStr = broadcastLengthNode.InnerText,
                Keywords = keywords
            };
        }
    }

    public class InterPressOnlineBasinModel : IScrappedModel
    {
        public string Title { get; set; }
        public string PublishName { get; set; }
        public string PublishDateStr { get; set; }
        public DateTime PublishDate { get => DateTime.Parse(this.PublishDateStr); }
        public string Content { get; set; }
        public IEnumerable<string> Keywords { get; set; }
    }

    public class InterPressYaziliBasinModel : IScrappedModel
    {
        public string Title { get; set; }
        public string PublishName { get; set; }
        public string PublishDateStr { get; set; }
        public DateTime PublishDate { get => DateTime.Parse(this.PublishDateStr); }
        public string Location { get; set; }
        public string Content { get; set; }
        public decimal AdEquivalent { get; set; }
        public int Circulation { get; set; }
        public int Page { get; set; }
        public IEnumerable<string> Keywords { get; set; }
    }

    public class InterPressTvModel : IScrappedModel
    {
        public string Title { get; set; }
        public string BroadcastName { get; set; }
        public string ProgramName { get; set; }
        public string BroadcastDateStr { get; set; }
        public DateTime BroadcastDate { get => DateTime.Parse(this.BroadcastDateStr); }
        public string BroadcastLengthStr { get; set; }
        public TimeSpan BroadcastLength { get => TimeSpan.Parse(this.BroadcastLengthStr); }
        public IEnumerable<string> Keywords { get; set; }
    }
}
