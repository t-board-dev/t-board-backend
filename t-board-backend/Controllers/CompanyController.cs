using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Services.Contracts;
using t_board_backend.Models.Company;
using t_board_backend.Models.Company.Dto;

namespace t_board_backend.Controllers
{
    [Authorize(Roles = "Admin")]
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

        [HttpGet("getCompanies")]
        [ProducesResponseType(typeof(CompanyDto[]), 200)]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _dbContext.Companies.Select(c => new CompanyDto()
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                LogoURL = c.LogoURL
            })
                .ToArrayAsync();

            return Ok(companies);
        }

        [HttpPost("getCompany")]
        [ProducesResponseType(typeof(CompanyDto), 200)]
        public async Task<IActionResult> GetCompany(int companyId)
        {
            var company = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .Select(c => new CompanyDto()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    LogoURL = c.LogoURL
                })
                .FirstOrDefaultAsync();

            if (company == null) return NotFound(companyId);

            return Ok(company);
        }

        [HttpPost("getCompanyTypes")]
        [ProducesResponseType(typeof(CompanyTypeDto[]), 200)]
        public async Task<IActionResult> GetCompanyTypes()
        {
            var companyTypes = await _dbContext.CompanyTypes
                .Select(t => new CompanyTypeDto()
                {
                    Name = t.Name,
                    Code = t.Code
                })
                .ToArrayAsync();

            return Ok(companyTypes);
        }

        [HttpPost("getCompanyUsers")]
        [ProducesResponseType(typeof(CompanyUserDto[]), 200)]
        public async Task<IActionResult> GetCompanyUsers(int companyId)
        {
            var company = await _dbContext.Companies
                .Where(c => c.Id == companyId)
                .FirstOrDefaultAsync();

            if (company == null) return NotFound(companyId);

            var companyUsers = await _dbContext.CompanyUsers
                .Join(_dbContext.BoardUsers, cu => cu.UserId, u => u.Id,
                (cu, u) => new CompanyUserDto()
                {
                    CompanyId = cu.CompanyId,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Title = u.Title,
                    AccountLocked = u.LockoutEnabled
                })
                .Where(cu => cu.CompanyId == company.Id)
                .ToArrayAsync();

            return Ok(companyUsers);
        }

        [HttpPost("createCompany")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest createCompanyRequest)
        {
            var company = new Company
            {
                Name = createCompanyRequest.CompanyName,
                Type = createCompanyRequest.CompanyType,
                LogoURL = createCompanyRequest.CompanyUrl
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

        [HttpPost("updateCompany")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyDto companyDto)
        {
            var company = await _dbContext.Companies.Where(c => c.Id == companyDto.Id).FirstOrDefaultAsync();
            if (company == null) return NotFound(companyDto);

            company.Name = companyDto.Name;
            company.Type = companyDto.Type;
            company.LogoURL = companyDto.LogoURL;

            _dbContext.Entry(company).State = EntityState.Modified;

            var companyUpdated = await _dbContext.SaveChangesAsync();
            if (companyUpdated is 0) return Problem("Company could not updated!");

            return Ok();
        }
    }
}
