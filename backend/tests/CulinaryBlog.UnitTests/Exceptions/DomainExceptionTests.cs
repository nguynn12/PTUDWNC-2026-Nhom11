namespace CulinaryBlog.UnitTests.Exceptions;

using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Exceptions;
using Xunit;

/// <summary>
/// Unit Tests kiểm tra tính kế thừa và toàn vẹn của các Domain Exceptions.
/// Tuân thủ yêu cầu chung Lab 3: Hoàn thành Domain Exceptions trong Domain Layer.
/// </summary>
public class DomainExceptionTests
{
    [Fact]
    public void EntityNotFoundException_ShouldInheritFromDomainException_AndFormatMessage()
    {
        // Arrange
        var key = Guid.NewGuid();

        // Act
        var ex = new EntityNotFoundException("Recipe", key);

        // Assert
        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Contains("Recipe", ex.Message);
        Assert.Contains(key.ToString(), ex.Message);
    }

    [Fact]
    public void BusinessRuleValidationException_ShouldStorePropertyName_AndInheritFromDomainException()
    {
        // Act
        var ex = new CulinaryBlog.Domain.Exceptions.BusinessRuleValidationException("Quantity", "Định lượng phải lớn hơn 0.");

        // Assert
        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Equal("Quantity", ex.PropertyName);
        Assert.Equal("Định lượng phải lớn hơn 0.", ex.Message);
    }

    [Fact]
    public void ConcurrencyConflictException_ShouldProvideDefaultMessage()
    {
        // Act
        var ex = new ConcurrencyConflictException();

        // Assert
        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Contains("Dữ liệu đã bị thay đổi", ex.Message);
    }

    [Fact]
    public void ForbiddenDomainException_ShouldProvideDefaultMessage()
    {
        // Act
        var ex = new ForbiddenDomainException();

        // Assert
        Assert.IsAssignableFrom<DomainException>(ex);
        Assert.Contains("không có quyền", ex.Message);
    }

    [Fact]
    public void ApplicationExceptions_ShouldInheritFromCorrespondingDomainExceptions()
    {
        // Assert: Đảm bảo cầu nối nhất quán giữa Domain và Application
        Assert.IsAssignableFrom<EntityNotFoundException>(new NotFoundException("Item", Guid.NewGuid()));
        Assert.IsAssignableFrom<CulinaryBlog.Domain.Exceptions.BusinessRuleValidationException>(new ValidationException("Field", "Error"));
        Assert.IsAssignableFrom<ForbiddenDomainException>(new ForbiddenException());
        Assert.IsAssignableFrom<ConcurrencyConflictException>(new ConflictException("Conflict"));
    }
}
