using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiMailSender.Helpers;
using System.Net.Mail;
using MimeKit;

namespace MultiMailSender.Helpers
{
    static class GMailHelpers
    {
        public static GmailService GetGMailService(string[] scopes)
        {
            UserCredential credential;

            using (var stream =
                new FileStream(Constants._credentialFileName, FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = Constants._tokenFolderName;
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = Constants._applicationName,
            });

            return service;
        }

        public async static Task<Message> SendEmailAsync(GmailService service, Message message)
        {
            var request = service.Users.Messages.Send(message, Constants._googleConstantMe);
            var result = await request.ExecuteAsync();
            return result;
        }

        public async static Task<Message> SendEmailAsync(GmailService service, string from, string to, string subject, string body, string attachementPath, bool isHtml = true)
        {
            if (!string.IsNullOrWhiteSpace(attachementPath) && !File.Exists(attachementPath))
            {
                throw new FileNotFoundException($"File {attachementPath} not found!");
            }

            var mailMessage = new MailMessage()
            {
                From = new MailAddress(from),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(new MailAddress(to));

            if (!string.IsNullOrWhiteSpace(attachementPath))
            {
                mailMessage.Attachments.Add(new Attachment(attachementPath));
            }

            var mimeMessage = MimeMessage.CreateFromMailMessage(mailMessage);
            var encodedMessage = mimeMessage.ToString();
            var rawMessage = Base64UrlEncode(encodedMessage);
            var gmailMessage = new Message()
            {
                Raw = rawMessage
            };

            var result = await SendEmailAsync(service, gmailMessage);
            return result;
        }

        private static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
    }
}
