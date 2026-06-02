using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace WebAPI.Entities
{
    public partial class Laptop
    {
        private string? _imageUrl;

        [NotMapped]
        public string ImageUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_imageUrl)) return _imageUrl;

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                foreach (var ext in allowedExtensions)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"{LaptopCode}{ext}");
                    if (File.Exists(filePath))
                    {
                        return $"/uploads/{LaptopCode}{ext}";
                    }
                }
                return "/images/placeholder.jpg"; // Fallback placeholder
            }
            set
            {
                _imageUrl = value;
            }
        }
    }
}
