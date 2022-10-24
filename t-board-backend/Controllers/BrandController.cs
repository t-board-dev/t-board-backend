using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board_backend.Models.Brand;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace t_board_backend.Controllers
{
    [Authorize(Roles = "Admin, CompanyOwner")]
    [Route("brand/")]
    public class BrandController : Controller
    {
        private readonly TBoardDbContext _dbContext;

        public BrandController(
            TBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getBrands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _dbContext.Brands
                .ToArrayAsync();

            return Ok(brands);
        }

        [HttpPost("getBrand")]
        public async Task<IActionResult> GetBrand(int brandId)
        {
            var brand = await _dbContext.Brands
                .Where(c => c.Id == brandId)
                .FirstOrDefaultAsync();

            if (brand == null) return NotFound(brandId);

            return Ok(brand);
        }

        [HttpPost("getBrandUsers")]
        public async Task<IActionResult> GetBrandUsers(int brandId)
        {
            var brand = await _dbContext.Brands
                .Where(c => c.Id == brandId)
                .FirstOrDefaultAsync();

            if (brand == null) return NotFound(brandId);

            var brandUsers = await _dbContext.BrandUsers
                .Join(_dbContext.BoardUsers, bu => bu.UserId, u => u.Id,
                (bu, u) => new
                {
                    BrandId = bu.BrandId,
                    UserFirstName = u.FirstName,
                    UserLastName = u.LastName,
                    UserEmail = u.Email,
                })
                .Where(bu => bu.BrandId == brand.Id)
                .ToArrayAsync();

            return Ok(brandUsers);
        }

        [HttpPost("createBrand")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest createBrandRequest)
        {
            var brand = new Brand
            {
                CompanyId = createBrandRequest.CompanyId,
                Name = createBrandRequest.Name,
                Keywords = createBrandRequest.Keywords,
                LogoUrl = createBrandRequest.LogoUrl
            };

            await _dbContext.Brands.AddAsync(brand);
            var brandCreated = await _dbContext.SaveChangesAsync();
            if (brandCreated is 0) return UnprocessableEntity(createBrandRequest);

            return Ok();
        }
    }
}
