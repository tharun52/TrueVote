using Microsoft.AspNetCore.Mvc;

namespace TrueVote.Models.DTOs
{
    public class FileReponseDto
    {
        public FileContentResult? File { get; set; }
        public string FileType { get; set; } = string.Empty;
    }
}