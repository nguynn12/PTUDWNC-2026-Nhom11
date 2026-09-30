using CulinaryBlog.Application.Auth.Commands.Register;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-001: email trùng → 409 AUTH_EMAIL_EXISTS; lỗi policy Identity → 422 theo field.</summary>
public sealed class RegisterCommandHandlerTests
{
    private static readonly RegisterCommand Command = new("an@example.com", "Passw0rd!", "Nguyễn An");

    private static RegisterCommandHandler CreateHandler(CreateUserResult result, FakeUnitOfWork unitOfWork) =>
        new(new FakeIdentityService { CreateResult = result }, new FakeJwtService(), unitOfWork);

    [Fact]
    public async Task EmailDaTonTai_Tra409AuthEmailExists()
    {
        var unitOfWork = new FakeUnitOfWork();
        var handler = CreateHandler(CreateUserResult.Duplicate(), unitOfWork);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthEmailExists, ex.ErrorCode);
        Assert.True(unitOfWork.RolledBack);
        Assert.Empty(unitOfWork.Tokens.Tokens);
    }

    [Fact]
    public async Task IdentityTuChoiMatKhau_Tra422KemLoiTheoField()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["password"] = ["Passwords must have at least one non alphanumeric character."],
        };
        var handler = CreateHandler(CreateUserResult.Failed(errors), new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.ValidationError, ex.ErrorCode);
        Assert.True(ex.Errors.ContainsKey("password"));
    }

    [Fact]
    public async Task DangKyThanhCong_LuuRefreshTokenVaCommitTransaction()
    {
        var unitOfWork = new FakeUnitOfWork();
        var identity = new FakeIdentityService
        {
            CreateResult = CreateUserResult.Success("user-1"),
            Account = new UserAccount("user-1", "an@example.com", "Nguyễn An", null, null, ["Author"], false, true, DateTimeOffset.UtcNow),
        };
        var handler = new RegisterCommandHandler(identity, new FakeJwtService(), unitOfWork);

        var response = await handler.Handle(Command, TestContext.Current.CancellationToken);

        Assert.Contains("Author", response.User.Roles);
        Assert.Single(unitOfWork.Tokens.Tokens);
        Assert.True(unitOfWork.Committed);
        Assert.False(unitOfWork.RolledBack);
    }
}
