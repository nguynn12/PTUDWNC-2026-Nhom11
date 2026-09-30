using CulinaryBlog.Application.Common.Behaviours;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Xunit;

namespace CulinaryBlog.UnitTests.Application.Behaviours;

public class ValidationBehaviourTests
{
    [Fact]
    public async Task ShouldThrowValidationFailedExceptionWhenValidationFails()
    {
        // Arrange
        var validator = new FakeValidator();
        var validators = new List<IValidator<TestRequest>> { validator };
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest();
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationFailedException>(() => 
            behaviour.Handle(request, delegate { return Task.FromResult(new TestResponse()); }, CancellationToken.None));

            
        Assert.True(exception.Errors.ContainsKey("property"));
    }

    [Fact]
    public async Task ShouldNotThrowWhenValidationSucceeds()
    {
        // Arrange
        var validators = new List<IValidator<TestRequest>>();
        var behaviour = new ValidationBehaviour<TestRequest, TestResponse>(validators);
        var request = new TestRequest();
        
        // Act
        var result = await behaviour.Handle(request, delegate { return Task.FromResult(new TestResponse()); }, CancellationToken.None);
        
        // Assert
        Assert.NotNull(result);
    }

    public class TestRequest { }
    public class TestResponse { }

    private class FakeValidator : AbstractValidator<TestRequest>
    {
        public override async Task<ValidationResult> ValidateAsync(ValidationContext<TestRequest> context, CancellationToken cancellation = new CancellationToken())
        {
            return new ValidationResult(new List<ValidationFailure> 
            { 
                new ValidationFailure("Property", "Error message") 
            });
        }
    }
}
