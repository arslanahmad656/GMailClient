using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Mail;
using MimeKit;

namespace Practice
{
    static class GMailHelper
    {
        public static GmailService GetGMailService(string[] scopes, string applicationName)
        {
            UserCredential credential;

            using (var stream =
                new FileStream(@"Data\credentials2.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Gmail API service.
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            return service;
        }

        public static List<string> GetLabels(GmailService service)
        {
            UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");
            IList<Label> labels = request.Execute().Labels;
            return labels.Select(l => l.Name).ToList();
        }

        public static List<Draft> GetDrafts(GmailService service)
        {
            UsersResource.DraftsResource.ListRequest request = service.Users.Drafts.List("me");
            var drafts = request.Execute();
            return drafts.Drafts.ToList();
        }

        public static Draft GetDraft(GmailService service, string draftId)
        {
            UsersResource.DraftsResource.GetRequest request = service.Users.Drafts.Get("me", draftId);
            var draft = request.Execute();
            return draft;
        }

        public static Message SendEmail(GmailService service)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("apitesting656@gmail.com"),
                Subject = "Testing message",
                Body = "This is <b>Body</b> of the message. This message was sent from .NET as <em>testing</em>.",
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress("ars.ahm6@gmail.com"));
            mailMessage.Attachments.Add(new Attachment("sample.txt"));

            var mimeMsg = MimeMessage.CreateFromMailMessage(mailMessage);
            var encodedMsg = mimeMsg.ToString();
            var rawMsg = Base64UrlEncode(encodedMsg);

            var gmailMessage = new Message()
            {
                Raw = rawMsg
            };

            var request = service.Users.Messages.Send(gmailMessage, "me");
            var result = request.Execute();

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
