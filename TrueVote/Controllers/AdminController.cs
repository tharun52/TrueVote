using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;
using TrueVote.Misc;
using TrueVote.Models.DTOs;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddAdminAsync([FromBody] AddAdminRequestDto adminDto)
        {
            if (adminDto == null)
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body"));
            }

            try
            {
                var newAdmin = await _adminService.AddAdmin(adminDto);
                return Created($"/api/admin/{newAdmin.Name}", ApiResponseHelper.Success(newAdmin, "Admin added successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }
        [HttpPut("update/{email}")]
        public async Task<IActionResult> UpdateAdminAsync(string email, [FromBody] UpdateAdminRequest adminDto)
        {
            if (adminDto == null)
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Invalid request body"));
            }

            if (string.IsNullOrEmpty(adminDto.PrevPassword))
            {
                return BadRequest(ApiResponseHelper.Failure<object>("Previous password is required"));
            }

            try
            {
                var updatedAdmin = await _adminService.UpdateAdmin(
                    email,
                    adminDto.PrevPassword,
                    adminDto.NewPassword,
                    adminDto.Name
                );
                return Ok(ApiResponseHelper.Success(updatedAdmin, "Admin updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponseHelper.Failure<object>(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }

        [HttpGet("{adminId}")]
        public async Task<IActionResult> GetAdminByIdAsync(Guid adminId)
        {
            try
            {
                var admin = await _adminService.GetAdminByIdAsync(adminId);
                return Ok(ApiResponseHelper.Success(admin, "Admin fetched successfully"));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponseHelper.Failure<object>(ex.Message));
            }
        }

        [HttpDelete("{adminId}")]
        public async Task<IActionResult> DeleteAdminAsync(Guid adminId)
        {
            try
            {
                var result = await _adminService.DeleteAdminAsync(adminId);
                if (result)
                    return Ok(ApiResponseHelper.Success<object>(null, "Admin deleted successfully"));
                else
                    return NotFound(ApiResponseHelper.Failure<object>("Admin not found"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure<object>("An unexpected error occurred: " + ex.Message));
            }
        }
    }
}