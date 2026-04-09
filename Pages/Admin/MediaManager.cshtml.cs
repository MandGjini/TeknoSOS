using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace TeknoSOS.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class MediaManagerModel : PageModel
    {
        private readonly IWebHostEnvironment _env;

        public MediaManagerModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public List<MediaFile> MediaFiles { get; set; } = new();
        public long TotalSizeBytes { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Folder { get; set; }

        public class MediaFile
        {
            public string FileName { get; set; } = string.Empty;
            public string RelativePath { get; set; } = string.Empty;
            public long SizeBytes { get; set; }
            public string SizeFormatted => SizeBytes < 1024 ? $"{SizeBytes} B" : SizeBytes < 1024 * 1024 ? $"{SizeBytes / 1024} KB" : $"{SizeBytes / 1024 / 1024} MB";
            public string Extension { get; set; } = string.Empty;
            public DateTime LastModified { get; set; }
            public bool IsImage => new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" }.Contains(Extension.ToLower());
        }

        public void OnGet()
        {
            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                return;
            }

            var searchFolder = string.IsNullOrEmpty(Folder) ? uploadsPath : Path.Combine(uploadsPath, Folder);
            if (!Directory.Exists(searchFolder)) return;

            var files = Directory.GetFiles(searchFolder, "*.*", SearchOption.AllDirectories);
            
            MediaFiles = files.Select(f =>
            {
                var fi = new FileInfo(f);
                return new MediaFile
                {
                    FileName = fi.Name,
                    RelativePath = "/uploads/" + Path.GetRelativePath(uploadsPath, f).Replace("\\", "/"),
                    SizeBytes = fi.Length,
                    Extension = fi.Extension,
                    LastModified = fi.LastWriteTimeUtc
                };
            }).OrderByDescending(mf => mf.LastModified).ToList();

            TotalSizeBytes = MediaFiles.Sum(f => f.SizeBytes);
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Request.Form.Files.Count == 0)
                return RedirectToPage();

            var uploadsPath = Path.Combine(_env.WebRootPath, "uploads", "media");
            Directory.CreateDirectory(uploadsPath);

            foreach (var file in Request.Form.Files)
            {
                if (file.Length > 20 * 1024 * 1024) continue; // 20MB max

                var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadsPath, uniqueName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);
            }

            return RedirectToPage();
        }

        public IActionResult OnPostDelete(string path)
        {
            if (string.IsNullOrEmpty(path)) return RedirectToPage();

            var fullPath = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            return RedirectToPage();
        }
    }
}
