using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;

namespace TrueVote.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class FileController : ControllerBase
    {
        private readonly IPollFileService _pollFileService;

        public FileController(IPollFileService pollFileService)
        {
            _pollFileService = pollFileService;
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> GetPollFile(Guid fileId)
        {
            try
            {
                return await _pollFileService.DownloadFileAsync(fileId);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving file", detail = ex.Message });
            }
        }
    }
}