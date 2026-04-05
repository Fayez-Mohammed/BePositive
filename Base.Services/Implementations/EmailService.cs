using Base.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Base.DAL.Models.BaseModels;

namespace Base.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        public EmailService(IConfiguration config, ILogger<EmailService> logger, IEmailSender emailSender,UserManager<ApplicationUser> userManager)
        {
            _config = config;
            _logger = logger;
            _emailSender = emailSender;
            _userManager = userManager;
        }
        public async Task SendOtpEmailAsync(string to, string otp)
        {
            var user=await _userManager.FindByEmailAsync(to);
            string userName = user.FullName;
            var minutes = 5;
            var year = DateTime.UtcNow.Year;

            var logoUrl = "https://bepositive.runasp.net/uploads/be-postive-logo-text.png";

            var htmlTemplate = @$"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Login Verification Code</title>
</head>

<body style='margin:0;padding:0;background-color:#fef2f2;font-family:Arial,sans-serif;color:#111827;'>

<div style='background-color:#fef2f2;padding:40px 0;'>

<div style='max-width:600px;margin:0 auto;background-color:#ffffff;border-radius:6px;'>

<div style='text-align:center;padding:20px;'>

<img src='" + logoUrl + @"'
     style='max-width:180px;height:auto;margin-bottom:10px;' />

<h2 style='color:#d4183d'>Login Verification</h2>

</div>

<div style='padding:20px;text-align:center;'>

<p>
Hi <b>{{UserName}}</b><br/><br/>
Use this OTP to login
</p>

<div style='font-size:32px;
font-weight:bold;
letter-spacing:8px;
color:#d4183d;
margin:20px 0;'>

{{OTP}}

</div>

<p style='color:red'>
This code will expire in {{minutes}} minutes
</p>

</div>

<div style='text-align:center;font-size:12px;color:#999;padding:15px;'>
© {{CurrentYear}} Be Positive
</div>

</div>
</div>

</body>
</html>
";

            htmlTemplate = htmlTemplate
                .Replace("{{OTP}}", otp)
                .Replace("{{UserName}}", userName)
                .Replace("{{minutes}}", minutes.ToString())
                .Replace("{{CurrentYear}}", year.ToString());

            await _emailSender.SendEmailAsync(
                to,
                "Your OTP Code",
                htmlTemplate
            );

            _logger.LogInformation($"Sending OTP {otp} to {to}");
        }
        //public async Task SendOtpEmailAsync(string to, string otp)
        //{
        //    await _emailSender.SendEmailAsync(to, "Your OTP Code",
        //            $"<p>Your OTP verification code is: <b>{otp}</b></p><p>It will expire in 5 minutes.</p>");
        //    // يمكن استخدام SMTP أو أي خدمة مثل SendGrid
        //    _logger.LogInformation($"Sending OTP {otp} to {to}");
        //    await Task.CompletedTask;
        //}
    }
}
