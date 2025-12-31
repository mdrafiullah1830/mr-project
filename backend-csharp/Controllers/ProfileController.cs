using MRShop.Profile.Models;
using MRShop.Profile.Services;
using Microsoft.AspNetCore.Mvc;

namespace MRShop.Profile.Controllers
{
    /// <summary>
    /// User profile management endpoints
    /// </summary>
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            IProfileService profileService, 
            ILogger<ProfileController> logger,
            IWebHostEnvironment environment)
        {
            _profileService = profileService;
            _logger = logger;
            _environment = environment;
        }

        /// <summary>
        /// Get user profile by userId
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User profile data</returns>
        [HttpGet("{userId}")]
        public async Task<ActionResult<ProfileApiResponse<UserProfile>>> GetProfile(string userId)
        {
            _logger.LogInformation("Get profile request for user {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "User ID is required",
                    Data = null
                });
            }

            var profile = await _profileService.GetProfileAsync(userId);

            if (profile == null)
            {
                return NotFound(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "Profile not found",
                    Data = null
                });
            }

            return Ok(new ProfileApiResponse<UserProfile>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = profile
            });
        }

        /// <summary>
        /// Create a new user profile
        /// </summary>
        /// <param name="profile">Profile data</param>
        /// <returns>Created profile</returns>
        [HttpPost]
        public async Task<ActionResult<ProfileApiResponse<UserProfile>>> CreateProfile([FromBody] UserProfile profile)
        {
            _logger.LogInformation("Create profile request for user {UserId}", profile.UserId);

            if (string.IsNullOrEmpty(profile.UserId))
            {
                return BadRequest(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "User ID is required",
                    Data = null
                });
            }

            var (success, message, createdProfile) = await _profileService.CreateProfileAsync(profile);

            if (!success)
            {
                return BadRequest(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return CreatedAtAction(
                nameof(GetProfile),
                new { userId = profile.UserId },
                new ProfileApiResponse<UserProfile>
                {
                    Success = true,
                    Message = message,
                    Data = createdProfile
                });
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Updated profile data</param>
        /// <returns>Updated profile</returns>
        [HttpPut("{userId}")]
        public async Task<ActionResult<ProfileApiResponse<UserProfile>>> UpdateProfile(
            string userId, 
            [FromBody] UpdateProfileRequest request)
        {
            _logger.LogInformation("Update profile request for user {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "User ID is required",
                    Data = null
                });
            }

            // Set userId from route parameter
            request.UserId = userId;

            var (success, message, updatedProfile) = await _profileService.UpdateProfileAsync(userId, request);

            if (!success)
            {
                return BadRequest(new ProfileApiResponse<UserProfile>
                {
                    Success = false,
                    Message = message,
                    Data = null
                });
            }

            return Ok(new ProfileApiResponse<UserProfile>
            {
                Success = true,
                Message = message,
                Data = updatedProfile
            });
        }

        /// <summary>
        /// Upload profile photo
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="file">Image file</param>
        /// <returns>Updated profile with photo path</returns>
        [HttpPost("{userId}/photo")]
        [RequestSizeLimit(5_000_000)] // 5MB limit
        public async Task<ActionResult<ProfileApiResponse<object>>> UploadProfilePhoto(
            string userId, 
            IFormFile file)
        {
            _logger.LogInformation("Upload profile photo request for user {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "User ID is required",
                    Data = null
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "No file uploaded",
                    Data = null
                });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid file type. Only images are allowed (jpg, jpeg, png, gif, webp)",
                    Data = null
                });
            }

            // Validate file size (5MB max)
            if (file.Length > 5_000_000)
            {
                return BadRequest(new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "File size exceeds 5MB limit",
                    Data = null
                });
            }

            try
            {
                // Create uploads directory if it doesn't exist
                var uploadsDir = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                    _logger.LogInformation("Created uploads directory: {Directory}", uploadsDir);
                }

                // Generate unique filename
                var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{fileExtension}";
                var filePath = Path.Combine(uploadsDir, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store relative path in database
                var relativePath = $"/uploads/profiles/{uniqueFileName}";
                var (success, message) = await _profileService.UpdateProfilePhotoAsync(userId, relativePath);

                if (!success)
                {
                    // Delete uploaded file if database update failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    return BadRequest(new ProfileApiResponse<object>
                    {
                        Success = false,
                        Message = message,
                        Data = null
                    });
                }

                return Ok(new ProfileApiResponse<object>
                {
                    Success = true,
                    Message = "Profile photo uploaded successfully",
                    Data = new { photo_path = relativePath }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile photo for user {UserId}", userId);
                return StatusCode(500, new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while uploading the photo",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Check if profile exists
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Boolean indicating if profile exists</returns>
        [HttpGet("{userId}/exists")]
        public async Task<ActionResult<ProfileApiResponse<object>>> ProfileExists(string userId)
        {
            _logger.LogInformation("Check profile exists for user {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new ProfileApiResponse<object>
                {
                    Success = false,
                    Message = "User ID is required",
                    Data = null
                });
            }

            var exists = await _profileService.ProfileExistsAsync(userId);

            return Ok(new ProfileApiResponse<object>
            {
                Success = true,
                Message = exists ? "Profile exists" : "Profile not found",
                Data = new { exists }
            });
        }
    }
}
