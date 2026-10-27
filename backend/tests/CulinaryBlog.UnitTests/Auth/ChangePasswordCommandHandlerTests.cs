using CulinaryBlog.Application.Auth.Commands.ChangePassword;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

public sealed class ChangePasswordCommandHandlerTests
{
    private static UserAccount Account() =>
        new("user-1", "an@example.com", "Nguyễn An", null, null, ["Author"], true, true, DateTimeOffset.UtcNow);

    [Fact]
    public async Task DoiMatKhauThanhCong_ThuHoiTokenCuVaTraPhienMoi()
    {
        var identity = new FakeIdentityService { Account = Account(), ChangePasswordResult = true };
        var currentUser = new FakeCurrentUser("user-1");
        var tokens = new FakeRefreshTokenRepository();
        tokens.Add(Domain.Entities.RefreshToken.CreateNewFamily("user-1", "old-hash", 7, "127.0.0.1"));
        var handler = new ChangePasswordCommandHandler(
            currentUser, identity, new FakeJwtService(), tokens, new FakeUnitOfWork(), new FakeClientInfo());

        var response = await handler.Handle(
            new ChangePasswordCommand("OldPassword1!", "NewPassword1!"),
            TestContext.Current.CancellationToken);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("raw-refresh-token", response.RefreshToken);
        Assert.Equal(2, tokens.Tokens.Count);
        Assert.Single(tokens.Tokens, token => token.IsRevoked);
        Assert.Single(tokens.Tokens, token => !token.IsRevoked);
    }

    [Fact]
    public async Task DoiMatKhauSaiMatKhauHienTai_TraAuthInvalidCredentials()
    {
        var handler = new ChangePasswordCommandHandler(
            new FakeCurrentUser("user-1"),
            new FakeIdentityService { Account = Account(), ChangePasswordResult = false },
            new FakeJwtService(),
            new FakeRefreshTokenRepository(),
            new FakeUnitOfWork(),
            new FakeClientInfo());

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            handler.Handle(
                new ChangePasswordCommand("wrong", "NewPassword1!"),
                TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthInvalidCredentials, exception.ErrorCode);
    }

    [Fact]
    public void MatKhauMoiYeu_BiValidatorTuChoi()
    {
        var result = new ChangePasswordCommandValidator()
            .Validate(new ChangePasswordCommand("OldPassword1!", "weak"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ChangePasswordCommand.NewPassword));
    }
}

internal sealed class FakeCurrentUser(string? userId) : ICurrentUserService
{
    public string? UserId { get; } = userId;
    public bool IsAdmin => false;
}
