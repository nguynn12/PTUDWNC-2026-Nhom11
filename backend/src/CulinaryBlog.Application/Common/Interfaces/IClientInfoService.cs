namespace CulinaryBlog.Application.Common.Interfaces;

/// <summary>Thông tin client của request hiện tại (IP) — dùng để ghi <c>CreatedByIp</c> của refresh token.</summary>
public interface IClientInfoService
{
    /// <summary>Địa chỉ IP của client (IPv4 hoặc IPv6, tối đa 45 ký tự); "unknown" nếu không xác định được.</summary>
    string IpAddress { get; }
}
