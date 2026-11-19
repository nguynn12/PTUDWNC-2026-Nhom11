using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.UnitTests.Support;

internal static class HandlerTestSupport
{
    internal static CulinaryBlogDbContext CreateContext() => new(
        new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    internal sealed record User(string? UserId, bool IsAdmin = false) : ICurrentUserService;

    internal sealed class Authors(params AuthorSummary[] authors) : IUserQueryService
    {
        public List<string[]> Lookups { get; } = [];

        public Task<IReadOnlyDictionary<string, AuthorSummary>> GetAuthorSummariesAsync(
            IEnumerable<string> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.ToArray();
            Lookups.Add(ids);
            var matches = authors.Where(author => ids.Contains(author.Id)).ToDictionary(author => author.Id);
            return Task.FromResult<IReadOnlyDictionary<string, AuthorSummary>>(matches);
        }
    }

    internal sealed class Cache : ICacheService
    {
        private readonly Dictionary<string, object?> _entries = new();
        public List<(string Key, TimeSpan? Expiration)> Writes { get; } = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) =>
            Task.FromResult(_entries.TryGetValue(key, out var value) && value is T match ? match : default);

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            _entries[key] = value;
            Writes.Add((key, expiration));
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _entries.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            foreach (var key in _entries.Keys.Where(key => key.StartsWith(prefix, StringComparison.Ordinal)).ToArray())
                _entries.Remove(key);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            if (_entries.TryGetValue(key, out var value) && value is T match)
                return match;

            var result = await factory();
            await SetAsync(key, result, expiration, cancellationToken);
            return result;
        }
    }
}
