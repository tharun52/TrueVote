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
    }
}