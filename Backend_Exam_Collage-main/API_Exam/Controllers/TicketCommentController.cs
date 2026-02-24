using API_Exam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Exam.Controllers
{
    [ApiController]
    [Authorize]
    public class TicketCommentController : Controller
    {
        private readonly BackendExamCollageContext context;

        public TicketCommentController(BackendExamCollageContext _context)
        {
            context = _context;
        }

        #region GetCommentsForTicket
        [HttpGet("api/tickets/{ticketId}/comments")]
        public async Task<IActionResult> GetCommentsForTicket(int ticketId)
        {
            var comments = await context.TicketComments
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .Where(c => c.TicketId == ticketId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var response = comments.Select(c => new
            {
                id = c.Id,
                comment = c.Comment,
                user = new
                {
                    id = c.User.Id,
                    name = c.User.Name,
                    email = c.User.Email,
                    role = new
                    {
                        id = c.User.Role.Id,
                        name = c.User.Role.Name
                    },
                    created_at = c.User.CreatedAt
                },
                created_at = c.CreatedAt
            });
            return Ok(response);
        }
        #endregion

        #region AddComment
        [HttpPost("api/tickets/{ticketId}/comments")]
        public async Task<IActionResult> AddComment(int ticketId, [FromBody] CommentRequestModel request)
        {
            if (request == null) return BadRequest("Comment data is missing.");

            var ticketExists = await context.Tickets.AnyAsync(t => t.Id == ticketId);
            if (!ticketExists) return NotFound("Ticket not found.");

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out int userId)) return Unauthorized("User ID not found in token.");

            var ticketComment = new TicketComment
            {
                TicketId = ticketId,
                UserId = userId,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow
            };

            context.TicketComments.Add(ticketComment);
            await context.SaveChangesAsync();

            var result = await context.TicketComments
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(c => c.Id == ticketComment.Id);

            return CreatedAtAction(nameof(AddComment), new { id = result.Id }, new
            {
                id = result.Id,
                comment = result.Comment,
                user = new
                {
                    id = result.User.Id,
                    name = result.User.Name,
                    email = result.User.Email,
                    role = new
                    {
                        id = result.User.Role.Id,
                        name = result.User.Role.Name
                    },
                    created_at = result.User.CreatedAt
                },
                created_at = result.CreatedAt
            });
        }
        #endregion

        #region EditComment
        [HttpPatch("api/comments/{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> EditComment(int id, [FromBody] CommentRequestModel request)
        {
            if (request == null) return BadRequest("Comment data is missing.");

            var comment = await context.TicketComments
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound("Comment not found.");

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(userIdText, out int userId)) return Unauthorized("User ID not found in token.");

            if (comment.UserId != userId && userRole != "MANAGER")
            {
                return Forbid("Not authorized to edit this comment.");
            }

            comment.Comment = request.Comment;
            await context.SaveChangesAsync();

            return Ok(new
            {
                id = comment.Id,
                comment = comment.Comment,
                user = new
                {
                    id = comment.User.Id,
                    name = comment.User.Name,
                    email = comment.User.Email,
                    role = new
                    {
                        id = comment.User.Role.Id,
                        name = comment.User.Role.Name
                    },
                    created_at = comment.User.CreatedAt
                },
                created_at = comment.CreatedAt
            });
        }
        #endregion

        #region DeleteComment
        [HttpDelete("api/comments/{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await context.TicketComments.FindAsync(id);
            if (comment == null) return NotFound("Comment not found.");

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (!int.TryParse(userIdText, out int userId)) return Unauthorized("User ID not found in token.");

            if (comment.UserId != userId && userRole != "MANAGER")
            {
                return Forbid("You are not authorized to delete this comment.");
            }

            context.TicketComments.Remove(comment);
            await context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
