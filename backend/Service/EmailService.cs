using Resend;

namespace backend.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;

        public ResendEmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
        {
            var fromEmail = _configuration["Resend:FromEmail"]!;
            var fromName = _configuration["Resend:FromName"] ?? "Designare";

            var message = new EmailMessage
            {
                From = $"{fromName} <{fromEmail}>",
                To = { toEmail },
                Subject = "密碼重設",
                HtmlBody = $"""
                    <html>
                    <body style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; line-height: 1.6; color: #1a202c;">
                        <div style="max-width: 580px; margin: 0 auto; padding: 20px;">
                            <p style="margin: 0 0 20px 0;">你好，</p>
                            <p style="margin: 0 0 24px 0;">
                                我們收到你的密碼重設申請。點擊下方按鈕來完成重設。這個連結在 15 分鐘內有效。
                            </p>
                            <div style="text-align: center; margin: 35px 0;">
                                <a href="{resetUrl}" style="background: #1e293b; color: white; padding: 11px 28px; text-decoration: none; border-radius: 5px; font-weight: 500; display: inline-block; font-size: 14px;">立即重設</a>
                            </div>
                            <p style="color: #64748b; font-size: 13px; margin: 20px 0;">
                                無法點擊按鈕？將以下連結複製到瀏覽器：<br>
                                <code style="background: #f8fafc; padding: 6px 10px; border-radius: 3px; display: block; margin-top: 6px; font-size: 12px; overflow-wrap: break-word;">{resetUrl}</code>
                            </p>
                            <p style="color: #64748b; font-size: 13px; margin-top: 28px; padding-top: 20px; border-top: 1px solid #e2e8f0;">
                                <strong style="color: #1a202c;">沒有提出重設申請嗎？</strong>你可以安全地忽略這封信。
                            </p>
                            <p style="color: #94a3b8; font-size: 12px; margin: 12px 0 0 0;">
                                系統自動寄送，恕不回覆。
                            </p>
                        </div>
                    </body>
                    </html>
                """
            };

            await _resend.EmailSendAsync(message);
        }
    }
}
