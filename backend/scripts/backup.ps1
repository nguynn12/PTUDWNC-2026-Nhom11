<#
.SYNOPSIS
    Sao lưu PostgreSQL thật; lỗi kết thúc với mã khác 0 (NFR-REL-003, Lab 8 TV3).
.DESCRIPTION
    Thực hiện pg_dump sao lưu toàn bộ schema và dữ liệu PostgreSQL, lưu trữ dưới định dạng nén (.dump),
    tự động dọn dẹp các bản sao lưu cũ quá 30 ngày và ghi nhật ký có cấu trúc.
    Hỗ trợ cả môi trường chạy trực tiếp pg_dump hoặc qua Docker container.
.EXAMPLE
    .\backup.ps1 -Database "culinary_db" -BackupDir "./backups" -RetentionDays 30
#>

[CmdletBinding()]
param (
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [ValidatePattern('^[a-zA-Z0-9_-]+$')]
    [string]$Database = $(if ($env:POSTGRES_DB) { $env:POSTGRES_DB } else { "culinary_blog_dev" }),
    [string]$UserName = $(if ($env:POSTGRES_USER) { $env:POSTGRES_USER } else { "culinary_blog" }),
    [string]$Password = $env:PGPASSWORD,
    [string]$DockerContainer = "culinary-blog-postgres",
    [string]$BackupDir = "$PSScriptRoot/../../backups",
    [ValidateRange(1, 3650)]
    [int]$RetentionDays = 30
)

$ErrorActionPreference = "Stop"

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss_fff"
$resolvedBackupDir = [System.IO.Path]::GetFullPath($BackupDir)

if (-not (Test-Path $resolvedBackupDir)) {
    New-Item -ItemType Directory -Path $resolvedBackupDir -Force | Out-Null
    Write-Host "[$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')] Đã tạo thư mục lưu trữ: $resolvedBackupDir" -ForegroundColor Cyan
}

$backupFileName = "backup_${Database}_${timestamp}.dump"
$backupFilePath = Join-Path $resolvedBackupDir $backupFileName

Write-Host "====================================================" -ForegroundColor Green
Write-Host "   CULINARY BLOG - POSTGRESQL DATABASE BACKUP" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host "Thời gian bắt đầu: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host "Cơ sở dữ liệu:     $Database"
Write-Host "Tệp sao lưu:       $backupFilePath"
Write-Host "----------------------------------------------------"

$partialPath = "$backupFilePath.partial"
$containerDump = "/tmp/culinary-backup-$([Guid]::NewGuid().ToString('N')).dump"
$previousPassword = $env:PGPASSWORD
$backupSuccess = $false
$pgDumpCmd = Get-Command "pg_dump" -ErrorAction SilentlyContinue
$pgRestoreCmd = Get-Command "pg_restore" -ErrorAction SilentlyContinue
$dockerCmd = Get-Command "docker" -ErrorAction SilentlyContinue

try {
    if (-not [string]::IsNullOrEmpty($Password)) { $env:PGPASSWORD = $Password }

    # Chỉ chấp nhận dump có exit code 0 và đọc được mục lục bằng pg_restore.
    if ($pgDumpCmd -and $pgRestoreCmd) {
        try {
            & $pgDumpCmd.Source -h $HostName -p $Port -U $UserName -d $Database -w -F c -b -f $partialPath
            if ($LASTEXITCODE -ne 0) { throw "pg_dump thất bại." }
            & $pgRestoreCmd.Source --list $partialPath | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "pg_restore không đọc được archive." }
            $backupSuccess = $true
        }
        catch { Write-Warning "Sao lưu cục bộ thất bại: $_" }
    }

    if (-not $backupSuccess -and $dockerCmd) {
        Write-Host "[INFO] Sao lưu qua Docker container '$DockerContainer'..."
        try {
            # Ghi file trong container rồi docker cp để giữ nguyên byte nhị phân trên cả Windows PowerShell 5.1.
            & $dockerCmd.Source exec $DockerContainer pg_dump -U $UserName -d $Database -w -F c -b -f $containerDump
            if ($LASTEXITCODE -ne 0) { throw "pg_dump trong Docker thất bại." }
            & $dockerCmd.Source exec $DockerContainer pg_restore --list $containerDump | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Archive trong Docker không hợp lệ." }
            & $dockerCmd.Source cp "${DockerContainer}:$containerDump" $partialPath
            if ($LASTEXITCODE -ne 0) { throw "Không sao chép được archive từ Docker." }
            $backupSuccess = $true
        }
        catch { Write-Warning "Sao lưu Docker thất bại: $_" }
        finally {
            & $dockerCmd.Source exec $DockerContainer rm -f -- $containerDump | Out-Null
        }
    }

    if (-not $backupSuccess -or -not (Test-Path -LiteralPath $partialPath -PathType Leaf)) {
        throw "Sao lưu thất bại. Không tạo file dump giả và không dọn các bản sao lưu cũ."
    }

    $stream = [System.IO.File]::OpenRead($partialPath)
    try {
        $header = New-Object byte[] 5
        if ($stream.Read($header, 0, 5) -ne 5 -or [System.Text.Encoding]::ASCII.GetString($header) -ne 'PGDMP') {
            throw "File sao lưu không phải PostgreSQL custom archive."
        }
    }
    finally { $stream.Dispose() }

    Move-Item -LiteralPath $partialPath -Destination $backupFilePath
    Write-Host "[SUCCESS] Sao lưu PostgreSQL đã kiểm tra: $backupFilePath"
}
finally {
    $env:PGPASSWORD = $previousPassword
    if (Test-Path -LiteralPath $partialPath) { Remove-Item -LiteralPath $partialPath -Force }
}

# 3. Dọn dẹp các bản sao lưu cũ hơn RetentionDays
Write-Host "----------------------------------------------------"
Write-Host "[INFO] Kiểm tra dọn dẹp các tệp sao lưu cũ hơn $RetentionDays ngày..." -ForegroundColor Cyan

$cutoffDate = (Get-Date).AddDays(-$RetentionDays)
$oldFiles = Get-ChildItem -LiteralPath $resolvedBackupDir -File -Filter "backup_${Database}_*.dump" | Where-Object { $_.LastWriteTime -lt $cutoffDate }

$deletedCount = 0
foreach ($file in $oldFiles) {
    if ([System.IO.Path]::GetDirectoryName($file.FullName) -ne $resolvedBackupDir.TrimEnd('\', '/')) {
        throw "Đường dẫn dọn dẹp nằm ngoài thư mục sao lưu."
    }
    Remove-Item -LiteralPath $file.FullName -Force
    Write-Host "  -> Đã xóa tệp cũ: $($file.Name) (Tạo lúc $($file.LastWriteTime))" -ForegroundColor Gray
    $deletedCount++
}

Write-Host "[INFO] Đã dọn dẹp $deletedCount tệp sao lưu cũ." -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Green

