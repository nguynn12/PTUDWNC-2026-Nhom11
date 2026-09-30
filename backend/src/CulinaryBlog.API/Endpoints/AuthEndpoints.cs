using CulinaryBlog.Application.Auth.Commands.Login;
using CulinaryBlog.Application.Auth.Commands.Logout;
using CulinaryBlog.Application.Auth.Commands.RefreshToken;
using CulinaryBlog.Application.Auth.Commands.Register;
using CulinaryBlog.Application.Auth.Commands.UpdateProfile;
using CulinaryBlog.Application.Auth.Commands.ConfirmEmail;
using CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;
using CulinaryBlog.Application.Auth.Commands.ForgotPassword;
using CulinaryBlog.Application.Auth.Commands.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .RequireRateLimiting("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Ok(result);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender) =>
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

        group.MapPost("/email/resend", async (ResendConfirmationEmailCommand command, ISender sender) =>
        {
            await sender.Send(command);
            return Results.NoContent();
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
    }
}
