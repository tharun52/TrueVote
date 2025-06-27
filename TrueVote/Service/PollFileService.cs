using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TrueVote.Interfaces;
using TrueVote.Models;

namespace TrueVote.Service
{
    public class PollFileService : IPollFileService
    {
        private readonly IRepository<Guid, PollFile> _pollFileRepository;

        public PollFileService(IRepository<Guid, PollFile> pollFileRepository)
        {
            _pollFileRepository = pollFileRepository;
        }

        public async Task<FileContentResult> DownloadFileAsync(Guid id)
        {
            var file = await _pollFileRepository.Get(id);

            if (file == null || file.Content == null)
            {
                throw new FileNotFoundException("Poll file not found in database");
            }

            var contentType = string.IsNullOrWhiteSpace(file.FileType)
                ? MediaTypeNames.Application.Octet
                : file.FileType;

            var fileName = string.IsNullOrWhiteSpace(file.Filename)
                ? "file" + (contentType.Contains("image") ? ".png" : "")
                : file.Filename;

            return new FileContentResult(file.Content, contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}