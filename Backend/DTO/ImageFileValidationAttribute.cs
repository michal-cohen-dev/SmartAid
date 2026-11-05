using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DTO;

public class ImageFileValidationAttribute : ValidationAttribute
{
    private const int MaxFileSize = 5 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    protected override ValidationResult? IsValid(
        object? value,
        ValidationContext validationContext)
    {
        // תמונה היא שדה אופציונלי
        if (value == null)
            return ValidationResult.Success;

        if (value is not IFormFile file)
        {
            return new ValidationResult(
                "Invalid image file.");
        }

        if (file.Length == 0)
        {
            return new ValidationResult(
                "The image file is empty.");
        }

        if (file.Length > MaxFileSize)
        {
            return new ValidationResult(
                "The maximum image size is 5MB.");
        }

        var extension = Path.GetExtension(file.FileName);

        if (!AllowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase))
        {
            return new ValidationResult(
                "Only jpg, jpeg, png and webp files are allowed.");
        }

        return ValidationResult.Success;
    }
}