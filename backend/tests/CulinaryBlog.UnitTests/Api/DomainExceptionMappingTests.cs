using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Domain exception phải được GlobalExceptionHandler trả về đúng HTTP status + mã lỗi.</summary>
public sealed class DomainExceptionMappingTests
{
    [Fact]
    public void BusinessRuleViolation_Tra422()
    {
        var problem = ProblemDetailsMapper.Map(
            new BusinessRuleViolationException("RECIPE_PUBLISH_INCOMPLETE", "Cần ít nhất 1 nguyên liệu và 1 bước."));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.Status);
        Assert.Equal("RECIPE_PUBLISH_INCOMPLETE", problem.ErrorCode);
    }

    [Fact]
    public void EntityNotFound_Tra404()
    {
        var problem = ProblemDetailsMapper.Map(new EntityNotFoundException("RECIPE_NOT_FOUND", "Không tìm thấy công thức."));

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public void DomainConflict_Tra409()
    {
        var problem = ProblemDetailsMapper.Map(new DomainConflictException("RECIPE_NOT_DELETED", "Công thức chưa bị xoá."));

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
    }

    [Fact]
    public void RefreshTokenRevoked_Tra401()
    {
        var problem = ProblemDetailsMapper.Map(new RefreshTokenRevokedException());

        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal(RefreshTokenRevokedException.Code, problem.ErrorCode);
    }

    [Fact]
    public void RefreshTokenExpired_Tra401()
    {
        var problem = ProblemDetailsMapper.Map(new RefreshTokenExpiredException());

        Assert.Equal(StatusCodes.Status401Unauthorized, problem.Status);
        Assert.Equal(RefreshTokenExpiredException.Code, problem.ErrorCode);
    }
}
