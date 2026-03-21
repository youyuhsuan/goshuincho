using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<UsersController> _logger;


        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
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
    }
}
