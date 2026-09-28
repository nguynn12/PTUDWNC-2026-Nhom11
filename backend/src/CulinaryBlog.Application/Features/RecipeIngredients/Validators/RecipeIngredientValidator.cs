namespace CulinaryBlog.Application.Features.RecipeIngredients.Validators;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.RecipeIngredients.DTOs;

/// <summary>
/// Trình kiểm thực nghiệp vụ cho nguyên liệu công thức nấu ăn.
/// Tuân thủ nghiêm ngặt SRS v1.2.0 (FR-RCP-009) và RESOLVED-CONFLICTS.md (E1, E2).
/// </summary>
public static class RecipeIngredientValidator
{
    public static void ValidateCreate(CreateRecipeIngredientRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Tên nguyên liệu không được để trống." };
        }
        else if (request.Name.Length > 200)
        {
            errors["Name"] = new[] { "Tên nguyên liệu không được vượt quá 200 ký tự." };
        }

        ValidateQuantityAndUnit(request.Quantity, request.Unit, errors);

        if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Length > 500)
        {
            errors["Notes"] = new[] { "Ghi chú không được vượt quá 500 ký tự." };
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

    public static void ValidateUpdate(UpdateRecipeIngredientRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["Name"] = new[] { "Tên nguyên liệu không được để trống." };
        }
        else if (request.Name.Length > 200)
        {
            errors["Name"] = new[] { "Tên nguyên liệu không được vượt quá 200 ký tự." };
        }

        ValidateQuantityAndUnit(request.Quantity, request.Unit, errors);

        if (!string.IsNullOrWhiteSpace(request.Notes) && request.Notes.Length > 500)
        {
            errors["Notes"] = new[] { "Ghi chú không được vượt quá 500 ký tự." };
        }

        if (request.OrderIndex < 0)
        {
            errors["OrderIndex"] = new[] { "Thứ tự hiển thị phải lớn hơn hoặc bằng 0." };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    /// <summary>
    /// Kiểm thực quy tắc số lượng và đơn vị (Quyết định E2):
    /// - Cả 2 có thể cùng null để thể hiện 'vừa đủ'.
    /// - Nếu có Quantity thì bắt buộc phải có Unit và Quantity > 0.
    /// - Nếu có Unit thì bắt buộc phải có Quantity.
    /// </summary>
    private static void ValidateQuantityAndUnit(decimal? quantity, string? unit, Dictionary<string, string[]> errors)
    {
        bool hasQuantity = quantity.HasValue;
        bool hasUnit = !string.IsNullOrWhiteSpace(unit);

        if (hasQuantity && !hasUnit)
        {
            errors["Unit"] = new[] { "Đơn vị đo lường là bắt buộc khi đã nhập định lượng (Quyết định E2)." };
        }
        else if (!hasQuantity && hasUnit)
        {
            errors["Quantity"] = new[] { "Định lượng số học là bắt buộc khi đã nhập đơn vị đo lường (Quyết định E2)." };
        }

        if (hasQuantity && quantity!.Value <= 0)
        {
            errors["Quantity"] = new[] { "Định lượng nguyên liệu phải lớn hơn 0." };
        }

        if (hasUnit && unit!.Length > 50)
        {
            errors["Unit"] = new[] { "Đơn vị đo lường không được vượt quá 50 ký tự." };
        }
    }
}
