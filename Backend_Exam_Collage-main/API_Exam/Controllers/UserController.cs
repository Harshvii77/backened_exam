using API_Exam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Exam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class UserController : Controller
    {
        private readonly BackendExamCollageContext context;
        public UserController(BackendExamCollageContext _context)
        {
            context = _context;
        }

        #region GetAllUsers
        [HttpGet]
        [Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await context.Users
                .Include(u => u.Role)
                .ToListAsync();

            var result = users.Select(u => new
            {
                id = u.Id,
                name = u.Name,
                email = u.Email,
                role = new
                {
                    id = u.Role?.Id ?? 0,
                    name = u.Role?.Name ?? "N/A"
                },
                created_at = u.CreatedAt
            });

            return Ok(result);
        }
        #endregion

        #region AddUser
        [HttpPost]
        //[Authorize(Roles = "MANAGER")]
        public async Task<IActionResult> AddUser([FromBody] UserInsertRequestModel request)
        {
            if (request == null) return BadRequest("Data is missing.");

            if (await context.Users.AnyAsync(u => u.Email == request.email))
                return BadRequest("A user with this email already exists.");

            var roleData = await context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.role);

            if (roleData == null) return BadRequest("The specified role does not exist.");

            var userData = new User
            {
                Name = request.name,
                Email = request.email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.password),
                RoleId = roleData.Id,
                CreatedAt = DateTime.Now
            };

            context.Users.Add(userData);
            await context.SaveChangesAsync();

            return Ok(new
            {
                id = userData.Id,
                name = userData.Name,
                email = userData.Email,
                role = new
                {
                    id = roleData.Id,
                    name = roleData.Name
                },
                created_at = userData.CreatedAt
            });
        }
        #endregion
    }
}
