using OpenPop.Mime;
using OpenPop.Pop3;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Pop3Message = OpenPop.Mime.Message;

namespace MirrorHDrezka
{
    internal class MirrorHDRezka
    {
        static readonly string emailHDrezka = "mirror@hdrezka.org";

        public static async Task OpenAsync(string from, string password)
        {
            string adres = POP3(from, password);
            bool websiteWorking = false;
            if (adres != null)
            {
                websiteWorking = MirrorUp("https://" + adres);
            }
            if (websiteWorking)
            {
                Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
            }
            else
            {
                SendEmailAsync(from, password).GetAwaiter().GetResult();
                Thread.Sleep(5000);
                adres = POP3(from, password);
                if (adres != null)
                {
                    Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
                }
            }
        }

        static bool MirrorUp(string url)
        {
            HttpStatusCode result = default(HttpStatusCode);
            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        result = response.StatusCode;
                        response.Close();
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.InnerException != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        static string POP3(string from, string password)
        {
            Pop3Client pop3 = new Pop3Client();
            Pop3Message message = default;

            pop3.Connect("pop.mail.ru", 995, true);
            pop3.Authenticate(from, password);

            int messageCount = pop3.GetMessageCount();
            for (int i = messageCount; i > 0; i--)
            {
                message = pop3.GetMessage(i);
                if (message.Headers.From.Address == emailHDrezka)
                {
                    break;
                }
            }
            if (message != null)
            {
                MessagePart plainTextPart = message.FindFirstPlainTextVersion();
                string bodyMailTxt = plainTextPart.GetBodyAsText();

                bodyMailTxt = bodyMailTxt.Replace('\n', ' ');
                bodyMailTxt = bodyMailTxt.Replace('\r', ' ');
                foreach (string word in bodyMailTxt.Split(' '))
                {
                    if (word.Contains(".net"))
                    {
                        return word;
                    }
                }
            }
            return null;
        }

        static async Task SendEmailAsync(string from, string password)
        {
            SmtpClient client = new SmtpClient
            {
                UseDefaultCredentials = false,
                Port = 25,
                EnableSsl = true,
                DeliveryFormat = SmtpDeliveryFormat.International,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = "smtp.mail.ru",
                Timeout = 300000,
                Credentials = new NetworkCredential(from, password)
            };

            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(from)
            };
            mailMessage.To.Add(emailHDrezka);
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.BodyTransferEncoding = System.Net.Mime.TransferEncoding.Base64;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;

            await client.SendMailAsync(mailMessage);
            client.Dispose();
        }
    }
}
