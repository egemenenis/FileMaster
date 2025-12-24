using FileMaster.Application.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileMaster.Application.Interfaces;

public interface IFileService
{
    Task<FileResponseDto> UploadFileAsync(IFormFile file, string userId);
    Task<List<FileResponseDto>> GetAllFilesAsync();
}