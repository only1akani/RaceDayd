using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {//start of class

        private readonly RaceDayDbContext _context;

        public AuthController(RaceDayDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Registers a new user as an Organiser or a Participant. The password is hashed before saving.
        /// </summary>
        /// <param name="dto">fullName, email, password, idNumber and role (Organiser or Participant)</param>
        /// <returns>201 with the new userId, 400 for missing fields or an invalid role, 409 if the email is already registered</returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register(RegisterDto dto)
        {//start of method

            //the role must exist in the Roles table
            Role? role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null)
            {
                return BadRequest(new { message = "Role must be either Organiser or Participant." });
            }

            //emails must be unique
            string email = dto.Email.Trim().ToLower();
            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == email);
            if (emailTaken)
            {
                return Conflict(new { message = "That email is already registered." });
            }

            User user = new User
            {
                RoleId = role.RoleId,
                FullName = dto.FullName.Trim(),
                Email = email,
                //hash the password, never store the real one
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                IdNumber = dto.IdNumber.Trim()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { userId = user.UserId });

        }//end of method

        /// <summary>
        /// Logs a user in. The server stores the user id and role in the session.
        /// </summary>
        /// <param name="dto">email and password</param>
        /// <returns>200 with the userId and role, 401 if the email or password is wrong</returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto dto)
        {//start of method

            string email = dto.Email.Trim().ToLower();

            User? user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            //same message for wrong email or wrong password
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Wrong email or password." });
            }

            //save who is logged in on the server
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role!.RoleName);

            return Ok(new { userId = user.UserId, role = user.Role.RoleName });

        }//end of method

        /// <summary>
        /// Logs the current user out by clearing the session.
        /// </summary>
        /// <returns>200 when logged out, 401 if nobody is logged in</returns>
        [HttpPost("logout")]
        [SessionAuthorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {//start of method

            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });

        }//end of method

    }//end of class
}//end of namespace