using CulinaryBlog.Application.Common.Models;
using Xunit;

namespace CulinaryBlog.UnitTests.Application;

public sealed class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(100, 20, 5)]
    public void TotalPages_TinhDungTheoTongSoVaKichThuocTrang(int totalCount, int pageSize, int expectedPages)
    {
        var paged = new PagedResult<int>([], page: 1, pageSize, totalCount);

        Assert.Equal(expectedPages, paged.TotalPages);
    }

    [Fact]
    public void TrangDau_CoTrangSauKhongCoTrangTruoc()
    {
        var paged = new PagedResult<int>([1, 2], page: 1, pageSize: 2, totalCount: 5);

        Assert.True(paged.HasNextPage);
        Assert.False(paged.HasPreviousPage);
    }

    [Fact]
    public void TrangCuoi_KhongCoTrangSau()
    {
        var paged = new PagedResult<int>([5], page: 3, pageSize: 2, totalCount: 5);

        Assert.False(paged.HasNextPage);
        Assert.True(paged.HasPreviousPage);
    }

    [Fact]
    public void KhongCoDuLieu_KhongCoTrangTruocSau()
    {
        var paged = new PagedResult<int>([], page: 1, pageSize: 10, totalCount: 0);

        Assert.Equal(0, paged.TotalPages);
        Assert.False(paged.HasNextPage);
        Assert.False(paged.HasPreviousPage);
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 0, 0)]
    [InlineData(1, 10, -1)]
    public void ThamSoKhongHopLe_NemArgumentOutOfRangeException(int page, int pageSize, int totalCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new PagedResult<int>([], page, pageSize, totalCount));
    }
}
