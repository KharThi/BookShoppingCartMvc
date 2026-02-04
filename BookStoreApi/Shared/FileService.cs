namespace BookStoreApi.Shared;

public interface IFileService
{
    void DeleteFile(string fileName);
    Task<string> SaveFile(IFormFile file, string[] allowedExtensions);
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFile(IFormFile file, string[] allowedExtensions)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentNullException(nameof(file));

        if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            throw new Exception("WebRootPath is NULL. Ensure wwwroot folder exists.");

        var path = Path.Combine(_environment.WebRootPath, "images");

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                $"Only {string.Join(", ", allowedExtensions)} files allowed");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var fileNameWithPath = Path.Combine(path, fileName);

        using var stream = new FileStream(fileNameWithPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/{fileName}";
    }

    public void DeleteFile(string fileName)
    {
        var wwwPath = _environment.WebRootPath;
        var fileNameWithPath = Path.Combine(wwwPath, "images\\", fileName);
        if (!File.Exists(fileNameWithPath))
            throw new FileNotFoundException(fileName);
        File.Delete(fileNameWithPath);

    }
}
