using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using t_board.Services.Contracts;

namespace t_board_backend.Controllers
{
    [Authorize]
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
    }
}
