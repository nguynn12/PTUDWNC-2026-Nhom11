using System.Text.Json;
using CulinaryBlog.API.ErrorHandling;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.UnitTests.Api;

/// <summary>Kiểm tra ánh xạ exception → HTTP status + mã lỗi theo SRS Phụ lục A/B, RESOLVED-CONFLICTS C3/C6.</summary>
public sealed class ProblemDetailsMapperTests
{
    [Theory]
    [InlineData(AppErrorKind.Validation, 422)]
    [InlineData(AppErrorKind.BusinessRule, 422)]
    [InlineData(AppErrorKind.NotFound, 404)]
    [InlineData(AppErrorKind.Conflict, 409)]
    [InlineData(AppErrorKind.Unauthorized, 401)]
    [InlineData(AppErrorKind.Forbidden, 403)]
    public void StatusFor_TraDungHttpStatusTheoPhuLucA(AppErrorKind kind, int expectedStatus)
    {
        Assert.Equal(expectedStatus, ProblemDetailsMapper.StatusFor(kind));
    }

    [Fact]
    public void Map_NotFoundException_Tra404VaGiuNguyenMaLoiVaThongBao()
    {
        var problem = ProblemDetailsMapper.Map(
            new NotFoundException(ErrorCodes.RecipeNotFound, "Không tìm thấy công thức."));

        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("RECIPE_NOT_FOUND", problem.ErrorCode);
        Assert.Equal("Không tìm thấy công thức.", problem.Detail);
        Assert.Null(problem.Errors);
    }

    [Fact]
    public void Map_ConflictException_Tra409()
    {
        var problem = ProblemDetailsMapper.Map(
            new ConflictException(ErrorCodes.AuthEmailExists, "Email đã được đăng ký."));

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal("AUTH_EMAIL_EXISTS", problem.ErrorCode);
    }

    [Fact]
    public void Map_BusinessRuleException_Tra422()
    {
        var problem = ProblemDetailsMapper.Map(
            new BusinessRuleException(ErrorCodes.RecipePublishIncomplete, "Cần ít nhất 1 nguyên liệu và 1 bước."));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.Status);
        Assert.Equal("RECIPE_PUBLISH_INCOMPLETE", problem.ErrorCode);
    }

    [Fact]
    public void Map_ValidationFailedException_Tra422KemLoiTheoTungField()
    {
        var errors = new Dictionary<string, string[]>
        {
            ["email"] = ["Email không đúng định dạng."],
            ["password"] = ["Mật khẩu tối thiểu 8 ký tự.", "Mật khẩu cần có chữ hoa."],
        };

        var problem = ProblemDetailsMapper.Map(new ValidationFailedException(errors));

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, problem.Status);
        Assert.Equal("VALIDATION_ERROR", problem.ErrorCode);
        Assert.Equal(ValidationFailedException.DefaultMessage, problem.Detail);
        Assert.NotNull(problem.Errors);
        Assert.Equal(2, problem.Errors["password"].Length);
    }

    [Fact]
    public void Map_BadHttpRequestException_Tra400MalformedRequest()
    {
        var problem = ProblemDetailsMapper.Map(
            new BadHttpRequestException("Failed to read parameter from JSON body.", StatusCodes.Status400BadRequest));

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("MALFORMED_REQUEST", problem.ErrorCode);
    }

    [Fact]
    public void Map_BadHttpRequestExceptionVoiStatusKhac_GiuNguyenStatusCuaFramework()
    {
        var problem = ProblemDetailsMapper.Map(
            new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge));

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, problem.Status);
        Assert.Equal("MALFORMED_REQUEST", problem.ErrorCode);
    }

    [Fact]
    public void Map_JsonException_Tra400MalformedRequest()
    {
        var problem = ProblemDetailsMapper.Map(new JsonException("'}' is invalid"));

        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("MALFORMED_REQUEST", problem.ErrorCode);
    }

    [Fact]
    public void Map_DbUpdateConcurrencyException_Tra409TheoMucC6()
    {
        var problem = ProblemDetailsMapper.Map(new DbUpdateConcurrencyException("xmin mismatch"));

        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal("RECIPE_CONCURRENCY_CONFLICT", problem.ErrorCode);
    }

    [Fact]
    public void Map_ExceptionKhongXacDinh_Tra500VaKhongLoThongTinNoiBo()
    {
        const string secret = "Host=db;Password=bi-mat";

        var problem = ProblemDetailsMapper.Map(new InvalidOperationException(secret));

        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("INTERNAL_SERVER_ERROR", problem.ErrorCode);
        Assert.Equal(ProblemDetailsMapper.InternalErrorDetail, problem.Detail);
        Assert.DoesNotContain(secret, problem.Detail, StringComparison.Ordinal);
    }
}
