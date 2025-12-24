using FileMaster.Infrastructure.Persistence;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using static System.Net.Mime.MediaTypeNames;

namespace FileMaster.Infrastructure.Services;

public class ImageService
{
    private readonly AppDbContext _context;

    public ImageService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateThumbnail(Guid fileId, string filePath)
    {
        var fileRecord = await _context.UserFiles.FindAsync(fileId);
        if (fileRecord == null) return;

        var thumbnailName = "thumb_" + Path.GetFileName(filePath);
        var thumbnailPath = Path.Combine(Path.GetDirectoryName(filePath)!, thumbnailName);

        using (var image = await SixLabors.ImageSharp.Image.LoadAsync(filePath))
        {
            image.Mutate(x => x.Resize(150, 150));
            await image.SaveAsync(thumbnailPath);
        }

        fileRecord.ThumbnailPath = thumbnailPath;
        await _context.SaveChangesAsync();
    }
}