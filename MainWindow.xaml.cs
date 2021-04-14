using MailKit.Net.Imap;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using Mail = System.Net.Mail;
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
using System.Net;
using MailKit.Net.Smtp;
using MimeKit;
using System.IO;
using System.Threading;

namespace MailClientWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Mail.SmtpClient client = null;
        Mail.MailMessage mail = null;
        MailKit.MailFolder inbox = null;
        List<MimeMessage> messages;

        ImapClient imap = null;
        User currentUser = null;

        int currentState;



        public MainWindow()
        {
            InitializeComponent();

            ListMessagePanel.Visibility = Visibility.Collapsed;
            NewMessagePanel.Visibility = Visibility.Collapsed;
            leftPanel.Visibility = Visibility.Collapsed;
            AnswerMessagePanel.Visibility = Visibility.Collapsed;

            currentState = 0;


        }




        private void Imap()
        {

            imap = new ImapClient();

            imap.Connect("imap.gmail.com", 993, true);
            try
            {

                imap.Authenticate(currentUser.Login, currentUser.Password);
                GetMessages();
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }

        private void GetMessages()
        {
            if (imap.IsAuthenticated)
            {
                Console.WriteLine("user OK");
                imap.Inbox.Open(MailKit.FolderAccess.ReadOnly);
                inbox = (MailKit.MailFolder)imap.Inbox;

                // List<MimeKit.MimeMessage> messages = new List<MimeKit.MimeMessage>();
                currentState = inbox.Count - 1;
                DownloadOldMessages(ref currentState, 10);
            }
            else
            {
                Console.WriteLine("user not ok");
                // return;
            }
        }

        private void DownloadOldMessages(ref int state, int countMessage)
        {
            messages = new List<MimeMessage>();
            if (state >= 0)
            {

                int countStep = 0;
                try
                {

                    for (int i = state; i > state - countMessage; i--)
                    {
                        if (i >= 0)
                        {
                            messages.Add(inbox.GetMessage(i));
                            countStep++;
                            //var ms = inbox.GetMessage(i);

                            //Console.WriteLine("Sender\t"+ms.Sender.Address);
                        }
                        else
                        {
                            break;
                        }

                    }
                    btOpenNewMessages.Dispatcher.BeginInvoke((Action)delegate
                    {
                        btOpenNewMessages.IsEnabled = true;
                    });


                    state -= countStep;
                    if (state == 0)
                    {
                        btOpenOldMessages.Dispatcher.BeginInvoke((Action)delegate
                        {
                            btOpenOldMessages.IsEnabled = false;

                        });
                    }
                    if (state >= inbox.Count)
                    {
                        state--;
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                ListMessagePanel.Dispatcher.BeginInvoke((Action)delegate
                {

                    ListMessagePanel.DataContext = messages;
                });
            }
        }
        private void DownloadNewMessages(ref int state, int countMessage)
        {
            messages = new List<MimeMessage>();
            if (state <= inbox.Count)
            {

                int countStep = 0;
                try
                {

                    for (int i = state; i < state + countMessage; i++)
                    {
                        if (i != inbox.Count)
                        {
                            messages.Add(inbox.GetMessage(i));
                            countStep++;
                        }
                        else
                        {
                            break;
                        }

                    }
                    btOpenOldMessages.Dispatcher.BeginInvoke((Action)delegate
                    {

                        btOpenOldMessages.IsEnabled = true;
                    });

                    state += countStep;
                    if (state >= inbox.Count)
                    {
                        btOpenNewMessages.Dispatcher.BeginInvoke((Action)delegate
                        {
                            btOpenNewMessages.IsEnabled = false;

                        });
                        state--;
                    }

                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                ListMessagePanel.Dispatcher.BeginInvoke((Action)delegate
                {

                    ListMessagePanel.DataContext = messages;
                });
            }

        }
        private void BtBackToInbox(object sender, RoutedEventArgs e)
        {
            ClearFormNewMessages();
            ListMessagePanel.Visibility = Visibility.Visible;
            NewMessagePanel.Visibility = Visibility.Collapsed;
            AnswerMessagePanel.Visibility = Visibility.Collapsed;
        }

        private void ClearFormNewMessages()
        {
            tbSentTo.Text = "";
            tbHeader.Text = "";
            textMessage.Text = "";
            pathAttachment.Content = "";

        }

        private void BtNewMessage(object sender, RoutedEventArgs e)
        {
            ListMessagePanel.Visibility = Visibility.Collapsed;
            NewMessagePanel.Visibility = Visibility.Visible;
            AnswerMessagePanel.Visibility = Visibility.Collapsed;
            mail = new Mail.MailMessage();
        }

        private void AttcachClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == true)
            {
                Mail.Attachment attachment = new Mail.Attachment(open.FileName);
                mail.Attachments.Add(attachment);


                pathAttachment.Content = System.IO.Path.GetFileName(open.FileName);

            }

        }

        private void SentMessage(object sender, RoutedEventArgs e)
        {
            if (MakeMessage())
            {

                Thread thread = new Thread(() => client.Send(mail));
                thread.Start();
                thread.Join();
                BtBackToInbox(null, null);

                Console.WriteLine("OK");
            }

        }

        private bool MakeMessage()
        {
            if (!String.IsNullOrWhiteSpace(tbSentTo.Text) && !String.IsNullOrWhiteSpace(tbHeader.Text))
            {
                try
                {

                    mail.From = new Mail.MailAddress(currentUser.Login);
                    mail.To.Add(tbSentTo.Text);

                    mail.Subject = tbHeader.Text;
                    mail.Body = textMessage.Text;
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return false;
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(Imap);
            thread.Start();
        }

        private void BtOpenNewMessages_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() => DownloadNewMessages(ref currentState, 10));
            thread.Start();
        }

        private void BtOpenOldMessages_Click(object sender, RoutedEventArgs e)
        {
            Thread thread = new Thread(() => DownloadOldMessages(ref currentState, 10));
            thread.Start();
        }

        private void BtSignIn_Click(object sender, RoutedEventArgs e)
        {
            currentUser = new User { Login = tbLogin.Text, Password = tbPassword.Password };
            btSignIn.IsEnabled = false;
            Thread thread = new Thread(() => Autorization(currentUser));
            thread.Start();
        }

        private void BtAnswer_Click(object sender, RoutedEventArgs e)
        {

            try
            {

                mail.From = new Mail.MailAddress(currentUser.Login);
                MimeMessage ms = listMessages.SelectedValue as MimeMessage;
                Console.WriteLine(ms.From.Mailboxes.First().Address);

                mail.To.Add(ms.From.Mailboxes.First().Address);

                mail.Body = answerTextBlock.Text;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
            Thread thread = new Thread(() => client.Send(mail));
            thread.Start();
            listMessages.SelectedItem = null;
            BtBackToInbox(null, null);
            answerTextBlock.Text = "";

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListMessagePanel.Visibility = Visibility.Collapsed;
            AnswerMessagePanel.Visibility = Visibility.Visible;
            mail = new Mail.MailMessage();
        }

        private void BtBackToListMessage(object sender, RoutedEventArgs e)
        {
            listMessages.SelectedItem = null;
            ListMessagePanel.Visibility = Visibility.Visible;
            AnswerMessagePanel.Visibility = Visibility.Collapsed;
            answerTextBlock.Text = "";
        }

        private void AnswerTextBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string str = (sender as TextBox).Text + "\n";
                (sender as TextBox).Text = str;
                (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
                //(sender as TextBox).Text.Insert((sender as TextBox).SelectionStart, Environment.NewLine);
                // (sender as TextBox).SelectionStart = (sender as TextBox).Text.Length;
            }
        }
    }
}
