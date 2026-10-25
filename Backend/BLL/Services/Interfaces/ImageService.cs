
using AutoMapper;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using DTO;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Interfaces
{
    public interface IImageService
    {
        string Save(IFormFile file);
        void Delete(string fileName);
    }

    public class ImageService : IImageService
    {
        private readonly string _folder;

        public ImageService(string folder)
        {
            _folder = folder;
            Directory.CreateDirectory(_folder);
        }

        public string Save(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("לא נבחר קובץ.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowed.Contains(ext))
                throw new ArgumentException(
                    "מותר להעלות רק קובצי jpg, jpeg, png או webp.");

            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("גודל הקובץ המרבי הוא 5MB.");

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(_folder, fileName);

            using (var stream = File.Create(path))
            {
                file.CopyTo(stream);
            }

            return fileName;
        }

        public void Delete(string fileName)
        {
            var safeFileName = Path.GetFileName(fileName);
            var path = Path.Combine(_folder, safeFileName);

            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
