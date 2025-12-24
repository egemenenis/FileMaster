using System;
using System.Collections.Generic;
using System.Text;

namespace FileMaster.Application.DTOs;

public class FileResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
}