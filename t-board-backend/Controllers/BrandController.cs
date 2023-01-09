using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board_backend.Extensions;
using t_board_backend.Models.Board.Dto;
using t_board_backend.Models.Brand;
using t_board_backend.Models.Brand.Dto;

namespace t_board_backend.Controllers
{
    [Authorize]
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
        [ProducesResponseType(typeof(BrandDto[]), 200)]
        public async Task<IActionResult> GetBrands()
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            Expression<Func<Brand, bool>> expression = b => b.CompanyId > 0;

            if (isCurrentUserAdmin is false)
            {
                var companyId = await HttpContext.GetCurrentUserCompanyId();
                expression = b => b.CompanyId == companyId;
            }

            var companyBrands = _dbContext.Brands
                .Include(b => b.Boards)
                .AsQueryable()
                .Where(expression)
                .ToList();

            if (isCurrentUserAdmin is false)
            {
                var userId = await HttpContext.GetCurrentUserId();

                var authorizedBrands = await _dbContext.BrandUsers
                    .Where(q => q.UserId == userId)
                    .Select(q => q.BrandId)
                    .ToListAsync();

                companyBrands = companyBrands.Where(b => authorizedBrands.Contains(b.Id)).ToList();
            }

            var brands = companyBrands
                .Select(br => new BrandDto()
                {
                    Id = br.Id,
                    Name = br.Name,
                    LogoURL = br.LogoURL,
                    CompanyId = br.CompanyId,
                    Keywords = br.Keywords,
                    Design = br.Design,
                    CreateDate = br.CreateDate,
                    UpdateDate = br.UpdateDate,
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design,
                        CreateDate = bo.CreateDate,
                        UpdateDate = bo.UpdateDate,
                        CreateUser = bo.CreateUser,
                        UpdateUser = bo.UpdateUser
                    }).ToArray()
                }).ToArray();

            return Ok(brands);
        }

        [HttpGet("getBrand")]
        [ProducesResponseType(typeof(BrandDto), 200)]
        public async Task<IActionResult> GetBrand(int brandId)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            Expression<Func<Brand, bool>> expression = b => b.Id == brandId && b.CompanyId > 0;

            if (isCurrentUserAdmin is false)
            {
                var companyId = await HttpContext.GetCurrentUserCompanyId();
                expression = b => b.Id == brandId && b.CompanyId == companyId;
            }

            var brandQuery = _dbContext.Brands
                .Include(b => b.Boards)
                .AsQueryable()
                .Where(expression);

            if (isCurrentUserAdmin is false)
            {
                var userId = await HttpContext.GetCurrentUserId();

                brandQuery
                    .Join(_dbContext.BrandUsers,
                        brand => brand.Id,
                        brandUser => brandUser.BrandId,
                        (brand, brandUser) => new { brand, brandUser })
                    .Where(q =>
                        q.brandUser.UserId == userId)
                    .Select(q => q.brand);
            }

            var brand = await brandQuery
                .Select(br => new BrandDto()
                {
                    Id = br.Id,
                    Name = br.Name,
                    LogoURL = br.LogoURL,
                    CompanyId = br.CompanyId,
                    Keywords = br.Keywords,
                    Design = br.Design,
                    CreateDate = br.CreateDate,
                    UpdateDate = br.UpdateDate,
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design,
                        CreateDate = bo.CreateDate,
                        UpdateDate = bo.UpdateDate,
                        CreateUser = bo.CreateUser,
                        UpdateUser = bo.UpdateUser
                    })
                    .ToArray()
                })
                .FirstOrDefaultAsync();

            if (brand == null) return NotFound(brandId);

            return Ok(brand);
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpGet("getBrandUsers")]
        [ProducesResponseType(typeof(BrandUserDto[]), 200)]
        public async Task<IActionResult> GetBrandUsers(int brandId)
        {
            var companyId = await HttpContext.GetCurrentUserCompanyId();

            var brand = await _dbContext.Brands
                .Where(b =>
                    b.Id == brandId &&
                    b.CompanyId == companyId)
                .FirstOrDefaultAsync();

            if (brand == null) return NotFound(brandId);

            var brandUsers = await _dbContext.BrandUsers
                .Join(_dbContext.TBoardUsers, bu => bu.UserId, u => u.Id,
                (bu, u) => new BrandUserDto()
                {
                    BrandId = bu.BrandId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Title = u.Title,
                    AvatarURL = u.AvatarURL,
                    AccountLocked = u.LockoutEnabled && u.LockoutEnd > DateTime.Now
                })
                .Where(bu => bu.BrandId == brand.Id)
                .ToArrayAsync();

            return Ok(brandUsers);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getCompanyBrands")]
        [ProducesResponseType(typeof(BrandDto[]), 200)]
        public async Task<IActionResult> GetCompanyBrands(int companyId)
        {
            var brands = await _dbContext.Brands
                .Where(b => b.CompanyId == companyId)
                .Select(br => new BrandDto()
                {
                    Id = br.Id,
                    Name = br.Name,
                    LogoURL = br.LogoURL,
                    CompanyId = br.CompanyId,
                    Keywords = br.Keywords,
                    Design = br.Design,
                    CreateDate = br.CreateDate,
                    UpdateDate = br.UpdateDate,
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design,
                        CreateDate = bo.CreateDate,
                        UpdateDate = bo.UpdateDate,
                        CreateUser = bo.CreateUser,
                        UpdateUser = bo.UpdateUser
                    })
                    .ToArray()
                })
                .ToArrayAsync();

            return Ok(brands);
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpPost("createBrand")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest createBrandRequest)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            var brand = new Brand
            {
                CompanyId = createBrandRequest.CompanyId,
                Name = createBrandRequest.Name,
                LogoURL = createBrandRequest.LogoURL,
                Keywords = createBrandRequest.Keywords,
                Design = createBrandRequest.Design,
                CreateDate = DateTimeOffset.Now
            };

            await _dbContext.Brands.AddAsync(brand);
            var brandCreated = await _dbContext.SaveChangesAsync();
            if (brandCreated is 0) return UnprocessableEntity(createBrandRequest);

            var brandUser = new BrandUser();
            if (isCurrentUserAdmin is false)
            {
                var userId = await HttpContext.GetCurrentUserId();

                brandUser.UserId = userId;
                brandUser.BrandId = brand.Id;
            }
            else
            {
                var ownerRole = await _dbContext.Roles.Where(r => r.NormalizedName == "COMPANYOWNER").FirstOrDefaultAsync();
                var companyOwnerOfBrandsCompany = await _dbContext.CompanyUsers
                    .Join(
                        _dbContext.UserRoles,
                        cu => cu.UserId,
                        ur => ur.UserId,
                        (cu, ur) => new
                        {
                            UserId = cu.UserId,
                            CompanyId = cu.CompanyId,
                            UserRoleId = ur.RoleId
                        })
                    .Where(q =>
                        q.CompanyId == createBrandRequest.CompanyId &&
                        q.UserRoleId == ownerRole.Id)
                    .FirstOrDefaultAsync();

                brandUser.UserId = companyOwnerOfBrandsCompany.UserId;
                brandUser.BrandId = brand.Id;
            }

            await _dbContext.BrandUsers.AddAsync(brandUser);
            _ = await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpPost("createBrandUser")]
        public async Task<IActionResult> CreateBrandUser([FromBody] CreateBrandUserRequest createBrandUserRequest)
        {
            var companyId = await HttpContext.GetCurrentUserCompanyId();

            foreach (var brandId in createBrandUserRequest.BrandIds)
            {
                var brand = await _dbContext.Brands
                    .Where(b => b.Id == brandId && b.CompanyId == companyId)
                    .FirstOrDefaultAsync();

                if (brand == null) return NotFound(brandId);
            }

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
                CompanyId = await HttpContext.GetCurrentUserCompanyId(),
                UserId = user.Id
            };

            await _dbContext.CompanyUsers.AddAsync(companyUser);

            var brandUserCreated = await _dbContext.SaveChangesAsync();
            if (brandUserCreated is 0) return UnprocessableEntity("User could not created!");

            var invitationSent = await _inviteService.SendInvitation(user.Email);
            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpPost("updateBrand")]
        public async Task<IActionResult> UpdateBrand([FromBody] BrandDto brandDto)
        {
            var brand = await _dbContext.Brands.Where(b => b.Id == brandDto.Id).FirstOrDefaultAsync();
            if (brand == null) return NotFound(brandDto);

            brand.Name = brandDto.Name;
            brand.LogoURL = brandDto.LogoURL;
            brand.Keywords = brandDto.Keywords;
            brand.Design = brandDto.Design;
            brand.UpdateDate = DateTimeOffset.Now;

            _dbContext.Entry(brand).State = EntityState.Modified;

            var brandUpdated = await _dbContext.SaveChangesAsync();
            if (brandUpdated is 0) return Problem("Brand could not updated!");

            return Ok();
        }
    }
}
