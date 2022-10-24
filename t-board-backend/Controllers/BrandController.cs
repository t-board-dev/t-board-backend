using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board_backend.Models.Brand;

namespace t_board_backend.Controllers
{
    [Authorize(Roles = "Admin, CompanyOwner")]
    [Route("brand/")]
    public class BrandController : Controller
    {
        private readonly IInviteService _inviteService;

        private readonly UserManager<TBoardUser> _userManager;

        private readonly TBoardDbContext _dbContext;

        public BrandController(
            IInviteService inviteService,
            UserManager<TBoardUser> userManager,
            TBoardDbContext dbContext)
        {
            _inviteService = inviteService;

            _userManager = userManager;

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

        [HttpPost("getCompanyBrands")]
        public async Task<IActionResult> GetCompanyBrands(int companyId)
        {
            var brands = await _dbContext.Brands
                .Where(b => b.CompanyId == companyId)
                .ToArrayAsync();

            return Ok(brands);
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

        [HttpPost("createBrandUser")]
        public async Task<IActionResult> CreateBrandUser([FromBody] CreateBrandUserRequest createBrandUserRequest)
        {
            var user = new TBoardUser
            {
                FirstName = createBrandUserRequest.UserFirstName,
                LastName = createBrandUserRequest.UserLastName,
                UserName = createBrandUserRequest.UserEmail,
                Email = createBrandUserRequest.UserEmail,
                Title = createBrandUserRequest.UserTitle,
                PhoneNumber = createBrandUserRequest.UserPhoneNumber
            };

            var userCreated = await _userManager.CreateAsync(user);
            if (userCreated.Succeeded is false) return UnprocessableEntity(userCreated.Errors);

            foreach (var brandId in createBrandUserRequest.BrandIds)
            {
                var brandUser = new BrandUser
                {
                    BrandId = brandId,
                    UserId = user.Id
                };

                await _dbContext.BrandUsers.AddAsync(brandUser);
            }

            var companyUser = new CompanyUser
            {
                CompanyId = createBrandUserRequest.CompanyId,
                UserId = user.Id
            };

            await _dbContext.CompanyUsers.AddAsync(companyUser);

            var brandUserCreated = await _dbContext.SaveChangesAsync();
            if (brandUserCreated is 0) return UnprocessableEntity("User could not created!");

            var invitationSent = await _inviteService.SendInvitation(user.Email);
            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }
    }
}
