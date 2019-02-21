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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MultiMailSender.Helpers
{
    static class CommonHelpers
    {
        private static string[] _scopes;

        public static string[] Scopes 
            => _scopes ?? (_scopes = new[] { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailCompose, GmailService.Scope.GmailSend, GmailService.Scope.GmailModify });

        public static void ClearSession()
        {
            if (Directory.Exists(Constants._tokenFolderName))
            {
                var di = new DirectoryInfo(Constants._tokenFolderName);
                foreach (var file in di.GetFiles())
                {
                    file.Delete();
                }
            }
        }

        public static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }
    }
}
