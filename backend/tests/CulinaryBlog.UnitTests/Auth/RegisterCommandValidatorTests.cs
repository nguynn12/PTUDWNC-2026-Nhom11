using CulinaryBlog.Application.Auth.Commands.Register;
using Xunit;

namespace CulinaryBlog.UnitTests.Auth;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Theory]
    [InlineData("demo@culinary.test", "Culinary8!", "Nguyễn An")]
    [InlineData("demo@example.test", "Abcdef1!", "An")]
    public void Registration_AcceptsValidEmailAndPasswordAtOrAboveMinimumLength(
        string email, string password, string displayName)
    {
        var result = _validator.Validate(new RegisterCommand(email, password, displayName));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Abcde1!")]
    [InlineData("culinary8!")]
    [InlineData("CULINARY8!")]
    [InlineData("Culinary!!")]
    [InlineData("Culinary88")]
    public void Registration_RejectsPasswordMissingAnyRequiredComponent(string password)
    {
        var result = _validator.Validate(new RegisterCommand("demo@example.test", password, "Nguyễn An"));

        Assert.Equal(nameof(RegisterCommand.Password), Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Registration_ReportsMalformedEmailAndDisplayNameTooLong()
    {
        var result = _validator.Validate(new RegisterCommand("not-an-email", "Culinary8!", new string('a', 101)));

        Assert.Equal(new[] { nameof(RegisterCommand.Email), nameof(RegisterCommand.DisplayName) },
            result.Errors.Select(error => error.PropertyName));
    }
}
