using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using t_board.Entity;
using t_board.Helpers;
using t_board_backend.Extensions;
using t_board_backend.Models.Board;
using t_board_backend.Models.Board.Dto;

namespace t_board_backend.Controllers
{
    [Route("board/")]
    public class BoardController : Controller
    {
        private readonly TBoardDbContext _dbContext;

        public BoardController(
            TBoardDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("getBoard")]
        [ProducesResponseType(typeof(BoardDto), 200)]
        public async Task<IActionResult> GetBoard([FromQuery] string boardId)
        {
            //var currentUser = await HttpContext.GetCurrentUserId();

            var board = await _dbContext.Boards
                .Where(b => 
                    b.Id == boardId &&
                    b.Status == 1)
                .Select(b => new BoardDto()
                {
                    Id = b.Id,
                    BrandId = b.BrandId,
                    Name = b.Name,
                    Description = b.Description,
                    Status = b.Status,
                    Design = b.Design,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,
                    CreateUser = b.CreateUser,
                    UpdateUser = b.UpdateUser
                })
                .FirstOrDefaultAsync();

            if (board == null) return NotFound();

            //var brandUser = await _dbContext.BrandUsers
            //    .Where(u =>
            //        u.UserId == currentUser &&
            //        u.BrandId == board.BrandId)
            //    .FirstOrDefaultAsync();

            //if (brandUser == null) return Forbid();

            return Ok(board);
        }

        [Authorize]
        [HttpGet("getBrandBoards")]
        [ProducesResponseType(typeof(BoardDto[]), 200)]
        public async Task<IActionResult> GetBrandBoards([FromQuery] int brandId, int pageIndex, int pageSize)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            var currentUser = await HttpContext.GetCurrentUserId();

            if (isCurrentUserAdmin is false)
            {
                var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == brandId).FirstOrDefaultAsync();
                if (brandUser == null) return NotFound();
            }

            var boardsQuery = _dbContext.Boards
                .Where(b => b.BrandId == brandId)
                .Select(b => new BoardDto()
                {
                    Id = b.Id,
                    BrandId = b.BrandId,
                    Name = b.Name,
                    Description = b.Description,
                    Status = b.Status,
                    Design = b.Design,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,
                    CreateUser = b.CreateUser,
                    UpdateUser = b.UpdateUser
                });

            var boards = await PaginatedList<BoardDto>.CreateAsync(boardsQuery, pageIndex, pageSize);

            return Ok(new
            {
                PageIndex = boards.PageIndex,
                PageCount = boards.TotalPages,
                HasNextPage = boards.HasNextPage,
                HasPreviousPage = boards.HasPreviousPage,
                Result = boards
            });
        }

        [HttpGet("getBoardItems")]
        [ProducesResponseType(typeof(BoardItemDto[]), 200)]
        public async Task<IActionResult> GetBoardItems([FromQuery] string boardId)
        {
            var board = await _dbContext.Boards
                .Where(b => b.Id == boardId)
                .Include(b => b.BoardItems)
                .FirstOrDefaultAsync();

            //var currentUser = await HttpContext.GetCurrentUserId();

            //var brandUser = await _dbContext.BrandUsers
            //    .Where(u =>
            //        u.UserId == currentUser &&
            //        u.BrandId == board.BrandId)
            //    .FirstOrDefaultAsync();

            //if (brandUser == null) return NotFound();

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
                    Data = b.Data,
                    CreateDate = b.CreateDate,
                    UpdateDate = b.UpdateDate,
                    CreateUser = b.CreateUser,
                    UpdateUser = b.UpdateUser
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

        [Authorize]
        [HttpPost("createBoard")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest createBoardRequest)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            var currentUser = await HttpContext.GetCurrentUserId();

            if (isCurrentUserAdmin is false)
            {
                var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == createBoardRequest.BrandId).FirstOrDefaultAsync();
                if (brandUser == null) return Forbid();
            }

            var newBoard = new Board()
            {
                Id = Guid.NewGuid().ToString(),
                BrandId = createBoardRequest.BrandId,
                Name = createBoardRequest.Name,
                Description = createBoardRequest.Description,
                Status = createBoardRequest.Status,
                Design = createBoardRequest.Design,
                CreateDate = DateTimeOffset.Now,
                CreateUser = currentUser
            };

            _dbContext.Add(newBoard);
            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPost("updateBoard")]
        public async Task<IActionResult> UpdateBoard([FromBody] BoardDto boardDto)
        {
            var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
            var currentUser = await HttpContext.GetCurrentUserId();

            var board = await _dbContext.Boards.Where(b => b.Id == boardDto.Id).FirstOrDefaultAsync();
            if (board == null) return NotFound(boardDto);

            if (isCurrentUserAdmin is false)
            {
                var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == board.BrandId).FirstOrDefaultAsync();
                if (brandUser == null) return Forbid();
            }

            board.Name = boardDto.Name;
            board.Description = boardDto.Description;
            board.Status = boardDto.Status;
            board.Design = boardDto.Design;
            board.UpdateDate = DateTimeOffset.Now;
            board.UpdateUser = currentUser;

            _dbContext.Entry(board).State = EntityState.Modified;

            var boardUpdated = await _dbContext.SaveChangesAsync();
            if (boardUpdated is 0) return Problem("Board could not updated!");

            return Ok();
        }

        [Authorize]
        [HttpPost("updateBoardItems")]
        public async Task<IActionResult> UpdateBoardItems([FromBody] BoardItemDto[] boardItems)
        {
            try
            {
                var isCurrentUserAdmin = HttpContext.IsCurrentUserAdmin();
                var currentUser = await HttpContext.GetCurrentUserId();

                foreach (var boardItem in boardItems)
                {
                    var board = await _dbContext.Boards.Where(b => b.Id == boardItem.BoardId).FirstOrDefaultAsync();
                    if (board == null) BadRequest("Board could not found!");

                    if (isCurrentUserAdmin is false)
                    {
                        var brandUser = await _dbContext.BrandUsers.Where(u => u.UserId == currentUser && u.BrandId == board.BrandId).FirstOrDefaultAsync();
                        if (brandUser == null) return Forbid();
                    }

                    var type = await _dbContext.BoardItemTypes.Where(t => t.Id == boardItem.Type).FirstOrDefaultAsync();
                    if (type == null) BadRequest($"Board item type could not found!");

                    var currentBoardItem = await _dbContext.BoardItems.Where(i => i.Id == boardItem.Id).FirstOrDefaultAsync();

                    if (currentBoardItem == null)
                    {
                        var newBoardItem = new BoardItem()
                        {
                            BoardId = boardItem.BoardId,
                            Title = boardItem.Title,
                            Type = boardItem.Type,
                            GridData = boardItem.GridData,
                            CustomGridData = boardItem.CustomGridData,
                            Data = boardItem.Data,
                            CreateDate = DateTimeOffset.Now,
                            CreateUser = currentUser
                        };

                        _dbContext.Add(newBoardItem);
                    }
                    else
                    {
                        currentBoardItem.Title = boardItem.Title;
                        currentBoardItem.GridData = boardItem.GridData;
                        currentBoardItem.CustomGridData = boardItem.CustomGridData;
                        currentBoardItem.Data = boardItem.Data;
                        currentBoardItem.UpdateDate = DateTimeOffset.Now;
                        currentBoardItem.UpdateUser = currentUser;

                        _dbContext.Entry(currentBoardItem).State = EntityState.Modified;
                    }

                }

                var boardItemsUpdated = await _dbContext.SaveChangesAsync();
                if (boardItemsUpdated is 0) return Problem("Board items could not updated!");

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }
    }
}
