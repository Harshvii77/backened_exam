using API_Exam.Models;
using API_Exam.Service;
using Microsoft.AspNetCore.Mvc;

namespace API_Exam.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly JwtService _jwtService;
        public AuthController(JwtService jwtService) => _jwtService = jwtService;
        
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequestModel request)
        {
            var result = await _jwtService.Authenticate(request);
            if (result is null)
            {
                return Unauthorized();
            }
            return Ok(result);
        }
    }
}
