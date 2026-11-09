# Cấu hình email SMTP

Mặc định môi trường Development dùng `MockEmailService` và ghi nội dung email vào log.
Khi đủ `Email:Host`, `Email:Port`, `Email:User`, `Email:Password` và `Email:From`,
API tự động dùng `SmtpEmailService` (MailKit).

Không ghi mật khẩu SMTP vào `appsettings*.json` hoặc commit vào Git. Cấu hình local bằng
User Secrets từ thư mục `backend/src/CulinaryBlog.API`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Email:Host" "smtp.example.com"
dotnet user-secrets set "Email:Port" "587"
dotnet user-secrets set "Email:User" "account@example.com"
dotnet user-secrets set "Email:Password" "<smtp-password>"
dotnet user-secrets set "Email:From" "Culinary Blog <account@example.com>"
```

Email reset dùng `/auth/reset-password?email=...&token=...`; email xác nhận dùng
`/auth/confirm-email?userId=...&token=...`, đều dựa trên `ClientApp:BaseUrl`.
