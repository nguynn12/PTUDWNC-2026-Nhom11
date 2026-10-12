using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Auth.Commands.Register;

/// <summary>
/// SRS FR-AUTH-001: tạo tài khoản role Author và trả cặp token (endpoint trả 201).
/// Email trùng → 409 <c>AUTH_EMAIL_EXISTS</c>; lỗi policy mật khẩu của Identity → 422
/// <c>VALIDATION_ERROR</c> (ném FluentValidation.ValidationException giống ValidationBehavior).
/// Sau khi lưu tài khoản sẽ gửi email xác nhận; lỗi gửi email chỉ ghi log, KHÔNG làm hỏng việc
/// đăng ký (người dùng có thể yêu cầu gửi lại qua FR-AUTH-009).
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public const string EmailExistsMessage = "Email đã được đăng ký.";

    private const int RefreshTokenExpiryDays = 7;

    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClientInfoService _clientInfo;
    private readonly IAccountEmailSender _accountEmailSender;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork unitOfWork,
        IClientInfoService clientInfo,
        IAccountEmailSender accountEmailSender,
        ILogger<RegisterCommandHandler> logger)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _unitOfWork = unitOfWork;
        _clientInfo = clientInfo;
        _accountEmailSender = accountEmailSender;
        _logger = logger;
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
            throw new FluentValidation.ValidationException(result.Errors.SelectMany(
                pair => pair.Value.Select(message => new ValidationFailure(pair.Key, message))));
        }

        var user = await _identityService.GetUserDetailsByIdAsync(result.UserId)
            ?? throw new InvalidOperationException("Không đọc được tài khoản vừa tạo.");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);
        var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

        _refreshTokens.Add(
            CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(user.Id, tokenHash, RefreshTokenExpiryDays, _clientInfo.IpAddress));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendConfirmationEmailAsync(user.Id, user.Email, user.DisplayName, cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawToken,
            ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
            User = user.ToUserDto(),
        };
    }

    private async Task SendConfirmationEmailAsync(string userId, string email, string displayName, CancellationToken cancellationToken)
    {
        try
        {
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
            if (token is not null)
            {
                await _accountEmailSender.SendEmailConfirmationAsync(userId, email, displayName, token, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Không gửi được email xác nhận cho user {UserId}", userId);
        }
    }
}
