using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace QwenWebAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private bool IsAuthValid()
        {
            if (!Request.Headers.TryGetValue("Auth", out var authHeader))
                return false;
            
            if (string.IsNullOrEmpty(Runtimes.AuthToken))
                return false;

            return string.Equals(authHeader.ToString(), Runtimes.AuthToken, StringComparison.Ordinal);
        }

        private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".doc", ".docs", ".txt", ".md", ".ppt", ".pptx",
            ".cs", ".cpp", ".c", ".h", ".hpp", ".py", ".ps1"
        };

        private static readonly ConcurrentDictionary<string, (string FilePath, DateTime ExpireAt)> TempLinks = new();

        private readonly string _filesDirectory;

        public FileController()
        {
            _filesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "files");
            if (!Directory.Exists(_filesDirectory))
                Directory.CreateDirectory(_filesDirectory);
        }


        [HttpPost("upload")]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {
            if (!IsAuthValid())
                return Unauthorized("无效的Auth请求头");

            if (file == null || file.Length == 0)
                return BadRequest("未提供文件");

            var originalName = file.FileName;
            var extension = Path.GetExtension(originalName).ToLowerInvariant();
            if (!AllowedTypes.Contains(extension))
                return BadRequest("仅允许上传图片文件 (jpg, jpeg, png, gif, webp, bmp)");

            var safeFileName = Guid.NewGuid().ToString("N") + extension;
            var filePath = Path.Combine(_filesDirectory, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var token = Guid.NewGuid().ToString("N");
            var expireAt = DateTime.UtcNow.AddMinutes(10);

            TempLinks[token] = (filePath, expireAt);

            var now = DateTime.UtcNow;
            var expiredKeys = TempLinks.Where(kvp => kvp.Value.ExpireAt < now)
                                       .Select(kvp => kvp.Key)
                                       .ToList();
            foreach (var key in expiredKeys)
                TempLinks.TryRemove(key, out _);

            var downloadUrl = $"{Request.Scheme}://{Request.Host}/File/download/{token}";
            return Ok(new { url = downloadUrl, expires_in = 600 });
        }

        [HttpGet("download/{token}")]
        public async Task<ActionResult> DownloadByToken(string token)
        {
            if (!TempLinks.TryGetValue(token, out var record))
                return NotFound("链接不存在或已失效");

            if (record.ExpireAt < DateTime.UtcNow)
            {
                TempLinks.TryRemove(token, out _);
                return NotFound("链接已过期");
            }

            if (!System.IO.File.Exists(record.FilePath))
                return NotFound("文件不存在");

            var contentType = GetContentType(Path.GetExtension(record.FilePath));
            var fileBytes = await System.IO.File.ReadAllBytesAsync(record.FilePath);
            return File(fileBytes, contentType, Path.GetFileName(record.FilePath));
        }

        private static string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
