namespace CulinaryBlog.Application.Common.Interfaces;

public interface ICurrentUser
{
    string? Id { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
}
