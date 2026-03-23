using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IStorageService _storageService;
        private readonly ILogger<UsersController> _logger;


        public UsersController(
            IUserService userService,
            IStorageService storageService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _storageService = storageService;
            _logger = logger;
        }

        /// GET: api/users/{id}
        /// <summary>
        /// Gets a user by ID
        /// </summary>
        /// <response code="200">Returns user information</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        /// PATCH: api/users/{id}
        /// <summary>
        /// Updates a user's information.
        /// If the updated user is the current user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="204">User updated successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, UpdateUserRequest request)
        {
            await _userService.UpdateUserAsync(id, request);
            return NoContent();
        }

        /// DELETE: api/users/{id}
        /// <summary>
        /// Deletes a user by ID. If the deleted user is the current user, also clears authentication cookies
        /// </summary>
        /// <param name="id"></param>
        /// <response code="204">User deleted successfully</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        /// POST: api/users/{id}/picture
        /// <summary>
        /// Uploads a profile picture for the specified user
        /// </summary>
        /// <response code="200">Picture uploaded successfully, returns picture URL</response>
        /// <response code="400">Invalid file format or size</response>
        /// <response code="401">Unauthorized</response>
        /// <response code="403">Forbidden — cannot update another user's picture</response>
        [Authorize]
        [HttpPost("{id}/picture")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> UploadProfilePicture(Guid id, IFormFile file)
        {
            // Verify the authenticated user is updating their own picture
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetCurrentAuth: Missing claims");
                return Unauthorized();
            }

            // Validate file
            if (file == null || file.Length == 0)
                return BadRequest("No file provided.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("File size exceeds 5MB limit.");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType))
                return BadRequest("Only JPEG, PNG and WebP are allowed.");

            // Upload to storage
            var pictureUrl = await _storageService.UploadProfilePictureAsync(id.ToString(), file);

            // Update user picture URL in DB
            await _userService.UpdateProfilePictureAsync(id, pictureUrl);

            _logger.LogInformation("UploadProfilePicture: User {UserId} updated profile picture", id);

            return Ok(pictureUrl);
        }
    }
}
