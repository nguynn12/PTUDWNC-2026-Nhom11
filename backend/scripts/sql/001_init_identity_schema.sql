-- ============================================================================
-- Culinary Blog — Module Xác thực / Người dùng / Phân quyền
-- Query tạo bảng database (PostgreSQL 16)
-- Khớp 1:1 với Domain Entities + EF Core Fluent API Configuration trong repo:
--   backend/src/CulinaryBlog.Domain/Entities/ApplicationUser.cs, RefreshToken.cs
--   backend/src/CulinaryBlog.Infrastructure/Persistence/Configurations/*.cs
-- Tham khảo thiết kế đầy đủ: docs/database/Thiet_Ke_CSDL_Nguoi_Dung_Xac_Thuc.md
--
-- CÁCH CHẠY:
--   docker compose up -d postgres
--   docker compose exec -T postgres psql -U culinary_blog -d culinary_blog_dev \
--       < backend/scripts/sql/001_init_identity_schema.sql
-- (hoặc mở file này trong pgAdmin → Query Tool → Execute, sau khi kết nối server
--  "Culinary Blog PostgreSQL" đã có sẵn trong infrastructure/pgadmin/servers.json)
--
-- LƯU Ý: đây là cách bootstrap thủ công để chạy được ngay. Khi đã cài được công cụ
-- `dotnet-ef` (dotnet tool install --global dotnet-ef), nên chuyển hẳn sang EF Core
-- Migrations làm nguồn chân lý duy nhất cho schema (đặc biệt từ khi thêm bảng Recipe/
-- Category về sau) — xem mục 7 trong tài liệu thiết kế.
-- ============================================================================

BEGIN;

-- ── AspNetRoles ──────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "AspNetRoles" (
    "Id"               varchar(450)  NOT NULL,
    "Name"             varchar(256)  NULL,
    "NormalizedName"   varchar(256)  NULL,
    "ConcurrencyStamp" text          NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex"
    ON "AspNetRoles" ("NormalizedName");

-- ── AspNetUsers (ApplicationUser) ─────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "AspNetUsers" (
    "Id"                   varchar(450)  NOT NULL,
    "UserName"             varchar(256)  NULL,
    "NormalizedUserName"   varchar(256)  NULL,
    "Email"                varchar(256)  NULL,
    "NormalizedEmail"      varchar(256)  NULL,
    "EmailConfirmed"       boolean       NOT NULL DEFAULT FALSE,
    "PasswordHash"         text          NULL,
    "SecurityStamp"        text          NULL,
    "ConcurrencyStamp"     text          NULL,
    "PhoneNumber"          text          NULL,
    "PhoneNumberConfirmed" boolean       NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled"     boolean       NOT NULL DEFAULT FALSE,
    "LockoutEnd"           timestamptz   NULL,
    "LockoutEnabled"       boolean       NOT NULL DEFAULT TRUE,
    "AccessFailedCount"    integer       NOT NULL DEFAULT 0,
    -- ── Field mở rộng theo SRS.md mục 7.7 ──────────────────────────────────
    "DisplayName"          varchar(100)  NOT NULL,
    "AvatarUrl"            varchar(500)  NULL,
    "Bio"                  text          NULL,
    "IsActive"             boolean       NOT NULL DEFAULT TRUE,
    "CreatedAt"            timestamptz   NOT NULL DEFAULT now(),
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex"
    ON "AspNetUsers" ("NormalizedUserName");
CREATE INDEX IF NOT EXISTS "EmailIndex"
    ON "AspNetUsers" ("NormalizedEmail");
CREATE INDEX IF NOT EXISTS "IX_AspNetUsers_IsActive"
    ON "AspNetUsers" ("IsActive");

-- ── AspNetRoleClaims ───────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "AspNetRoleClaims" (
    "Id"         serial        NOT NULL,
    "RoleId"     varchar(450)  NOT NULL,
    "ClaimType"  text          NULL,
    "ClaimValue" text          NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId"
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

-- ── AspNetUserClaims ───────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "AspNetUserClaims" (
    "Id"         serial        NOT NULL,
    "UserId"     varchar(450)  NOT NULL,
    "ClaimType"  text          NULL,
    "ClaimValue" text          NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

-- ── AspNetUserLogins (Google OAuth external login — FR-AUTH-003) ──────────────
CREATE TABLE IF NOT EXISTS "AspNetUserLogins" (
    "LoginProvider"       varchar(128)  NOT NULL,
    "ProviderKey"         varchar(128)  NOT NULL,
    "ProviderDisplayName" text          NULL,
    "UserId"              varchar(450)  NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

-- ── AspNetUserRoles ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS "AspNetUserRoles" (
    "UserId" varchar(450) NOT NULL,
    "RoleId" varchar(450) NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId"
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

-- ── AspNetUserTokens (token xác nhận email/reset mật khẩu — FR-AUTH-008/009) ──
CREATE TABLE IF NOT EXISTS "AspNetUserTokens" (
    "UserId"        varchar(450) NOT NULL,
    "LoginProvider" varchar(128) NOT NULL,
    "Name"          varchar(128) NOT NULL,
    "Value"         text         NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

-- ── RefreshTokens (bảng tuỳ biến — SRS.md mục 7.8) ────────────────────────────
CREATE TABLE IF NOT EXISTS "RefreshTokens" (
    "Id"                  uuid          NOT NULL,
    "UserId"              varchar(450)  NOT NULL,
    "FamilyId"            uuid          NOT NULL,
    "TokenHash"           char(64)      NOT NULL,
    "ExpiresAt"           timestamptz   NOT NULL,
    "RevokedAt"           timestamptz   NULL,
    "ReplacedByTokenHash" char(64)      NULL,
    "RevocationReason"    varchar(100)  NULL,
    "CreatedAt"           timestamptz   NOT NULL,
    "CreatedByIp"         varchar(45)   NULL,
    "RevokedByIp"         varchar(45)   NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_AspNetUsers_UserId"
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_FamilyId"   ON "RefreshTokens" ("FamilyId");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId"     ON "RefreshTokens" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId_RevokedAt"
    ON "RefreshTokens" ("UserId", "RevokedAt");

-- ── Seed 2 role cố định (khớp RoleConfiguration.cs — GUID KHÔNG được đổi) ─────
INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
    ('20000000-0000-0000-0000-000000000001', 'Admin',  'ADMIN',  'b9f6e7d1-0000-0000-0000-000000000001'),
    ('20000000-0000-0000-0000-000000000002', 'Author', 'AUTHOR', 'b9f6e7d1-0000-0000-0000-000000000002')
ON CONFLICT ("Id") DO NOTHING;

COMMIT;
