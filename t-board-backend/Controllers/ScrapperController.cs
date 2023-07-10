using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using t_board.Helpers;
using t_board.Services.Contracts;
using t_board_backend.Models.Scrapper;

namespace t_board_backend.Controllers
{
    [Authorize]
    [Route("scrapper/")]
    public class ScrapperController : Controller
    {
        private readonly IScrapper _ajansPressScrapper;
        private readonly IScrapper _interPressScrapper;
        private readonly IScrapper _medyaTakipScrapper;

        public ScrapperController(ServiceResolver serviceResolver)
        {
            _ajansPressScrapper = serviceResolver("AjansPress");
            _interPressScrapper = serviceResolver("InterPress");
            _medyaTakipScrapper = serviceResolver("MedyaTakip");
        }

        [HttpPost("scrapeAjansPress")]
        [Produces("application/json")]
        public async Task<IActionResult> ScrapeAjansPress(string url)
        {
            object scrappedModel = _ajansPressScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

            return Ok(jsonModel);
        }

        [HttpPost("scrapeInterPress")]
        [Produces("application/json")]
        public async Task<IActionResult> ScrapeInterPress(string url)
        {
            object scrappedModel = _interPressScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

            return Ok(jsonModel);
        }

        [HttpPost("scrapeMedyaTakip")]
        [Produces("application/json")]
        public async Task<IActionResult> ScrapeMedyaTakip(string url)
        {
            object scrappedModel = _medyaTakipScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                });

            return Ok(jsonModel);
        }

        [HttpPost("scrapeMetaData")]
        [Produces("application/json")]
        public async Task<IActionResult> ScrapeMetaData(string url)
        {
            if (Validator.IsValidUri(url) is false) return BadRequest("URL is not valid. Content could not scrapped.");

            var metaData = new MetaData();

            var web = new HtmlWeb();
            var doc = web.Load(url);

            var titleNode = doc.DocumentNode.SelectSingleNode("/html/head/title");

            if (titleNode != null)
            {
                metaData.Title = titleNode.InnerText;
            }

            var metaNodes = doc.DocumentNode.SelectNodes("/html/head/meta");
            foreach (var metaNode in metaNodes)
            {
                var attributes = metaNode.GetAttributes().ToList();

                if (attributes.Any(a => a.Name == "name" && a.Value == "description"))
                {
                    metaData.Description = attributes.FirstOrDefault(a => a.Name == "content")?.Value;
                }

                if (attributes.Any(a => a.Name == "property" && a.Value == "og:image"))
                {
                    metaData.ThumbnailURL = attributes.FirstOrDefault(a => a.Name == "content")?.Value;
                }

                attributes.ForEach(a => Console.WriteLine(a.Name + " " + a.Value));
            }

            return Ok(metaData);
        }
    }
}
