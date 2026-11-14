using System.Net;
using System.Net.Http.Headers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>MEDIA-01 / SEC-05: POST /auth/me/avatar nhận multipart (không bị antiforgery chặn) và kiểm tra ảnh thật.</summary>
public sealed class AvatarUploadEndpointTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private const string Url = "/api/v1/auth/me/avatar";

    [Fact]
    public async Task UploadAvatar_AnhPngHopLe_Tra200KhongBiAntiforgeryChan()
    {
        using var client = await CreateClientWithFakeStorageAsync();

        var response = await client.PostAsync(Url, BuildForm(RealPng(), "avatar.png", "image/png"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UploadAvatar_NoiDungGiaAnh_Tra400()
    {
        using var client = await CreateClientWithFakeStorageAsync();
        var fake = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5 };

        var response = await client.PostAsync(Url, BuildForm(fake, "avatar.png", "image/png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadAvatar_FileAvif_Tra400()
    {
        using var client = await CreateClientWithFakeStorageAsync();

        var response = await client.PostAsync(Url, BuildForm(RealPng(), "avatar.avif", "image/avif"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UploadAvatar_KhongCoToken_Tra401()
    {
        using var client = factory.CreateGuestClient();

        var response = await client.PostAsync(Url, BuildForm(RealPng(), "avatar.png", "image/png"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<HttpClient> CreateClientWithFakeStorageAsync()
    {
        // WithWebHostBuilder dựng host mới với InMemory DB riêng => tạo user ngay trong host này.
        var host = factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            foreach (var d in services.Where(d => d.ServiceType == typeof(IFileStorageService)).ToList())
                services.Remove(d);
            services.AddSingleton<IFileStorageService, FakeFileStorage>();
        }));

        using (var scope = host.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext>();
            await db.Database.EnsureCreatedAsync();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = ApplicationUser.Create("author@culinary.local", "Author Test");
            user.Id = CustomWebApplicationFactory.AuthorUserId;
            user.EmailConfirmed = true;
            var result = await userManager.CreateAsync(user, "OldPassword1!");
            Assert.True(result.Succeeded, string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        var client = host.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-Role", CulinaryBlog.Domain.Constants.Roles.Author);
        return client;
    }

    private static MultipartFormDataContent BuildForm(byte[] bytes, string fileName, string contentType)
    {
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return new MultipartFormDataContent { { content, "file", fileName } };
    }

    private static byte[] RealPng()
    {
        using var image = new Image<Rgba32>(8, 8);
        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }

    private sealed class FakeFileStorage : IFileStorageService
    {
        public Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken cancellationToken = default)
            => Task.FromResult($"http://fake/{folder}/{Guid.NewGuid()}.png");

        public Task DeleteAsync(string fileUrl, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
