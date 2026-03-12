using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICookieService _cookieService;
        private readonly ILogger<UsersController> _logger;


        public UsersController(
            IUserService userService,
            ICookieService cookieService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _cookieService = cookieService;
            _logger = logger;
        }

        /// GET: api/users/{id}
        /// <summary>
        /// Gets a user by ID.
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

        /// POST: api/users
        /// <summary>
        /// Creates a new user with the provided information. 
        /// </summary>
        /// <param name="request"></param>
        /// <response code="201">User created successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="409">Email already exists</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateUser(CreateUserRequest request)
        {
            await _userService.CreateUserAsync(request);
            return Created();
        }

        /// PUT: api/users/{id}
        /// <summary>
        /// Updates a user's information.
        /// If the updated user is the current user, also updates the JWT cookie with new claims.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <response code="204">User updated successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUser(Guid id, UpdateUserRequest request)
        {
            await _userService.UpdateUserAsync(id, request);
            return NoContent();
        }

        /// DELETE: api/users/{id}
        /// <summary>
        /// Deletes a user by ID. If the deleted user is the current user, also clears authentication cookies.
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

            // Clear auth cookies if the deleted user is the current user
            _cookieService.ClearAuthCookies(Response);
            return NoContent();
        }
    }
}
