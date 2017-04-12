﻿using System.Threading.Tasks;
using Abp.Net.Mail;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

#if NET46
using System.Net.Mail;
#endif

namespace Abp.MailKit
{
    public class MailKitEmailSender : EmailSenderBase
    {
        private readonly IAbpMailKitConfiguration _mailKitConfiguration;

        public MailKitEmailSender(IEmailSenderConfiguration smtpEmailSenderConfiguration, IAbpMailKitConfiguration mailKitConfiguration)
            : base(smtpEmailSenderConfiguration)
        {
            _mailKitConfiguration = mailKitConfiguration;
        }

        public override async Task SendAsync(string from, string to, string subject, string body, bool isBodyHtml = true)
        {
            using (var client = BuildSmtpClient())
            {
                var message = BuildMimeMessage(from, to, subject, body, isBodyHtml);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        public override void Send(string from, string to, string subject, string body, bool isBodyHtml = true)
        {
            using (var client = BuildSmtpClient())
            {
                var message = BuildMimeMessage(from, to, subject, body, isBodyHtml);
                client.Send(message);
                client.Disconnect(true);
            }
        }

#if NET46
        protected override async Task SendEmailAsync(MailMessage mail)
        {
            using (var client = BuildSmtpClient())
            {
                var message = mail.ToMimeMessage();
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }

        protected override void SendEmail(MailMessage mail)
        {
            using (var client = BuildSmtpClient())
            {
                var message = mail.ToMimeMessage();
                client.Send(message);
                client.Disconnect(true);
            }
        }
#endif

        protected virtual SmtpClient BuildSmtpClient()
        {
            var client = new SmtpClient();
            try
            {
                _mailKitConfiguration.SmtpClientConfigurer?.Invoke(client);
                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        private static MimeMessage BuildMimeMessage(string from, string to, string subject, string body, bool isBodyHtml = true)
        {
            var bodyType = isBodyHtml ? "html" : "plain";
            var message = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart(bodyType)
                {
                    Text = body
                }
            };

            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(to));
            
            return message;
        }
    }
}