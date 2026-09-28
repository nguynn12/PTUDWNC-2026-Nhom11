namespace CulinaryBlog.Application.Features.RecipeImages.Validators;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeImages.DTOs;

/// <summary>
/// Trình kiểm thực nghiệp vụ cho hình ảnh công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-008) và RESOLVED-CONFLICTS.md (E6, E7).
/// </summary>
public static class RecipeImageValidator
{
    public static void ValidateUpdate(UpdateRecipeImageRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(request.AltText) && request.AltText.Length > 200)
        {
            errors["AltText"] = new[] { "Văn bản thay thế (AltText) không được vượt quá 200 ký tự." };
        }

        if (request.OrderIndex.HasValue && request.OrderIndex.Value < 0)
        {
            errors["OrderIndex"] = new[] { "Thứ tự hiển thị phải lớn hơn hoặc bằng 0." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }
}
