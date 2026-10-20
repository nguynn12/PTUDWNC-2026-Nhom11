using CulinaryBlog.Application.Auth.Commands.ConfirmEmail;
using CulinaryBlog.Application.Auth.Commands.ResendConfirmationEmail;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Infrastructure.Services;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-008 (xác nhận email) và FR-AUTH-009 (gửi lại email xác nhận).</summary>
public sealed class EmailConfirmationTests
{
    private static UserAccount Account(bool emailConfirmed = false, bool isActive = true) =>
        new("user-1", "an@example.com", "Nguyễn An", null, null, ["Author"], emailConfirmed, isActive, DateTimeOffset.UtcNow);

    [Fact]
    public async Task XacNhanEmail_TokenHopLe_KhongLoi()
    {
        var handler = new ConfirmEmailCommandHandler(new FakeIdentityService { ConfirmEmailResult = true });

        await handler.Handle(new ConfirmEmailCommand("user-1", "token"), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task XacNhanEmail_TokenSai_Tra422()
    {
        var handler = new ConfirmEmailCommandHandler(new FakeIdentityService { ConfirmEmailResult = false });

        var ex = await Assert.ThrowsAsync<BusinessRuleValidationException>(
            () => handler.Handle(new ConfirmEmailCommand("user-1", "sai"), TestContext.Current.CancellationToken));

        Assert.Equal("VALIDATION_ERROR", ex.ErrorCode); // SRS Phụ lục B
    }

    [Fact]
    public void XacNhanEmail_ThieuUserIdHoacToken_KhongHopLe()
    {
        var result = new ConfirmEmailCommandValidator().Validate(new ConfirmEmailCommand("", ""));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfirmEmailCommand.UserId));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ConfirmEmailCommand.Token));
    }

    [Fact]
    public async Task GuiLai_EmailChuaXacNhan_GuiEmail()
    {
        var sender = new FakeAccountEmailSender();
        var handler = new ResendConfirmationEmailCommandHandler(new FakeIdentityService { Account = Account() }, sender);

        await handler.Handle(new ResendConfirmationEmailCommand("an@example.com"), TestContext.Current.CancellationToken);

        var sent = Assert.Single(sender.Sent);
        Assert.Equal("user-1", sent.UserId);
    }

    [Fact]
    public async Task GuiLai_EmailKhongTonTai_KhongLoiVaKhongGui()
    {
        var sender = new FakeAccountEmailSender();
        var handler = new ResendConfirmationEmailCommandHandler(new FakeIdentityService { Account = null }, sender);

        await handler.Handle(new ResendConfirmationEmailCommand("khong-co@example.com"), TestContext.Current.CancellationToken);

        Assert.Empty(sender.Sent);
    }

    [Fact]
    public async Task GuiLai_EmailDaXacNhan_KhongGui()
    {
        var sender = new FakeAccountEmailSender();
        var handler = new ResendConfirmationEmailCommandHandler(
            new FakeIdentityService { Account = Account(emailConfirmed: true) }, sender);

        await handler.Handle(new ResendConfirmationEmailCommand("an@example.com"), TestContext.Current.CancellationToken);

        Assert.Empty(sender.Sent);
    }

    [Fact]
    public void LinkXacNhan_MaHoaUserIdVaToken()
    {
        var link = AccountEmailSender.BuildConfirmationLink("http://localhost:3000/", "user 1", "a+b/c=");

        Assert.Equal("http://localhost:3000/auth/confirm-email?userId=user%201&token=a%2Bb%2Fc%3D", link);
    }
}
