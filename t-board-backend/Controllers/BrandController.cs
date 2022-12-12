﻿using Microsoft.AspNetCore.Authorization;
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

            var brandsQuery = _dbContext.Brands
                .Include(b => b.Boards)
                .AsQueryable()
                .Where(expression);

            if (isCurrentUserAdmin is false)
            {
                var userId = await HttpContext.GetCurrentUserId();

                brandsQuery
                     .Join(_dbContext.BrandUsers,
                         brand => brand.Id,
                         brandUser => brandUser.BrandId,
                         (brand, brandUser) => new { brand, brandUser })
                     .Where(q =>
                         q.brandUser.UserId == userId)
                     .Select(q => q.brand);
            }

            var brands = await brandsQuery
                .Select(br => new BrandDto()
                {
                    Id = br.Id,
                    Name = br.Name,
                    LogoURL = br.LogoURL,
                    CompanyId = br.CompanyId,
                    Keywords = br.Keywords,
                    Design = br.Design,
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design
                    }).ToArray()
                }).ToArrayAsync();

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
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design
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
                    AccountLocked = u.LockoutEnabled
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
                    Boards = br.Boards.Select(bo => new BoardDto()
                    {
                        Id = bo.Id,
                        BrandId = bo.BrandId,
                        Name = bo.Name,
                        Description = bo.Description,
                        Status = bo.Status,
                        Design = bo.Design
                    })
                    .ToArray()
                })
                .ToArrayAsync();

            return Ok(brands);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createBrand")]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest createBrandRequest)
        {
            var brand = new Brand
            {
                CompanyId = createBrandRequest.CompanyId,
                Name = createBrandRequest.Name,
                LogoURL = createBrandRequest.LogoURL,
                Keywords = createBrandRequest.Keywords,
                Design = createBrandRequest.Design
            };

            await _dbContext.Brands.AddAsync(brand);
            var brandCreated = await _dbContext.SaveChangesAsync();
            if (brandCreated is 0) return UnprocessableEntity(createBrandRequest);

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

            _dbContext.Entry(brand).State = EntityState.Modified;

            var brandUpdated = await _dbContext.SaveChangesAsync();
            if (brandUpdated is 0) return Problem("Brand could not updated!");

            return Ok();
        }
    }
}
