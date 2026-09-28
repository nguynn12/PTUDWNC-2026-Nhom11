using Microsoft.AspNetCore.Http.HttpResults;

namespace CulinaryBlog.API.Responses;

public static class ApiResponseExtensions
{
    /// <summary>
    /// Bật envelope <c>{data, meta}</c> cho 1 endpoint hoặc cả route group.
    /// KHÔNG dùng cho nhóm <c>/auth/*</c>: các endpoint trả token phải trả thẳng
    /// <c>{accessToken, refreshToken, expiresIn}</c> (ngoại lệ trong RESOLVED-CONFLICTS C2).
    /// </summary>
    public static TBuilder WithApiResponseEnvelope<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.AddEndpointFilter<TBuilder, ApiResponseEndpointFilter>();
}

/// <summary>Helper tạo kết quả đã bọc envelope cho các trường hợp filter không tự suy ra được.</summary>
public static class ApiResults
{
    /// <summary>201 Created + header Location + body <c>{"data": T}</c>.</summary>
    public static Created<ApiResponse<T>> Created<T>(string location, T data)
        => TypedResults.Created(location, new ApiResponse<T>(data));
}
