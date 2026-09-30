using CulinaryBlog.Application.Auth.Commands.Register;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using FluentValidation;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-001: email trùng → 409 AUTH_EMAIL_EXISTS; lỗi policy Identity → 422 theo field.</summary>
public sealed class RegisterCommandHandlerTests
{
    private static readonly RegisterCommand Command = new("an@example.com", "Passw0rd!", "Nguyễn An");

    private static RegisterCommandHandler CreateHandler(CreateUserResult result, FakeUnitOfWork unitOfWork) =>
        new(new FakeIdentityService { CreateResult = result }, new FakeJwtService(), unitOfWork.Tokens, unitOfWork,
            new FakeAccountEmailSender(), NullLogger<RegisterCommandHandler>.Instance);

    [Fact]
    public async Task EmailDaTonTai_Tra409AuthEmailExists()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(CreateUserResult.Duplicate(), unitOfWork);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthEmailExists, ex.ErrorCode);
        Assert.Empty(unitOfWork.Tokens.Tokens);
        Assert.Equal(0, unitOfWork.SaveChangesCount);
    }

    [Fact]
    public async Task IdentityTuChoiMatKhau_Tra422KemLoiTheoField()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["password"] = ["Passwords must have at least one non alphanumeric character."],
        };
        var handler = CreateHandler(CreateUserResult.Failed(errors), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        var failure = Assert.Single(ex.Errors);
        Assert.Equal("password", failure.PropertyName);
    }

    private static FakeIdentityService IdentityWithNewUser() => new()
    {
        CreateResult = CreateUserResult.Success("user-1"),
        Account = new UserAccount("user-1", "an@example.com", "Nguyễn An", null, null, ["Author"], false, true, DateTimeOffset.UtcNow),
    };

    [Fact]
    public async Task DangKyThanhCong_LuuRefreshTokenVaGuiEmailXacNhan()
    {
        var unitOfWork = new FakeUnitOfWork();
        var emailSender = new FakeAccountEmailSender();
        var handler = new RegisterCommandHandler(IdentityWithNewUser(), new FakeJwtService(), unitOfWork.Tokens, unitOfWork,
            emailSender, NullLogger<RegisterCommandHandler>.Instance);

        var response = await handler.Handle(Command, TestContext.Current.CancellationToken);

        Assert.Contains("Author", response.User.Roles);
        Assert.False(response.User.EmailConfirmed);
        Assert.Single(unitOfWork.Tokens.Tokens);
        Assert.Equal(1, unitOfWork.SaveChangesCount);

        var sent = Assert.Single(emailSender.Sent);
        Assert.Equal("user-1", sent.UserId);
        Assert.Equal("an@example.com", sent.Email);
        Assert.Equal("confirm-token", sent.Token);
    }

    [Fact]
    public async Task GuiEmailLoi_VanDangKyThanhCong()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = new RegisterCommandHandler(IdentityWithNewUser(), new FakeJwtService(), unitOfWork.Tokens, unitOfWork,
            new FakeAccountEmailSender { Fail = true }, NullLogger<RegisterCommandHandler>.Instance);

        var response = await handler.Handle(Command, TestContext.Current.CancellationToken);

        Assert.Equal("access-token", response.AccessToken);
        Assert.Single(unitOfWork.Tokens.Tokens);
    }
}
