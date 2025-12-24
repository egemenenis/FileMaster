using FileMaster.Domain.Entities;
using FileMaster.Infrastructure.Persistence;
using FileMaster.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly string _uploadPath;
    private readonly ILogger<FilesController> _logger;

    public FilesController(AppDbContext context, ILogger<FilesController> logger)
    {
        _context = context;
        _logger = logger;
        _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        if (!Directory.Exists(_uploadPath))
            Directory.CreateDirectory(_uploadPath);
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Upload attempt started. User: {UserId}, FileName: {FileName}", userId, file?.FileName);

        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Upload failed: No file selected. User: {UserId}", userId);
                return BadRequest("Dosya seçilmedi.");
            }

            var allowedExtensions = new[] { ".jpg", ".png", ".pdf" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Upload blocked: Forbidden extension {Extension}. User: {UserId}", extension, userId);
                return BadRequest("Bu dosya tipine izin verilmiyor.");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                _logger.LogWarning("Upload blocked: File size too large ({Size} bytes). User: {UserId}", file.Length, userId);
                return BadRequest("Dosya boyutu çok büyük (Maks 10MB).");
            }

            var fileName = Guid.NewGuid() + extension;
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userFile = new UserFile
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                StoragePath = filePath,
                FileExtension = extension,
                UserId = userId ?? "Anonymous"
            };

            _context.UserFiles.Add(userFile);
            await _context.SaveChangesAsync();

            if (file.ContentType.StartsWith("image/"))
            {
                BackgroundJob.Enqueue<ImageService>(x => x.CreateThumbnail(userFile.Id, filePath));
                _logger.LogInformation("Thumbnail generation enqueued for FileId: {FileId}", userFile.Id);
            }

            _logger.LogInformation("File uploaded successfully. FileId: {FileId}, User: {UserId}", userFile.Id, userId);
            return Ok(new { userFile.Id, message = "Dosya yüklendi, thumbnail sıraya alındı." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file upload for User: {UserId}", userId);
            return StatusCode(500, "Dosya yüklenirken teknik bir hata oluştu.");
        }
    }

    [HttpGet("my-files")]
    public async Task<IActionResult> GetMyFiles([FromServices] IMemoryCache cache)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string cacheKey = $"files_{userId}";

        _logger.LogInformation("Fetching files for User: {UserId}", userId);

        if (!cache.TryGetValue(cacheKey, out List<UserFile>? files))
        {
            _logger.LogInformation("Cache miss for user files. Fetching from Database. User: {UserId}", userId);
            files = await _context.UserFiles.Where(x => x.UserId == userId).ToListAsync();
            cache.Set(cacheKey, files, TimeSpan.FromMinutes(10));
        }
        else
        {
            _logger.LogInformation("Cache hit for user files. User: {UserId}", userId);
        }

        return Ok(files);
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> Download(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Download requested. FileId: {FileId}, User: {UserId}", id, userId);

        var fileRecord = await _context.UserFiles.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (fileRecord == null)
        {
            _logger.LogWarning("Download failed: File not found or access denied. FileId: {FileId}, User: {UserId}", id, userId);
            return NotFound();
        }

        if (!System.IO.File.Exists(fileRecord.StoragePath))
        {
            _logger.LogError("Physical file missing on disk! Path: {Path}, FileId: {FileId}", fileRecord.StoragePath, id);
            return NotFound("Dosya fiziksel olarak mevcut değil.");
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(fileRecord.StoragePath);
        _logger.LogInformation("File downloaded successfully. FileId: {FileId}, User: {UserId}", id, userId);
        return File(bytes, fileRecord.ContentType, fileRecord.FileName);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogWarning("Delete attempt started. FileId: {FileId}, User: {UserId}", id, userId);

        var fileRecord = await _context.UserFiles.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

        if (fileRecord == null)
        {
            _logger.LogWarning("Delete failed: Unauthorized or Not Found. FileId: {FileId}, User: {UserId}", id, userId);
            return NotFound("Dosya bulunamadı veya bu işlem için yetkiniz yok.");
        }

        try
        {
            if (System.IO.File.Exists(fileRecord.StoragePath))
            {
                System.IO.File.Delete(fileRecord.StoragePath);
                _logger.LogInformation("Physical file deleted from disk. Path: {Path}", fileRecord.StoragePath);
            }

            _context.UserFiles.Remove(fileRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Database record removed. FileId: {FileId}, User: {UserId}", id, userId);
            return Ok(new { message = "Dosya başarıyla silindi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during file deletion. FileId: {FileId}", id);
            return StatusCode(500, "Silme işlemi sırasında bir hata oluştu.");
        }
    }
}