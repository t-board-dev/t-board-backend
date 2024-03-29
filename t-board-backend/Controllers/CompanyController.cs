﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Helpers;
using t_board.Services.Contracts;
using t_board_backend.Extensions;
using t_board_backend.Models.Board.Dto;
using t_board_backend.Models.Brand.Dto;
using t_board_backend.Models.Company;
using t_board_backend.Models.Company.Dto;

namespace t_board_backend.Controllers
{
    [Authorize]
    [Route("company/")]
    public class CompanyController : Controller
    {
        private readonly IInviteService _inviteService;

        private readonly UserManager<TBoardUser> _userManager;

        private readonly TBoardDbContext _dbContext;

        public CompanyController(
            IInviteService inviteService,
            UserManager<TBoardUser> userManager,
            TBoardDbContext dbContext)
        {
            _inviteService = inviteService;

            _userManager = userManager;

            _dbContext = dbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getCompanies")]
        [ProducesResponseType(typeof(CompanyDto[]), 200)]
        public async Task<IActionResult> GetCompanies([FromQuery] int pageIndex, int pageSize)
        {
            var companiesQuery = _dbContext.Companies
                .Include(c => c.Brands)
                .ThenInclude(br => br.Boards)
                .Select(c => new CompanyDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    LogoURL = c.LogoURL,
                    CreateDate = c.CreateDate,
                    UpdateDate = c.UpdateDate,
                    Brands = c.Brands.Select(br => new BrandDto()
                    {
                        Id = br.Id,
                        CompanyId = br.CompanyId,
                        Name = br.Name,
                        LogoURL = br.LogoURL,
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
                    .ToArray()
                });

            var companies = await PaginatedList<CompanyDto>.CreateAsync(companiesQuery.AsNoTracking(), pageIndex, pageSize);

            return Ok(new
            {
                PageIndex = companies.PageIndex,
                PageCount = companies.TotalPages,
                HasNextPage = companies.HasNextPage,
                HasPreviousPage = companies.HasPreviousPage,
                Result = companies
            });
        }

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpGet("getCompany")]
        [ProducesResponseType(typeof(CompanyDto), 200)]
        public async Task<IActionResult> GetCompany(int companyId)
        {
            var currentUserCompanyId = await HttpContext.GetCurrentUserCompanyId();
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            if (isCurrentUserAdmin is false && companyId != currentUserCompanyId) return Forbid();

            var company = await _dbContext.Companies
                .Include(c => c.Brands)
                .ThenInclude(br => br.Boards)
                .Where(c => c.Id == companyId)
                .Select(c => new CompanyDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    LogoURL = c.LogoURL,
                    CreateDate = c.CreateDate,
                    UpdateDate = c.UpdateDate,
                    Brands = c.Brands.Select(br => new BrandDto()
                    {
                        Id = br.Id,
                        CompanyId = br.CompanyId,
                        Name = br.Name,
                        LogoURL = br.LogoURL,
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
                    .ToArray()
                })
                .FirstOrDefaultAsync();

            if (company == null) return NotFound(companyId);

            return Ok(company);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("getCompanyTypes")]
        [ProducesResponseType(typeof(CompanyTypeDto[]), 200)]
        public async Task<IActionResult> GetCompanyTypes()
        {
            var companyTypes = await _dbContext.CompanyTypes
                .Select(t => new CompanyTypeDto()
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code
                })
                .ToArrayAsync();

            return Ok(companyTypes);
        }

        //[HttpGet("getCompanyOwner")]
        //[ProducesResponseType(typeof(CompanyUserDto[]), 200)]
        //public async Task<IActionResult> GetCompanyOwner(int companyId)
        //{
        //    var company = await _dbContext.Companies
        //        .Where(c => c.Id == companyId)
        //        .FirstOrDefaultAsync();

        //    if (company == null) return NotFound(companyId);

        //    var companyOwnerRole = await _dbContext.Roles.Where(r => r.Name == "CompanyOwner").FirstOrDefaultAsync();

        //    var companyUsers = await _dbContext.CompanyUsers
        //        .Join(_dbContext.UserRoles,
        //        cu => cu.UserId,
        //        ur => ur.UserId,
        //        (companyUser, userRole) => new
        //        {
        //            companyUser,
        //            userRole
        //        })
        //        .Join(_dbContext.TBoardUsers,
        //        combined => combined.companyUser.UserId,
        //        boardUser => boardUser.Id,
        //        (combined, boardUser) => new { combined, boardUser })
        //        .Where(q =>
        //            q.combined.companyUser.CompanyId == company.Id &&
        //            q.combined.userRole.RoleId == companyOwnerRole.Id)
        //        .Select(q => new CompanyUserDto()
        //        {
        //            CompanyId = q.combined.companyUser.CompanyId,
        //            FirstName = q.boardUser.FirstName,
        //            LastName = q.boardUser.LastName,
        //            Email = q.boardUser.Email,
        //            Title = q.boardUser.Title,
        //            AvatarURL = q.boardUser.AvatarURL,
        //            AccountLocked = q.boardUser.LockoutEnabled && q.boardUser.LockoutEnd > DateTime.Now
        //        })
        //        .FirstOrDefaultAsync();

        //    return Ok(companyUsers);
        //}

        [Authorize(Roles = "Admin, CompanyOwner")]
        [HttpGet("getCompanyUsers")]
        [ProducesResponseType(typeof(CompanyUserDto[]), 200)]
        public async Task<IActionResult> GetCompanyUsers(int companyId)
        {
            var currentUserCompanyId = await HttpContext.GetCurrentUserCompanyId();
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();

            if (isCurrentUserAdmin is false && companyId != currentUserCompanyId) return Forbid();

            var company = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .FirstOrDefaultAsync();

            if (company == null) return NotFound(companyId);

            var companyUsers = await (from cu in _dbContext.CompanyUsers
                                      join u in _dbContext.TBoardUsers on cu.UserId equals u.Id
                                      where cu.CompanyId == companyId
                                      select new CompanyUserDto()
                                      {
                                          UserId = u.Id,
                                          CompanyId = cu.CompanyId,
                                          FirstName = u.FirstName,
                                          LastName = u.LastName,
                                          Email = u.Email,
                                          PhoneNumber = u.PhoneNumber,
                                          Title = u.Title,
                                          AvatarURL = u.AvatarURL,
                                          AccountLocked = u.LockoutEnabled && u.LockoutEnd > DateTime.Now
                                      })
                                .ToArrayAsync();

            var companyBrands = await _dbContext.Brands
                .Include(b => b.Boards)
                .Where(b => b.CompanyId == companyId)
                .ToArrayAsync();

            foreach (var companyUser in companyUsers)
            {
                var user = await _userManager.FindByEmailAsync(companyUser.Email);

                companyUser.IsCompanyOwner = await _userManager.IsInRoleAsync(user, "CompanyOwner");

                var userBrandIds = await _dbContext.BrandUsers.Where(bu => bu.UserId == user.Id).Select(bu => bu.BrandId).ToArrayAsync();

                var userBrands = companyBrands
                    .Where(b => userBrandIds.Contains(b.Id));

                companyUser.Brands = userBrands.Select(b => new BrandDto()
                {
                    Id = b.Id,
                    Name = b.Name,
                    LogoURL = b.LogoURL,
                    CompanyId = b.CompanyId,
                    Keywords = b.Keywords,
                    Design = b.Design,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,
                    Boards = userBrands
                            .SelectMany(b =>
                                b.Boards.Select(bo => new BoardDto()
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
                                }))
                            .ToArray()
                })
                .ToArray();

            }

            return Ok(companyUsers);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("createCompany")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest createCompanyRequest)
        {
            var company = new Company
            {
                Name = createCompanyRequest.CompanyName,
                Type = createCompanyRequest.CompanyType,
                LogoURL = createCompanyRequest.CompanyUrl,
                CreateDate = DateTimeOffset.Now
            };

            var owner = new TBoardUser
            {
                FirstName = createCompanyRequest.OwnerFirstName,
                LastName = createCompanyRequest.OwnerLastName,
                UserName = createCompanyRequest.OwnerEmail,
                Email = createCompanyRequest.OwnerEmail,
                Title = createCompanyRequest.OwnerTitle,
                PhoneNumber = createCompanyRequest.OwnerPhoneNumber
            };

            var ownerCreated = await _userManager.CreateAsync(owner);
            if (ownerCreated.Succeeded is false) return UnprocessableEntity(ownerCreated.Errors);

            var ownerRoleCreated = await _userManager.AddToRoleAsync(owner, "CompanyOwner");
            if (ownerRoleCreated.Succeeded is false) return UnprocessableEntity(ownerRoleCreated.Errors);

            await _dbContext.Companies.AddAsync(company);

            var companyCreated = await _dbContext.SaveChangesAsync();
            if (companyCreated is 0) return UnprocessableEntity("Company could not created!");

            var companyUser = new CompanyUser { UserId = owner.Id, CompanyId = company.Id };
            await _dbContext.CompanyUsers.AddAsync(companyUser);

            var userAssigned = await _dbContext.SaveChangesAsync();
            if (userAssigned is 0) return UnprocessableEntity("User could not assigned to company!");

            var invitationSent = await _inviteService.SendInvitation(owner.Email);
            if (invitationSent.Succeeded is false) return UnprocessableEntity(invitationSent.Message);

            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("updateCompany")]
        public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyRequest updateCompanyRequest)
        {
            var company = await _dbContext.Companies.Where(c => c.Id == updateCompanyRequest.Id).FirstOrDefaultAsync();
            if (company == null) return NotFound(updateCompanyRequest);

            company.Name = updateCompanyRequest.Name;
            company.Type = updateCompanyRequest.Type;
            company.LogoURL = updateCompanyRequest.LogoURL;
            company.UpdateDate = DateTimeOffset.Now;

            _dbContext.Entry(company).State = EntityState.Modified;

            var companyUpdated = await _dbContext.SaveChangesAsync();
            if (companyUpdated is 0) return Problem("Company could not updated!");

            return Ok();
        }
    }
}
