namespace CulinaryBlog.Application.Features.RecipeSteps.Validators;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeSteps.DTOs;

/// <summary>
/// Trình kiểm thực nghiệp vụ cho các bước thực hiện công thức nấu ăn.
/// Tuân thủ SRS v1.2.0 (FR-RCP-010) và RESOLVED-CONFLICTS.md (E5).
/// </summary>
public static class RecipeStepValidator
{
    public static void ValidateCreate(CreateRecipeStepRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(request.Title) && request.Title.Length > 200)
        {
            errors["Title"] = new[] { "Tiêu đề bước nấu không được vượt quá 200 ký tự." };
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors["Description"] = new[] { "Nội dung hướng dẫn chi tiết của bước nấu không được để trống." };
        }
        else if (request.Description.Length > 2000)
        {
            errors["Description"] = new[] { "Nội dung hướng dẫn chi tiết không được vượt quá 2000 ký tự." };
        }

        if (request.DurationMinutes.HasValue && request.DurationMinutes.Value < 0)
        {
            errors["DurationMinutes"] = new[] { "Thời gian thực hiện phải lớn hơn hoặc bằng 0 phút." };
        }

        if (request.StepNumber.HasValue && request.StepNumber.Value <= 0)
        {
            errors["StepNumber"] = new[] { "Thứ tự bước nấu (StepNumber) phải là số nguyên dương lớn hơn 0." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    public static void ValidateUpdate(UpdateRecipeStepRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(request.Title) && request.Title.Length > 200)
        {
            errors["Title"] = new[] { "Tiêu đề bước nấu không được vượt quá 200 ký tự." };
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            errors["Description"] = new[] { "Nội dung hướng dẫn chi tiết của bước nấu không được để trống." };
        }
        else if (request.Description.Length > 2000)
        {
            errors["Description"] = new[] { "Nội dung hướng dẫn chi tiết không được vượt quá 2000 ký tự." };
        }

        if (request.DurationMinutes.HasValue && request.DurationMinutes.Value < 0)
        {
            errors["DurationMinutes"] = new[] { "Thời gian thực hiện phải lớn hơn hoặc bằng 0 phút." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    public static void ValidateReorder(ReorderRecipeStepsRequest request)
    {
        if (request.StepIds == null || request.StepIds.Count == 0)
        {
            throw new BadRequestException("Danh sách các bước sắp xếp lại không được để trống.");
        }

        if (request.StepIds.Distinct().Count() != request.StepIds.Count)
        {
            throw new BadRequestException("Danh sách mã bước chứa các phần tử trùng lặp.");
        }
    }
}
