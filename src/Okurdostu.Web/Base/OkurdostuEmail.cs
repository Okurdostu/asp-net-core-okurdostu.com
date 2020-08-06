using MailKit.Net.Smtp;
using MimeKit;
using Okurdostu.Web.Services;
using System;
using System.IO;

namespace Okurdostu.Web
{
    public class OkurdostuEmail
    {
        public string SenderMail { get; set; }

        public string SenderName { get; set; }

        private readonly IEmailConfigurationService EmailConfigurations;

        public OkurdostuEmail(IEmailConfigurationService emailConfigurations)
        {
            EmailConfigurations = emailConfigurations;
        }

        public MimeMessage NewUserMail(string FullName, string Email, Guid guid)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.SenderName, this.SenderMail));
            message.To.Add(new MailboxAddress(FullName, Email));
            message.Subject = "Okurdostu | Hoş geldin";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText("Mailtemplates/NewUserMail.html")) //mail içeriği değişecek
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            builder.HtmlBody = builder.HtmlBody.Replace("{fullname}", FullName).Replace("{guid}", guid.ToString());

            message.Body = builder.ToMessageBody();
            return message;
        }

        public MimeMessage PasswordResetMail(string FullName, string Email, Guid guid)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.SenderName, this.SenderMail));
            message.To.Add(new MailboxAddress(FullName, Email));
            message.Subject = "Okurdostu | Şifre sıfırlama isteği";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText("Mailtemplates/PasswordResetMail.html"))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            builder.HtmlBody = builder.HtmlBody.Replace("{fullname}", FullName).Replace("{guid}", guid.ToString());

            message.Body = builder.ToMessageBody();
            return message;
        }


        public MimeMessage EmailAddressChangeMail(string FullName, string Email, Guid guid)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.SenderName, this.SenderMail));
            message.To.Add(new MailboxAddress(FullName, Email));
            message.Subject = "Okurdostu | Yeni e-mail adresi isteği";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText("Mailtemplates/ChangeEmailAdressMail.html"))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            builder.HtmlBody = builder.HtmlBody.Replace("{fullname}", FullName).Replace("{guid}", guid.ToString());

            message.Body = builder.ToMessageBody();
            return message;
        }


        public void Send(MimeMessage Mail)
        {
            using var client = new SmtpClient();
            client.Connect(EmailConfigurations.Server, EmailConfigurations.Port, MailKit.Security.SecureSocketOptions.None);
            client.Authenticate(SenderMail, EmailConfigurations.Password);
            client.Send(Mail);
            client.Disconnect(true);
        }
    }
}
