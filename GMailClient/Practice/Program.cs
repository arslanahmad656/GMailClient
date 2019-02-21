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

namespace Practice
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/gmail-dotnet-quickstart.json
        static string[] _scopes = { GmailService.Scope.GmailReadonly };
        static string _applicationName = "Gmail API .NET Quickstart";

        static void Main(string[] args)
        {
            var service = GMailHelper.GetGMailService(_scopes, _applicationName);

            var labels = GMailHelper.GetLabels(service);
            PrintLabels(labels);

            var drafts = GMailHelper.GetDrafts(service);
            var draft = GMailHelper.GetDraft(service, "r-3167230408123907724");
            
        }

        static void PrintDrafts(List<Draft> drafts)
        {

        }

        static void PrintLabels(List<string> labels)
        {
            Console.WriteLine("Labels:");
            if (labels != null && labels.Count > 0)
            {
                foreach (var labelItem in labels)
                {
                    Console.WriteLine("{0}", labelItem);
                }
            }
            else
            {
                Console.WriteLine("No labels found.");
            }
        }
    }
}
