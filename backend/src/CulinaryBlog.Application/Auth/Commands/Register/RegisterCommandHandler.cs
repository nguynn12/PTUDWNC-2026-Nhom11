using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Register;

/// <summary>
/// SRS FR-AUTH-001: tạo tài khoản role Author và trả cặp token (endpoint trả 201).
/// Email trùng → 409 <c>AUTH_EMAIL_EXISTS</c>; lỗi policy mật khẩu của Identity → 422
/// <c>VALIDATION_ERROR</c> (ném FluentValidation.ValidationException giống ValidationBehavior).
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public const string EmailExistsMessage = "Email đã được đăng ký.";

    private const int RefreshTokenExpiryDays = 7;
    private const string ClientIp = "127.0.0.1"; // TODO: lấy IP thật từ HttpContext

    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.CreateUserAsync(
            request.Email.Trim(),
            request.Password,
            request.DisplayName.Trim());

        if (result.DuplicateEmail)
        {
            throw new ConflictException(EmailExistsMessage, ErrorCodes.AuthEmailExists);
        }

        if (!result.Succeeded)
        {
            throw new ValidationException(result.Errors.SelectMany(
                pair => pair.Value.Select(message => new ValidationFailure(pair.Key, message))));
        }

        var user = await _identityService.GetUserDetailsByIdAsync(result.UserId)
            ?? throw new InvalidOperationException("Không đọc được tài khoản vừa tạo.");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);
        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

        _refreshTokens.Add(
            CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(user.Id, tokenHash, RefreshTokenExpiryDays, ClientIp));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }
}
