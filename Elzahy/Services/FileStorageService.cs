using Elzahy.DTOs;

namespace Elzahy.Services
{
    public interface IFileStorageService
    {
        Task<ApiResponse<FileUploadResult>> SaveImageAsync(IFormFile file, string subFolder = "images");
        Task<ApiResponse<FileUploadResult>> SaveVideoAsync(IFormFile file, string subFolder = "videos");
        Task<ApiResponse<bool>> DeleteFileAsync(string filePath);
        Task<ApiResponse<Stream>> GetFileStreamAsync(string filePath);
        string GetWebUrl(string filePath);
        bool FileExists(string filePath);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;
        private readonly List<string> _allowedImageTypes = new() { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private readonly List<string> _allowedVideoTypes = new() { "video/mp4", "video/webm", "video/ogg", "video/quicktime", "video/x-msvideo", "video/x-ms-wmv" };
        private readonly long _maxImageSize = 10 * 1024 * 1024; // 10MB
        private readonly long _maxVideoSize = 100 * 1024 * 1024; // 100MB

        public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<ApiResponse<FileUploadResult>> SaveImageAsync(IFormFile file, string subFolder = "images")
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                // Validate file
                var validation = ValidateImageFile(file);
                if (!validation.Ok)
                    return validation;

                // Generate unique filename
                var fileId = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{fileId}{extension}";

                // Create directory structure
                var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
                Directory.CreateDirectory(uploadDir);

                // Full file path
                var filePath = Path.Combine(uploadDir, fileName);
                var relativePath = Path.Combine("uploads", subFolder, fileName).Replace('\\', '/');

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = new FileUploadResult
                {
                    FilePath = relativePath,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    WebUrl = $"/uploads/{subFolder}/{fileName}"
                };

                _logger.LogInformation("[{TraceId}] Image saved successfully: {FilePath}", traceId, relativePath);
                return ApiResponse<FileUploadResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to save image", traceId);
                return ApiResponse<FileUploadResult>.Failure("Failed to save image", 5000);
            }
        }

        public async Task<ApiResponse<FileUploadResult>> SaveVideoAsync(IFormFile file, string subFolder = "videos")
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                // Validate file
                var validation = ValidateVideoFile(file);
                if (!validation.Ok)
                    return validation;

                // Generate unique filename
                var fileId = Guid.NewGuid();
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{fileId}{extension}";

                // Create directory structure
                var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
                Directory.CreateDirectory(uploadDir);

                // Full file path
                var filePath = Path.Combine(uploadDir, fileName);
                var relativePath = Path.Combine("uploads", subFolder, fileName).Replace('\\', '/');

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var result = new FileUploadResult
                {
                    FilePath = relativePath,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    WebUrl = $"/uploads/{subFolder}/{fileName}"
                };

                _logger.LogInformation("[{TraceId}] Video saved successfully: {FilePath}", traceId, relativePath);
                return ApiResponse<FileUploadResult>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to save video", traceId);
                return ApiResponse<FileUploadResult>.Failure("Failed to save video", 5000);
            }
        }

        public async Task<ApiResponse<bool>> DeleteFileAsync(string filePath)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return ApiResponse<bool>.Success(true); // Nothing to delete

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("[{TraceId}] File deleted successfully: {FilePath}", traceId, filePath);
                }

                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to delete file: {FilePath}", traceId, filePath);
                return ApiResponse<bool>.Failure("Failed to delete file", 5000);
            }
        }

        public async Task<ApiResponse<Stream>> GetFileStreamAsync(string filePath)
        {
            var traceId = Guid.NewGuid().ToString();
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return ApiResponse<Stream>.Failure("File path is required", 4001);

                var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("[{TraceId}] File not found: {FilePath}", traceId, filePath);
                    return ApiResponse<Stream>.Failure("File not found", 4004);
                }

                var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return ApiResponse<Stream>.Success(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{TraceId}] Failed to get file stream: {FilePath}", traceId, filePath);
                return ApiResponse<Stream>.Failure("Failed to get file", 5000);
            }
        }

        public string GetWebUrl(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return string.Empty;

            return filePath.StartsWith("/") ? filePath : $"/{filePath}";
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            return File.Exists(fullPath);
        }

        private ApiResponse<FileUploadResult> ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<FileUploadResult>.Failure("No file provided", 4001);

            if (!_allowedImageTypes.Contains(file.ContentType.ToLower()))
                return ApiResponse<FileUploadResult>.Failure("Invalid image format. Allowed: JPEG, PNG, GIF, WebP", 4002);

            if (file.Length > _maxImageSize)
                return ApiResponse<FileUploadResult>.Failure($"Image file too large. Maximum size is {_maxImageSize / (1024 * 1024)}MB", 4003);

            if (string.IsNullOrEmpty(file.FileName))
                return ApiResponse<FileUploadResult>.Failure("File name is required", 4001);

            return ApiResponse<FileUploadResult>.Success(new FileUploadResult()); // Validation passed
        }

        private ApiResponse<FileUploadResult> ValidateVideoFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return ApiResponse<FileUploadResult>.Failure("No file provided", 4001);

            if (!_allowedVideoTypes.Contains(file.ContentType.ToLower()))
                return ApiResponse<FileUploadResult>.Failure("Invalid video format. Allowed: MP4, WebM, OGG, MOV, AVI, WMV", 4002);

            if (file.Length > _maxVideoSize)
                return ApiResponse<FileUploadResult>.Failure($"Video file too large. Maximum size is {_maxVideoSize / (1024 * 1024)}MB", 4003);

            if (string.IsNullOrEmpty(file.FileName))
                return ApiResponse<FileUploadResult>.Failure("File name is required", 4001);

            return ApiResponse<FileUploadResult>.Success(new FileUploadResult()); // Validation passed
        }
    }
}