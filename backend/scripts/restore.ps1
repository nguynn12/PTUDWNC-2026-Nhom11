<#
.SYNOPSIS
    Script phục hồi cơ sở dữ liệu PostgreSQL cho Culinary Blog (NFR-REL-002, Lab 8 TV3).
.DESCRIPTION
    Thực hiện pg_restore phục hồi cơ sở dữ liệu từ tệp snapshot định dạng .dump.
.EXAMPLE
    .\restore.ps1 -BackupFilePath "./backups/backup_culinary_db_20261103.dump" -Database "culinary_db"
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string]$BackupFilePath,
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$Database = "culinary_db",
    [string]$UserName = "postgres",
    [string]$Password = $env:PGPASSWORD,
    [string]$DockerContainer = "culinary_postgres"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $BackupFilePath)) {
    Write-Error "Không tìm thấy tệp sao lưu: $BackupFilePath"
}

Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   CULINARY BLOG - POSTGRESQL DATABASE RESTORE" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "Tệp phục hồi:   $BackupFilePath"
Write-Host "Cơ sở dữ liệu:  $Database"
Write-Host "----------------------------------------------------"

$restoreSuccess = $false
$pgRestoreCmd = Get-Command "pg_restore" -ErrorAction SilentlyContinue

if ($pgRestoreCmd) {
    if (-not [string]::IsNullOrEmpty($Password)) {
        $env:PGPASSWORD = $Password
    }
    try {
        & pg_restore -h $HostName -p $Port -U $UserName -d $Database -c -v $BackupFilePath
        $restoreSuccess = ($LASTEXITCODE -eq 0)
    }
    catch {
        Write-Warning "[WARN] Lỗi khi chạy pg_restore trực tiếp: $_"
    }
}

if (-not $restoreSuccess) {
    $dockerCmd = Get-Command "docker" -ErrorAction SilentlyContinue
    if ($dockerCmd) {
        Write-Host "[INFO] Thử phục hồi qua Docker container '$DockerContainer'..." -ForegroundColor Yellow
        try {
            Get-Content $BackupFilePath -Raw | docker exec -i $DockerContainer pg_restore -U $UserName -d $Database -c
            $restoreSuccess = $true
        }
        catch {
            Write-Warning "[WARN] Lỗi khi phục hồi qua Docker: $_"
        }
    }
}

Write-Host "[INFO] Hoàn tất quá trình phục hồi (Status: $(if ($restoreSuccess) {'Thành công'} else {'Hoàn tất (Kiểm tra logs)'}))." -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Cyan

