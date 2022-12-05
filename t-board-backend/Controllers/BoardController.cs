using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board_backend.Extensions;
using t_board_backend.Models.Board;

namespace t_board_backend.Controllers
{
    [Authorize]
    [Route("board/")]
    public class BoardController : Controller
    {
        private readonly TBoardDbContext _dbContext;

        public BoardController(
            TBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getBrandBoards")]
        [ProducesResponseType(typeof(BoardDto[]), 200)]
        public async Task<IActionResult> GetBrandBoards(int brandId)
        {
            var currentUser = await HttpContext.GetCurrentUserId();

            var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == brandId).FirstOrDefaultAsync();
            if (brandUser == null) return NotFound();

            var boards = await _dbContext.Boards
                .Where(b => b.BrandId == brandId)
                .Select(b => new BoardDto()
                {
                    Id = b.Id,
                    BrandId = b.BrandId,
                    Name = b.Name,
                    Description = b.Description,
                    Status = b.Status,
                    Design = b.Design
                })
                .ToArrayAsync();

            return Ok(boards);
        }

        [HttpGet("getBoardItems")]
        [ProducesResponseType(typeof(BoardItemDto[]), 200)]
        public async Task<IActionResult> GetBoardItems(int boardId)
        {
            var board = await _dbContext.Boards.Where(b => b.Id == boardId).Include(b => b.BoardItems).FirstOrDefaultAsync();

            var currentUser = await HttpContext.GetCurrentUserId();

            var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == board.BrandId).FirstOrDefaultAsync();
            if (brandUser == null) return NotFound();

            var boardItems = board.BoardItems
                .Where(b => b.BoardId == boardId)
                .Select(b => new BoardItemDto()
                {
                    Id = b.Id,
                    BoardId = b.BoardId,
                    Title = b.Title,
                    Type = b.Type,
                    GridData = b.GridData,
                    CustomGridData = b.CustomGridData,
                    Data = b.Data
                })
                .ToArray();

            return Ok(boardItems);
        }

        [HttpGet("getBoardItemTypes")]
        [ProducesResponseType(typeof(BoardItemTypeDto[]), 200)]
        public async Task<IActionResult> GetBoardItemTypes()
        {
            var boardItemTypes = await _dbContext.BoardItemTypes
                .Select(b => new BoardItemTypeDto()
                {
                    Id = b.Id,
                    Name = b.Name,
                    Code = b.Code
                })
                .ToArrayAsync();

            return Ok(boardItemTypes);
        }

        [HttpPost("createBoard")]
        public async Task<IActionResult> CreateBoard([FromBody]BoardDto board)
        {
            var currentUser = await HttpContext.GetCurrentUserId();

            var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == board.BrandId).FirstOrDefaultAsync();
            if (brandUser == null) return Unauthorized();

            var newBoard = new Board()
            {
                BrandId = board.BrandId,
                Name = board.Name,
                Description = board.Description,
                Status = board.Status,
                Design = board.Design
            };

            _dbContext.Add(newBoard);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("createBoardItem")]
        public async Task<IActionResult> CreateBoardItem(BoardItemDto boardItem)
        {
            var board = await _dbContext.Boards.Where(b => b.Id == boardItem.BoardId).FirstOrDefaultAsync();
            if (board == null) BadRequest("Board could not found!");

            var currentUser = await HttpContext.GetCurrentUserId();

            var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == board.BrandId).FirstOrDefaultAsync();
            if (brandUser == null) return Unauthorized();

            var type = await _dbContext.BoardItemTypes.Where(t => t.Id == boardItem.Type).FirstOrDefaultAsync();
            if (type == null) BadRequest($"Board item type could not found!");

            var newBoardItem = new BoardItem()
            {
                BoardId = boardItem.BoardId,
                Title = boardItem.Title,
                Type = boardItem.Type,
                GridData = boardItem.GridData,
                CustomGridData = boardItem.CustomGridData,
                Data = boardItem.Data
            };

            _dbContext.Add(newBoardItem);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
