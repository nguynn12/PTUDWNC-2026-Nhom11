using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CulinaryBlog.Infrastructure.Migrations
{
    /// <summary>
    /// Đổi tên các tài khoản mẫu từ miền "@culinaryblog.local" sang "@culinaryblog.com" để trông như tài khoản thật,
    /// đồng thời gán ảnh hồ sơ cho tài khoản chưa có. Chỉ đổi Email/UserName (và bản chuẩn hóa), giữ nguyên Id,
    /// mật khẩu, vai trò và mọi dữ liệu liên kết (công thức, đánh giá...) vì chúng tham chiếu theo Id.
    /// </summary>
    public partial class RenameSeedAccountsRemoveLocalDomain : Migration
    {
        private const string OldDomain = "@culinaryblog.local";
        private const string NewDomain = "@culinaryblog.com";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RenameDomain(migrationBuilder, OldDomain, NewDomain);

            // Ảnh hồ sơ mặc định (chữ cái đầu của tên) cho tài khoản chưa có ảnh; không ghi đè ảnh đã tải lên.
            SetAvatarIfMissing(migrationBuilder, "author", "Author User");
            SetAvatarIfMissing(migrationBuilder, "author.unverified", "Unverified Author");
            SetAvatarIfMissing(migrationBuilder, "chef.baongoc", "Le Bao Ngoc");
            SetAvatarIfMissing(migrationBuilder, "chef.minhkhoi", "Tran Minh Khoi");
            SetAvatarIfMissing(migrationBuilder, "chef.quocviet", "Pham Quoc Viet");
            SetAvatarIfMissing(migrationBuilder, "chef.thuha", "Nguyen Thu Ha");
            SetAvatarIfMissing(migrationBuilder, "admin", "Admin User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chỉ hoàn tác tên miền; ảnh hồ sơ đã gán được giữ lại vì không thể phân biệt với ảnh người dùng đặt.
            RenameDomain(migrationBuilder, NewDomain, OldDomain);
        }

        private static void RenameDomain(MigrationBuilder migrationBuilder, string from, string to)
        {
            // Bỏ qua tài khoản nếu email đích đã tồn tại để không vi phạm chỉ mục duy nhất.
            migrationBuilder.Sql($"""
                UPDATE "AspNetUsers" AS u
                SET "Email" = replace(u."Email", '{from}', '{to}'),
                    "UserName" = replace(u."UserName", '{from}', '{to}'),
                    "NormalizedEmail" = upper(replace(u."Email", '{from}', '{to}')),
                    "NormalizedUserName" = upper(replace(u."UserName", '{from}', '{to}'))
                WHERE lower(u."Email") LIKE '%{from}'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AspNetUsers" AS o
                      WHERE o."Id" <> u."Id"
                        AND o."NormalizedEmail" = upper(replace(u."Email", '{from}', '{to}')));
                """);
        }

        private static void SetAvatarIfMissing(MigrationBuilder migrationBuilder, string localPart, string name)
        {
            var url = "https://ui-avatars.com/api/?name=" + name.Replace(' ', '+')
                + "&size=256&background=fed8c4&color=c85a17&bold=true";
            migrationBuilder.Sql($"""
                UPDATE "AspNetUsers"
                SET "AvatarUrl" = '{url}'
                WHERE "NormalizedEmail" = upper('{localPart}{NewDomain}')
                  AND ("AvatarUrl" IS NULL OR btrim("AvatarUrl") = '');
                """);
        }
    }
}
