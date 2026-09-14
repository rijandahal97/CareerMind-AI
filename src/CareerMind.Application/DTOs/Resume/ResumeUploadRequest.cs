using System;
using System.IO;

namespace CareerMind.Application.DTOs.Resume
{
    /// <summary>
    /// Passed from API layer to service layer for resume uploads.
    /// Decouples the Application layer from IFormFile (ASP.NET Core dependency).
    /// </summary>
    public class ResumeUploadRequest
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Length { get; set; }
        public Stream Content { get; set; } = Stream.Null;
    }
}
