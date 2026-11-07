using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{//start of namespace

    [Route("api/users")]
    [ApiController]
    [SessionAuthorize]
    public class UsersController : ControllerBase
    {//start of class

        private readonly RaceDayDbContext _context;
        private readonly IBlobStorageService _blobService;

        public UsersController(RaceDayDbContext context, IBlobStorageService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        /// <summary>
        /// Gets the profile of the logged in user (full name, email, role and profile picture).
        /// </summary>
        /// <param name="id">the user id (must be your own id)</param>
        /// <returns>200 with the profile, 401 if not logged in, 403 if it is not your profile, 404 if the user does not exist</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(int id)
        {//start of method

            User? user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            //users can only see their own profile
            if (user.UserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You can only view your own profile." });
            }

            return Ok(new
            {
                userId = user.UserId,
                fullName = user.FullName,
                email = user.Email,
                role = user.Role!.RoleName,
                profilePictureUrl = user.ProfilePictureUrl,
                createdAt = user.CreatedAt
            });

        }//end of method

        /// <summary>
        /// Updates the full name and email of the logged in user.
        /// </summary>
        /// <param name="id">the user id (must be your own id)</param>
        /// <param name="dto">fullName and email</param>
        /// <returns>200 when updated, 400 for invalid data, 401 if not logged in, 403 if it is not your profile, 404 if the user does not exist, 409 if the email belongs to someone else</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateProfile(int id, UpdateProfileDto dto)
        {//start of method

            User? user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (user.UserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You can only edit your own profile." });
            }

            //the new email must not belong to another user
            string email = dto.Email.Trim().ToLower();
            bool emailTaken = await _context.Users.AnyAsync(u => u.Email == email && u.UserId != id);
            if (emailTaken)
            {
                return Conflict(new { message = "That email is already registered." });
            }

            user.FullName = dto.FullName.Trim();
            user.Email = email;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile updated." });

        }//end of method

        /// <summary>
        /// Uploads a profile picture to Azure Blob Storage and saves the link on the user.
        /// </summary>
        /// <param name="id">the user id (must be your own id)</param>
        /// <param name="file">an image file (.jpg, .jpeg, .png or .webp, up to 5 MB)</param>
        /// <returns>200 with the profilePictureUrl, 400 for an invalid file, 401 if not logged in, 403 if it is not your profile, 404 if the user does not exist, 500 if the upload to Azure fails</returns>
        [HttpPost("{id}/profile-picture")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadProfilePicture(int id, IFormFile file)
        {//start of method

            User? user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (user.UserId != HttpContext.Session.GetInt32("UserId"))
            {
                return StatusCode(403, new { message = "You can only change your own profile picture." });
            }

            string? problem = ImageRules.Validate(file);
            if (problem != null)
            {
                return BadRequest(new { message = problem });
            }

            try
            {
                user.ProfilePictureUrl = await _blobService.UploadImageAsync(BlobContainers.ProfilePictures, file);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "The image could not be uploaded. Please try again." });
            }

            return Ok(new { profilePictureUrl = user.ProfilePictureUrl });

        }//end of method

    }//end of class
}//end of namespace