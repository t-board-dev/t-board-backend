using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
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
        public async Task<IActionResult> ScrapeAjansPress(string url)
        {
            object scrappedModel = _ajansPressScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(
                scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            return Ok(jsonModel);
        }

        [HttpPost("scrapeInterPress")]
        public async Task<IActionResult> ScrapeInterPress(string url)
        {
            object scrappedModel = _interPressScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(
                scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            return Ok(jsonModel);
        }

        [HttpPost("scrapeMedyaTakip")]
        public async Task<IActionResult> ScrapeMedyaTakip(string url)
        {
            object scrappedModel = _medyaTakipScrapper.Scrape(url);

            var jsonModel = JsonSerializer.Serialize(
                scrappedModel,
                new JsonSerializerOptions()
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

            return Ok(jsonModel);
        }
    }
}
