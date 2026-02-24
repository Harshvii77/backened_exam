using API_Exam_23010101161.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API_Exam_23010101161.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : Controller
    {
        private readonly BackendExamCollageContext context;
        public TicketController(BackendExamCollageContext _context)
        {
            context = _context;
        }

        #region GetAllTickets
        [HttpGet]
        [Authorize(Roles = "USER,MANAGER,SUPPORT")]
        public async Task<IActionResult> GetAllTickets()
        {
            var tickets = await context.Tickets
                .Include(t => t.CreatedByNavigation)
                    .ThenInclude(u => u.Role)
                .Include(t => t.AssignedToNavigation)
                    .ThenInclude(u => u.Role)
                .ToListAsync();

            var response = tickets.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                description = t.Description,
                status = t.Status,
                priority = t.Priority,
                created_by = new
                {
                    id = t.CreatedByNavigation.Id,
                    name = t.CreatedByNavigation.Name,
                    email = t.CreatedByNavigation.Email,
                    role = new
                    {
                        id = t.CreatedByNavigation.Role.Id,
                        name = t.CreatedByNavigation.Role.Name
                    },
                    created_at = t.CreatedByNavigation.CreatedAt
                },
                assigned_to = t.AssignedToNavigation != null ? new
                {
                    id = t.AssignedToNavigation.Id,
                    name = t.AssignedToNavigation.Name,
                    email = t.AssignedToNavigation.Email,
                    role = new
                    {
                        id = t.AssignedToNavigation.Role.Id,
                        name = t.AssignedToNavigation.Role.Name
                    },
                    created_at = t.AssignedToNavigation.CreatedAt
                } : null,
                created_at = t.CreatedAt
            });

            return Ok(response);
        }
        #endregion

        #region AddTicket
        [HttpPost]
        [Authorize(Roles = "USER,MANAGER")]
        public async Task<IActionResult> AddTicket([FromBody] TicketInsertRequestModel request)
        {
            if (request == null) return BadRequest("Ticket data is missing.");

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out int userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority ?? "LOW",
                CreatedBy = userId,
            };

            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();

            var result = await context.Tickets
                .Include(t => t.CreatedByNavigation)
                    .ThenInclude(u => u.Role)
                .Include(t => t.AssignedToNavigation)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            return Ok(new
            {
                id = result.Id,
                title = result.Title,
                description = result.Description,
                status = result.Status,
                priority = result.Priority,
                created_by = new
                {
                    id = result.CreatedByNavigation.Id,
                    name = result.CreatedByNavigation.Name,
                    email = result.CreatedByNavigation.Email,
                    role = new
                    {
                        id = result.CreatedByNavigation.Role.Id,
                        name = result.CreatedByNavigation.Role.Name
                    },
                    created_at = result.CreatedByNavigation.CreatedAt
                },
                assigned_to = result.AssignedToNavigation != null ? new
                {
                    id = result.AssignedToNavigation.Id,
                    name = result.AssignedToNavigation.Name,
                    email = result.AssignedToNavigation.Email,
                    role = new
                    {
                        id = result.AssignedToNavigation.Role.Id,
                        name = result.AssignedToNavigation.Role.Name
                    },
                    created_at = result.AssignedToNavigation.CreatedAt
                } : null,
                created_at = result.CreatedAt
            });
        }
        #endregion

        #region DeleteTicket
        [HttpDelete("{id}")]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var ticket = await context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound("Ticket not found.");

            context.Tickets.Remove(ticket);
            await context.SaveChangesAsync();

            return NoContent();
        }
        #endregion

        #region UpdateTicketStatus
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> UpdateTicketStatus(int id, [FromBody] TicketStatusUpdateRequestModel request)
        {
            if (request == null) return BadRequest("Status is missing.");

            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("Ticket not found.");

            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdText, out int userId)) return Unauthorized("User ID not found in token.");

            var oldStatus = ticket.Status ?? "N/A";
            ticket.Status = request.Status;

            context.TicketStatusLogs.Add(new TicketStatusLog
            {
                TicketId = ticket.Id,
                OldStatus = oldStatus,
                NewStatus = request.Status,
                ChangedBy = userId,
                ChangedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var result = await context.Tickets
                .Include(t => t.CreatedByNavigation)
                    .ThenInclude(u => u.Role)
                .Include(t => t.AssignedToNavigation)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            return Ok(new
            {
                id = result.Id,
                title = result.Title,
                description = result.Description,
                status = result.Status,
                priority = result.Priority,
                created_by = new
                {
                    id = result.CreatedByNavigation.Id,
                    name = result.CreatedByNavigation.Name,
                    email = result.CreatedByNavigation.Email,
                    role = new
                    {
                        id = result.CreatedByNavigation.Role.Id,
                        name = result.CreatedByNavigation.Role.Name
                    },
                    created_at = result.CreatedByNavigation.CreatedAt
                },
                assigned_to = result.AssignedToNavigation != null ? new
                {
                    id = result.AssignedToNavigation.Id,
                    name = result.AssignedToNavigation.Name,
                    email = result.AssignedToNavigation.Email,
                    role = new
                    {
                        id = result.AssignedToNavigation.Role.Id,
                        name = result.AssignedToNavigation.Role.Name
                    },
                    created_at = result.AssignedToNavigation.CreatedAt
                } : null,
                created_at = result.CreatedAt
            });
        }
        #endregion

        #region AssignTicket
        [HttpPatch("{id}/assign")]
        [Authorize(Roles = "MANAGER,SUPPORT")]
        public async Task<IActionResult> AssignTicket(int id, [FromBody] TicketAssignRequestModel request)
        {
            if (request == null) return BadRequest("User ID is missing.");

            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (ticket == null) return NotFound("Ticket not found.");

            var userExists = await context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists) return BadRequest("Target user does not exist.");

            ticket.AssignedTo = request.UserId;
            await context.SaveChangesAsync();

            var result = await context.Tickets
                .Include(t => t.CreatedByNavigation)
                    .ThenInclude(u => u.Role)
                .Include(t => t.AssignedToNavigation)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(t => t.Id == ticket.Id);

            return Ok(new
            {
                id = result.Id,
                title = result.Title,
                description = result.Description,
                status = result.Status,
                priority = result.Priority,
                created_by = new
                {
                    id = result.CreatedByNavigation.Id,
                    name = result.CreatedByNavigation.Name,
                    email = result.CreatedByNavigation.Email,
                    role = new
                    {
                        id = result.CreatedByNavigation.Role.Id,
                        name = result.CreatedByNavigation.Role.Name
                    },
                    created_at = result.CreatedByNavigation.CreatedAt
                },
                assigned_to = result.AssignedToNavigation != null ? new
                {
                    id = result.AssignedToNavigation.Id,
                    name = result.AssignedToNavigation.Name,
                    email = result.AssignedToNavigation.Email,
                    role = new
                    {
                        id = result.AssignedToNavigation.Role.Id,
                        name = result.AssignedToNavigation.Role.Name
                    },
                    created_at = result.AssignedToNavigation.CreatedAt
                } : null,
                created_at = result.CreatedAt
            });
        }
        #endregion
    }
}
