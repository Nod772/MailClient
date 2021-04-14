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

    public partial class MainWindow : Window
    {

        public class User
        {
            public string Login { get; set; }
            public string Password { get; set; }
        }

        private void Autorization(User user)
        {
            try
            {

                client = new Mail.SmtpClient("smtp.gmail.com")
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user.Login, user.Password)
                };

                imap = new ImapClient();

                imap.Connect("imap.gmail.com", 993, true);

                imap.Authenticate(currentUser.Login, currentUser.Password);
                lbCurrentUser.Dispatcher.BeginInvoke((Action)delegate
                {
                    lbCurrentUser.Content = user.Login;
                });

                SignInPanel.Dispatcher.BeginInvoke((Action)delegate
                {
                    SignInPanel.Visibility = Visibility.Collapsed;
                });
                leftPanel.Dispatcher.BeginInvoke((Action)delegate
                {
                    leftPanel.Visibility = Visibility.Visible;
                });

                ListMessagePanel.Dispatcher.BeginInvoke((Action)delegate
                {
                    ListMessagePanel.Visibility = Visibility.Visible;

                });
                GetMessages();




            }
            catch (Exception)
            {
                lbException.Dispatcher.BeginInvoke((Action)delegate
                {
                    btSignIn.IsEnabled = true;
                    lbException.Content = "Помилка входу";
                });
            }
        }
    }

}
