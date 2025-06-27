using Microsoft.AspNetCore.Mvc;

namespace TrueVote.Interfaces
{
    public interface IPollFileService
    {
        public Task<FileContentResult> DownloadFileAsync(Guid id);
    }
}