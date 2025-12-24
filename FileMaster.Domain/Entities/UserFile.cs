using System;
using System.Collections.Generic;
using System.Text;

namespace FileMaster.Domain.Entities
{
    public class UserFile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FileName { get; set; } = string.Empty;
        public string FileExtension { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string StoragePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = string.Empty;
    }
}
