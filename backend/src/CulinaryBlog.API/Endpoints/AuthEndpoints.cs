using CulinaryBlog.API.RateLimiting;
using CulinaryBlog.Application.Auth.Commands.GoogleLogin;
using CulinaryBlog.Application.Auth.Commands.Login;
using CulinaryBlog.Application.Auth.Commands.Logout;
using CulinaryBlog.Application.Auth.Commands.RefreshToken;
using CulinaryBlog.Application.Auth.Commands.Register;
using CulinaryBlog.Application.Auth.Commands.UpdateProfile;
using CulinaryBlog.Application.Auth.Commands.ConfirmEmail;
using CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;
using CulinaryBlog.Application.Auth.Commands.ForgotPassword;
using CulinaryBlog.Application.Auth.Commands.ResetPassword;
using CulinaryBlog.Application.Auth.Commands.ChangePassword;
using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Gắn vào nhóm /api/v{version} (Program.cs) nên đường dẫn thực tế là /api/v1/auth.
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .RequireRateLimiting(AuthRateLimiting.PolicyName);

        // FR-AUTH-001: 201 Created; body token ở top-level (RESOLVED-CONFLICTS C2 — ngoại lệ /auth/*).
        group.MapPost("/register", async (RegisterCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created("/api/v1/auth/me", result);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        });

        // FR-AUTH-003: đăng nhập bằng Google ID token (Google Identity Services ở frontend).
        group.MapPost("/google", async (GoogleLoginCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        });

        group.MapPost("/refresh", async (RefreshTokenCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        });

        group.MapPost("/email/confirm", async (ConfirmEmailCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        });

        // FR-AUTH-009: luôn 202 + message chung (không tiết lộ email có tồn tại hay không).
        group.MapPost("/email/resend", async (ResendConfirmationEmailCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.Accepted(value: new { message = ResendConfirmationEmailCommandHandler.GenericMessage });
        });

        group.MapPost("/password/forgot", async (ForgotPasswordCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        });

        group.MapPost("/password/reset", async (ResetPasswordCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        });

        group.MapPost("/password/change", async (ChangePasswordCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        // RESOLVED-CONFLICTS D3: logout chỉ cần refreshToken trong body, KHÔNG yêu cầu access
        // token còn hạn (access token có thể đã hết hạn khi người dùng bấm đăng xuất).
        group.MapPost("/logout", async (LogoutCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
        });

        group.MapGet("/me", async (ISender sender) =>
        {
            var result = await sender.Send(new CulinaryBlog.Application.Auth.Queries.GetProfile.GetProfileQuery());
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPatch("/me", async (UpdateProfileCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/me/avatar", async (
            IFormFile file,
            ICurrentUserService currentUser,
            IIdentityService identity,
            IFileStorageService storage,
            CancellationToken cancellationToken) =>
        {
            if (currentUser.UserId is not { Length: > 0 } userId)
                return Results.Unauthorized();
            if (file.Length is <= 0 or > 5 * 1024 * 1024)
                return Results.Problem("Ảnh đại diện phải có kích thước từ 1 byte đến 5 MB.", statusCode: 400);

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return Results.Problem("Chỉ hỗ trợ ảnh JPG, PNG hoặc WebP.", statusCode: 400);

            await using var input = file.OpenReadStream();
            var header = new byte[12];
            var read = await input.ReadAsync(header, cancellationToken);
            if (!IsSupportedImage(header, read))
                return Results.Problem("Nội dung tệp không phải ảnh hợp lệ.", statusCode: 400);
            input.Position = 0;

            var user = await identity.GetUserDetailsByIdAsync(userId);
            if (user is null)
                return Results.Unauthorized();
            if (!user.IsActive)
                return Results.Problem("Tài khoản đã bị vô hiệu hóa.", statusCode: 403);

            var avatarUrl = await storage.UploadAsync(input, file.FileName, file.ContentType, "avatars", cancellationToken);
            if (!await identity.UpdateProfileAsync(userId, null, null, avatarUrl))
            {
                await storage.DeleteAsync(avatarUrl, cancellationToken);
                return Results.Problem("Không cập nhật được ảnh đại diện.", statusCode: 500);
            }

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl))
                await storage.DeleteAsync(user.AvatarUrl, cancellationToken);

            var updated = await identity.GetUserDetailsByIdAsync(userId);
            return updated is null
                ? Results.Problem("Không tải lại được hồ sơ.", statusCode: 500)
                : Results.Ok(updated.ToUserDto());
        }).RequireAuthorization();
    }

    private static bool IsSupportedImage(byte[] bytes, int length) =>
        (length >= 3 && bytes[0] == 0xff && bytes[1] == 0xd8 && bytes[2] == 0xff) ||
        (length >= 8 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) ||
        (length >= 12 && bytes[..4].SequenceEqual("RIFF"u8.ToArray()) &&
         bytes[8..12].SequenceEqual("WEBP"u8.ToArray()));
}
