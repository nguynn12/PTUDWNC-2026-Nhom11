using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Repositories;
using MediatR;

namespace CulinaryBlog.Application.Auth.Commands.Register;

/// <summary>
/// SRS FR-AUTH-001: tạo tài khoản role Author và trả cặp token (endpoint trả 201).
/// Email trùng → 409 <c>AUTH_EMAIL_EXISTS</c>; lỗi policy mật khẩu của Identity → 422.
/// Tạo user + lưu refresh token nằm trong CÙNG một transaction của Unit of Work.
/// </summary>
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    public const string EmailExistsMessage = "Email đã được đăng ký.";

    private const int RefreshTokenExpiryDays = 7;
    private const string ClientIp = "127.0.0.1"; // TODO: lấy IP thật qua ICurrentUser

    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IIdentityService identityService, IJwtService jwtService, IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _identityService.CreateUserAsync(
                request.Email.Trim(),
                request.Password,
                request.DisplayName.Trim());

            if (result.DuplicateEmail)
            {
                throw new ConflictException(ErrorCodes.AuthEmailExists, EmailExistsMessage);
            }

            if (!result.Succeeded)
            {
                throw new ValidationFailedException(result.Errors);
            }

            var user = await _identityService.GetUserDetailsByIdAsync(result.UserId)
                ?? throw new InvalidOperationException("Không đọc được tài khoản vừa tạo.");

            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Roles, user.EmailConfirmed);
            var (tokenHash, rawToken) = _jwtService.GenerateRefreshToken();

            await _unitOfWork.RefreshTokens.AddAsync(
                CulinaryBlog.Domain.Entities.RefreshToken.CreateNewFamily(user.Id, tokenHash, RefreshTokenExpiryDays, ClientIp),
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawToken,
                ExpiresIn = _jwtService.AccessTokenLifetimeSeconds,
                User = user.ToUserDto(),
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
