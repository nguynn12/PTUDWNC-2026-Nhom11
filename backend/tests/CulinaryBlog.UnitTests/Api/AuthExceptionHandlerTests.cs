using System.Text.Json;
using CulinaryBlog.API.Middlewares;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>
/// Kiểm tra chuỗi xử lý lỗi giống Program.cs: AuthExceptionHandler chạy trước, exception còn lại
/// chuyển cho GlobalExceptionHandler dùng chung của nhóm. Body trả về theo RFC 7807.
/// </summary>
public sealed class AuthExceptionHandlerTests
{
    [Fact]
    public async Task SaiThongTinDangNhap_Tra401()
    {
        var (status, body, handledByAuth) = await HandleAsync(
            new UnauthorizedException("Email hoặc mật khẩu không đúng.", ErrorCodes.AuthInvalidCredentials));

        Assert.True(handledByAuth);
        Assert.Equal(StatusCodes.Status401Unauthorized, status);
        Assert.Equal("AUTH_INVALID_CREDENTIALS", body.GetProperty("type").GetString());
        Assert.Equal("Email hoặc mật khẩu không đúng.", body.GetProperty("detail").GetString());
        Assert.Equal("/api/v1/auth/login", body.GetProperty("instance").GetString());
    }

    [Fact]
    public async Task TaiKhoanBiKhoaTam_Tra423()
    {
        var (status, body, _) = await HandleAsync(new LockedException("Tài khoản đang bị tạm khoá."));

        Assert.Equal(StatusCodes.Status423Locked, status);
        Assert.Equal("AUTH_ACCOUNT_LOCKED", body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task RefreshTokenBiThuHoi_Tra401ThayVi400CuaDomainException()
    {
        var (status, body, handledByAuth) = await HandleAsync(new RefreshTokenRevokedException());

        Assert.True(handledByAuth);
        Assert.Equal(StatusCodes.Status401Unauthorized, status);
        Assert.Equal(RefreshTokenRevokedException.Code, body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task RefreshTokenHetHan_Tra401()
    {
        var (status, body, _) = await HandleAsync(new RefreshTokenExpiredException());

        Assert.Equal(StatusCodes.Status401Unauthorized, status);
        Assert.Equal(RefreshTokenExpiredException.Code, body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task JsonSaiCuPhap_Tra400MalformedRequest()
    {
        var (status, body, _) = await HandleAsync(new BadHttpRequestException("Failed to read parameter"));

        Assert.Equal(StatusCodes.Status400BadRequest, status);
        Assert.Equal("MALFORMED_REQUEST", body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task EmailDaTonTai_DoGlobalExceptionHandlerTra409()
    {
        var (status, body, handledByAuth) = await HandleAsync(
            new ConflictException("Email đã được đăng ký.", ErrorCodes.AuthEmailExists));

        Assert.False(handledByAuth);
        Assert.Equal(StatusCodes.Status409Conflict, status);
        Assert.Equal("AUTH_EMAIL_EXISTS", body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task TaiKhoanBiVoHieuHoa_DoGlobalExceptionHandlerTra403()
    {
        var (status, body, handledByAuth) = await HandleAsync(
            new ForbiddenException("Tài khoản đã bị quản trị viên vô hiệu hoá.", ErrorCodes.AuthAccountDisabled));

        Assert.False(handledByAuth);
        Assert.Equal(StatusCodes.Status403Forbidden, status);
        Assert.Equal("AUTH_ACCOUNT_DISABLED", body.GetProperty("type").GetString());
    }

    [Fact]
    public async Task LoiMatKhauCuaIdentity_DoGlobalExceptionHandlerTra422TheoField()
    {
        var (status, body, handledByAuth) = await HandleAsync(
            new FluentValidation.ValidationException([new ValidationFailure("password", "Mật khẩu phải có ký tự đặc biệt.")]));

        Assert.False(handledByAuth);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, status);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("type").GetString());
        Assert.Equal(
            "Mật khẩu phải có ký tự đặc biệt.",
            body.GetProperty("errors").GetProperty("password")[0].GetString());
    }

    private static async Task<(int Status, JsonElement Body, bool HandledByAuth)> HandleAsync(Exception exception)
    {
        using var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .BuildServiceProvider();

        using var responseBody = new MemoryStream();
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.Method = HttpMethods.Post;
        httpContext.Request.Path = "/api/v1/auth/login";
        httpContext.Response.Body = responseBody;

        var problemDetailsService = services.GetRequiredService<IProblemDetailsService>();
        var authHandler = new AuthExceptionHandler(problemDetailsService, NullLogger<AuthExceptionHandler>.Instance);
        var globalHandler = new GlobalExceptionHandler(problemDetailsService, NullLogger<GlobalExceptionHandler>.Instance);

        var cancellationToken = TestContext.Current.CancellationToken;
        var handledByAuth = await authHandler.TryHandleAsync(httpContext, exception, cancellationToken);
        var handled = handledByAuth || await globalHandler.TryHandleAsync(httpContext, exception, cancellationToken);

        Assert.True(handled);
        responseBody.Position = 0;
        using var json = JsonDocument.Parse(responseBody);
        return (httpContext.Response.StatusCode, json.RootElement.Clone(), handledByAuth);
    }
}
