<#
.SYNOPSIS
    Phục hồi PostgreSQL từ custom archive đã kiểm tra (NFR-REL-003, Lab 8 TV3).
.DESCRIPTION
    Database đích phải được chỉ định rõ và đã tồn tại. Restore dùng một transaction;
    lỗi kết thúc với mã khác 0. Không đưa dữ liệu nhị phân qua pipeline văn bản.
.EXAMPLE
    .\restore.ps1 -BackupFilePath "C:/backups/backup.dump" -Database "culinary_restore_check"
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)][string]$BackupFilePath,
    [Parameter(Mandatory = $true)][ValidatePattern('^[a-zA-Z0-9_-]+$')][string]$Database,
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$UserName = $(if ($env:POSTGRES_USER) { $env:POSTGRES_USER } else { "culinary_blog" }),
    [string]$Password = $env:PGPASSWORD,
    [string]$DockerContainer = "culinary-blog-postgres"
)
$ErrorActionPreference = "Stop"
$resolvedArchive = (Resolve-Path -LiteralPath $BackupFilePath).Path
$stream = [System.IO.File]::OpenRead($resolvedArchive)
try {
    $header = New-Object byte[] 5
    if ($stream.Read($header, 0, 5) -ne 5 -or [System.Text.Encoding]::ASCII.GetString($header) -ne 'PGDMP') {
        throw "File không phải PostgreSQL custom archive. Chưa thay đổi database."
    }
}
finally { $stream.Dispose() }
$previousPassword = $env:PGPASSWORD
$restoreSuccess = $false
$pgRestoreCmd = Get-Command "pg_restore" -ErrorAction SilentlyContinue
$dockerCmd = Get-Command "docker" -ErrorAction SilentlyContinue
$containerDump = "/tmp/culinary-restore-$([Guid]::NewGuid().ToString('N')).dump"
try {
    if (-not [string]::IsNullOrEmpty($Password)) { $env:PGPASSWORD = $Password }
    if ($pgRestoreCmd) {
        try {
            & $pgRestoreCmd.Source --list $resolvedArchive | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Không đọc được archive." }
            & $pgRestoreCmd.Source -h $HostName -p $Port -U $UserName -d $Database -w --clean --if-exists --single-transaction --exit-on-error --no-owner --no-privileges $resolvedArchive
            if ($LASTEXITCODE -ne 0) { throw "pg_restore thất bại; transaction đã rollback." }
            $restoreSuccess = $true
        }
        catch { Write-Warning "Phục hồi cục bộ thất bại: $_" }
    }
    if (-not $restoreSuccess -and $dockerCmd) {
        try {
            & $dockerCmd.Source cp $resolvedArchive "${DockerContainer}:$containerDump"
            if ($LASTEXITCODE -ne 0) { throw "Không sao chép được archive vào Docker." }
            & $dockerCmd.Source exec $DockerContainer pg_restore --list $containerDump | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "Không đọc được archive trong Docker." }
            & $dockerCmd.Source exec $DockerContainer pg_restore -U $UserName -d $Database -w --clean --if-exists --single-transaction --exit-on-error --no-owner --no-privileges $containerDump
            if ($LASTEXITCODE -ne 0) { throw "pg_restore trong Docker thất bại; transaction đã rollback." }
            $restoreSuccess = $true
        }
        catch { Write-Warning "Phục hồi Docker thất bại: $_" }
        finally { & $dockerCmd.Source exec $DockerContainer rm -f -- $containerDump | Out-Null }
    }
    if (-not $restoreSuccess) { throw "Phục hồi thất bại. Không báo thành công khi pg_restore lỗi." }
    Write-Host "[SUCCESS] Đã phục hồi archive vào database '$Database'."
}
finally { $env:PGPASSWORD = $previousPassword }
