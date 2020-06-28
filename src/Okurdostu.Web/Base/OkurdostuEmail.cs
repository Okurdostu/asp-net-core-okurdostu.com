using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using Okurdostu.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Okurdostu.Web
{
    public class OkurdostuEmail
    {
        private readonly IEmailConfiguration EmailConfigurations;

        public OkurdostuEmail(IEmailConfiguration emailConfigurations)
        {
            EmailConfigurations = emailConfigurations;
        }

        public MimeMessage NewUserMail(string FullName, string Email, Guid guid)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Halil İbrahim Kocaöz", "halil@okurdostu.com"));
            message.To.Add(new MailboxAddress(FullName, Email));
            message.Subject = "Okurdostu | Hoş geldin";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText("Views/NewUserMail.html"))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }
            builder.HtmlBody = builder.HtmlBody.Replace("{fullname}", FullName).Replace("{guid}", guid.ToString());

            message.Body = builder.ToMessageBody();
            return message;
        }

        public void SendFromHalil(MimeMessage Mail)
        {
            using (var client = new SmtpClient())
            {
                client.Connect(EmailConfigurations.Server, EmailConfigurations.Port, false);
                client.Authenticate("halil@okurdostu.com", EmailConfigurations.Password);
                client.Send(Mail);
                client.Disconnect(true);
            }
        }
    }
}
