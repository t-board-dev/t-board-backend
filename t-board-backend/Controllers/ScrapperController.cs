using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using t_board.Services.Contracts;

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
    }
}
