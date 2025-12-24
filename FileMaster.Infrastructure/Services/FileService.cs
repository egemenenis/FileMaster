using System;
using System.Collections.Generic;
using System.Text;

namespace FileMaster.Infrastructure.Services;

public class FileService
{
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
    {
        if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);

        var filePath = Path.Combine(_uploadPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(stream);

        return filePath;
    }
}