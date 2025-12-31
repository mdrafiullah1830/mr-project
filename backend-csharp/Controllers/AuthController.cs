using MRShop.Authentication.Models;
using MRShop.Authentication.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRShop.Authentication.Controllers
{
    /// <summary>
    /// Authentication endpoints for user sign up, sign in, and password recovery
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Sign up a new user
        /// </summary>
        /// <param name="request">Sign up request with username, email, and password</param>
        /// <returns>User object if successful</returns>
        [HttpPost("signup")]
        public async Task<ActionResult<ApiResponse<SignUpResponse>>> SignUp([FromBody] SignUpRequest request)
        {
            _logger.LogInformation("Sign up request for username {Username}", request.Username);

            var (success, message, user) = await _authService.SignUpAsync(request);

            if (!success)
            {
                return BadRequest(new ApiResponse<SignUpResponse>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            var response = new SignUpResponse
            {
                Id = user!.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            _logger.LogInformation("User {Username} signed up successfully", request.Username);

            return Created("api/auth/signup", new ApiResponse<SignUpResponse>
            {
                Success = true,
                Message = message,
                Data = response
            });
        }

        /// <summary>
        /// Sign in an existing user
        /// </summary>
        /// <param name="request">Sign in request with username/email and password</param>
        /// <returns>User object if successful</returns>
        [HttpPost("signin")]
        public async Task<ActionResult<ApiResponse<SignInResponse>>> SignIn([FromBody] SignInRequest request)
        {
            _logger.LogInformation("Sign in request for {UsernameOrEmail}", request.UsernameOrEmail);

            var (success, message, user) = await _authService.SignInAsync(request);

            if (!success)
            {
                return Unauthorized(new ApiResponse<SignInResponse>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            var response = new SignInResponse
            {
                Id = user!.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                SignInTime = DateTime.UtcNow
            };

            _logger.LogInformation("User {Username} signed in successfully", user.Username);

            return Ok(new ApiResponse<SignInResponse>
            {
                Success = true,
                Message = message,
                Data = response
            });
        }

        /// <summary>
        /// Initiate password recovery for a user
        /// </summary>
        /// <param name="request">Forgot password request with email</param>
        /// <returns>Status message</returns>
        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            _logger.LogInformation("Forgot password request for email {Email}", request.Email);

            var (success, message) = await _authService.ForgotPasswordAsync(request);

            return Ok(new ApiResponse<object>
            {
                Success = success,
                Message = message,
                Data = null
            });
        }

        /// <summary>
        /// Reset password for a user
        /// </summary>
        /// <param name="request">Reset password request with email and new password</param>
        /// <returns>Status message</returns>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            _logger.LogInformation("Reset password request for email {Email}", request.Email);

            var (success, message) = await _authService.ResetPasswordAsync(request);

            if (!success)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data = null
            });
        }

        /// <summary>
        /// Logout endpoint - frontend handles session clearing via localStorage
        /// </summary>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        public ActionResult<ApiResponse<object>> Logout()
        {
            _logger.LogInformation("User logout requested");
            
            // Since we're using localStorage on frontend, backend just confirms logout
            // No server-side session to clear in this implementation

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Logout successful. Please clear your browser session.",
                Data = null
            });
        }
    }
}
