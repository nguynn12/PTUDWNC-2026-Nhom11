using CulinaryBlog.Application.Auth.Commands.Login;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>
/// SRS FR-AUTH-002 (A1 401, A2 423, A3 khoá sau 5 lần sai) và FR-AUTH-010 (tài khoản bị vô hiệu hoá).
/// Các nhánh lỗi không được ghi refresh token nào.
/// </summary>
public sealed class LoginCommandHandlerTests
{
    private static LoginCommandHandler CreateHandler(CredentialCheckResult result) =>
        new(new FakeIdentityService { CredentialResult = result }, new FakeJwtService(), new FakeUnitOfWork());

    private static readonly LoginCommand Command = new("author@culinaryblog.local", "Sai-mat-khau1!");

    [Fact]
    public async Task SaiEmailHoacMatKhau_Tra401ThongBaoChung()
    {
        var handler = CreateHandler(new CredentialCheckResult(CredentialCheckStatus.InvalidCredentials));

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthInvalidCredentials, ex.ErrorCode);
        Assert.Equal(LoginCommandHandler.InvalidCredentialsMessage, ex.Message);
    }

    [Fact]
    public async Task TaiKhoanBiKhoaTam_Tra423KemThoiGianConLai()
    {
        var handler = CreateHandler(new CredentialCheckResult(
            CredentialCheckStatus.LockedOut, LockoutEnd: DateTimeOffset.UtcNow.AddMinutes(15)));

        var ex = await Assert.ThrowsAsync<LockedException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthAccountLocked, ex.ErrorCode);
        Assert.Equal(AppErrorKind.Locked, ex.Kind);
        Assert.Contains("phút", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TaiKhoanBiVoHieuHoa_Tra403()
    {
        var handler = CreateHandler(new CredentialCheckResult(CredentialCheckStatus.Disabled));

        var ex = await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthAccountDisabled, ex.ErrorCode);
    }

    [Fact]
    public void BuildLockedMessage_KhongCoThoiDiemMoKhoa_VanCoThongBao()
    {
        Assert.False(string.IsNullOrWhiteSpace(LoginCommandHandler.BuildLockedMessage(null)));
    }
}
