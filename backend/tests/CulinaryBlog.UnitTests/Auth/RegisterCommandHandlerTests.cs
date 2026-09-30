using CulinaryBlog.Application.Auth.Commands.Register;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

/// <summary>SRS FR-AUTH-001: email trùng → 409 AUTH_EMAIL_EXISTS; lỗi policy Identity → 422 theo field.</summary>
public sealed class RegisterCommandHandlerTests
{
    private static readonly RegisterCommand Command = new("an@example.com", "Passw0rd!", "Nguyễn An");

    private static RegisterCommandHandler CreateHandler(CreateUserResult result) =>
        new(new FakeIdentityService { CreateResult = result }, new FakeJwtService(), null!);

    [Fact]
    public async Task EmailDaTonTai_Tra409AuthEmailExists()
    {
        var handler = CreateHandler(CreateUserResult.Duplicate());

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.AuthEmailExists, ex.ErrorCode);
    }

    [Fact]
    public async Task IdentityTuChoiMatKhau_Tra422KemLoiTheoField()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["password"] = ["Passwords must have at least one non alphanumeric character."],
        };
        var handler = CreateHandler(CreateUserResult.Failed(errors));

        var ex = await Assert.ThrowsAsync<ValidationFailedException>(
            () => handler.Handle(Command, TestContext.Current.CancellationToken));

        Assert.Equal(ErrorCodes.ValidationError, ex.ErrorCode);
        Assert.True(ex.Errors.ContainsKey("password"));
    }
}
