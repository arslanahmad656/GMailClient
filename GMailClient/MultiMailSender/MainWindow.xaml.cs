using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MultiMailSender.Helpers;

namespace MultiMailSender
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<string> _recepients;
        private string _recepientsFile;
        //private string _attachementFile;
        private OpenFileDialog _recepientsFileDialog;
        private OpenFileDialog _attachementFileDialog;
        private GmailService _gmailService;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            InitiateSession();
            AttachEventHandlers();
        }

        private void Initialize()
        {
            _recepients = new List<string>();
            _recepientsFileDialog = new OpenFileDialog
            {
                Title = "Choose a file containing the recepient email addresses",
                Multiselect = false
            };

            _attachementFileDialog = new OpenFileDialog
            {
                Title = "Choose a file to attach",
                Multiselect = false
            };
        }

        private void AttachEventHandlers()
        {
            Button_RecepientFileBrowse.Click += (s, e) => SetRecepients();
            Button_InitiateSession.Click += (s, e) => InitiateSession();
            Button_ClearSession.Click += (s, e) => CommonHelpers.ClearSession();
            Button_DisplayToFileContents.Click += (s, e) => DisplayRecepients();
            Button_Attachement.Click += (s, e) => SetAttachementFile();
            Button_Send.Click += (s, e) => StartSending();
        }

        async void StartSending()
        {
            if (!Validate())
            {
                return;
            }

            var errors = new List<Exception>();

            var from = TextBox_From.Text;
            var subject = TextBox_Subject.Text;
            var body = TextBox_Body.Text;
            var attachement = TextBox_Attachement.Text;
            foreach (var recepient in _recepients)
            {
                try
                {
                    var result = await GMailHelpers.SendEmailAsync(_gmailService, from, recepient, subject, body, attachement, true);
                }
                catch (Exception ex)
                {
                    ex.HelpLink = recepient;
                    errors.Add(ex);
                }
            }

            if (errors.Count == 0)
            {
                MessageBox.Show("Operation Complete!");
            }
            else
            {
                var sb = new StringBuilder("Following errors occurred: " + Environment.NewLine);
                errors.ForEach(e =>
                {
                    sb.Append($"{e.HelpLink}: {e.Message}{Environment.NewLine}");
                });
                ShowErrorMessage(sb.ToString());
            }
        }

        private void SetAttachementFile()
        {
            var result = _attachementFileDialog.ShowDialog() ?? false;
            if (!result)
            {
                return;
            }

            if (!File.Exists(_attachementFileDialog.FileName))
            {
                ShowErrorMessage($"File {_attachementFileDialog.FileName} does not exist.");
                return;
            }

            TextBox_Attachement.Text = _attachementFileDialog.FileName;
        }

        private void DisplayRecepients()
        {
            var text = new StringBuilder();
            _recepients.ForEach(r => text.Append(r + Environment.NewLine));
            MessageBox.Show(text.ToString());
        }

        private void InitiateSession()
        {
            _gmailService = GMailHelpers.GetGMailService(CommonHelpers.Scopes);
        }

        private void SetRecepients()
        {
            _recepients.Clear();
            var result = _recepientsFileDialog.ShowDialog() ?? false;
            if (!result)
            {
                return;
            }

            var path = _recepientsFileDialog.FileName;
            if (!File.Exists(path))
            {
                ShowErrorMessage($"File {path} not found.");
                return;
            }

            var emailAddresses = File.ReadAllLines(path);   // assumed that file contains exactly one email address on each line
            _recepients.AddRange(emailAddresses);
            TextBox_ToFile.Text = path;
        }

        private void ShowErrorMessage(string text, string title = "Error")
        {
            MessageBox.Show(this, text, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool Validate()
        {
            bool valid = true;
            if (_recepients == null || _recepients.Count == 0)
            {
                valid = false;
                ShowErrorMessage("No recepients added. Recepients can be added by clicking 'Choose File' button.");
            }

            if (_gmailService == null)
            {
                valid = false;
                ShowErrorMessage("GMail session might not be initialized. Click 'Initiate Session' button to start one.");
            }

            //var invalidEmailAddresses = new List<string>();
            //_recepients.ForEach(r =>
            //{
            //    if (!CommonHelpers.IsValidEmail(r))
            //    {
            //        invalidEmailAddresses.Add(r);
            //    }
            //});

            //if (invalidEmailAddresses.Count != 0)
            //{
            //    var sb = new StringBuilder("The following email addresses in the file are not valid:");
            //}

            if (string.IsNullOrWhiteSpace(TextBox_From.Text))
            {
                valid = false;
                ShowErrorMessage("From email address not specified. Specify the email address associated with the session.");
            }

            if (string.IsNullOrWhiteSpace(TextBox_Subject.Text))
            {
                valid = false;
                ShowErrorMessage("Subject cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(TextBox_Body.Text))
            {
                var result = MessageBox.Show(this, "Message body is empty. Do you want to continue without a body?", "Body Empty!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                valid = result == MessageBoxResult.Yes;
            }

            if (string.IsNullOrWhiteSpace(TextBox_Attachement.Text))
            {
                var result = MessageBox.Show(this, "Message body is empty. Do you want to continue without a body?", "Body Empty!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                valid = result == MessageBoxResult.Yes;
            }
            else
            {
                if (!File.Exists(TextBox_Attachement.Text))
                {
                    valid = false;
                    ShowErrorMessage($"Attachement file {TextBox_Attachement.Text} does not exist.");
                }
            }

            return valid;
        }
    }
}
