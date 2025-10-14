using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // ✅ Login endpoint
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            // Read credentials from environment variables
            string validUser = Environment.GetEnvironmentVariable("BILLING_USER") ?? "admin";
            string validPass = Environment.GetEnvironmentVariable("BILLING_PASS") ?? "admin123";

            if (model.Username == validUser && model.Password == validPass)
            {
                // For simplicity, just return success (could return JWT in future)
                return Ok(new { message = "Login successful" });
            }
            else
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }
        }
    }

    // DTO for login
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
