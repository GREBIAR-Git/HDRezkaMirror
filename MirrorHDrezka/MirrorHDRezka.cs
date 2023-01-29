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

        static readonly string extension = "org";

        public static async Task OpenAsync(string from, string password)
        {
            string adres = Get(from, password);
            if (adres != null)
            {
                bool websiteWorking = IsMirrorWork("https://" + adres);
                if (websiteWorking)
                {
                    Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
                    return;
                }
            }
            await SendAsync(from, password);
            Thread.Sleep(5000);
            adres = Get(from, password);
            if (adres != null)
            {
                Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
            }
        }

        static bool IsMirrorWork(string url)
        {
            WebRequest request = HttpWebRequest.Create(url);
            request.Method = "HEAD";
            try
            {
                using HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    HttpStatusCode result = response.StatusCode;
                    response.Close();
                }
            }
            catch (WebException wex)
            {
                if (wex.InnerException != null)
                {
                    return false;
                }
            }
            return true;
        }

        static string Get(string from, string password)
        {
            Pop3Client pop3 = new();

            pop3.Connect("pop.mail.ru", 995, true);
            pop3.Authenticate(from, password);

            int messageCount = pop3.GetMessageCount();

            for (int i = messageCount; i > 0 && messageCount - 40 < i; i--)
            {
                Pop3Message message = pop3.GetMessage(i);
                if (message.Headers.From.Address == emailHDrezka)
                {
                    string bodyMailText = message.FindFirstPlainTextVersion().GetBodyAsText();
                    bodyMailText = bodyMailText.Replace("\n\r", " ");

                    foreach (string word in bodyMailText.Split(' '))
                    {
                        if (word.Contains("." + extension))
                        {
                            return word;
                        }
                    }
                    break;
                }
            }
            return null;
        }

        static async Task SendAsync(string from, string password)
        {
            SmtpClient client = new()
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

            MailMessage mailMessage = new()
            {
                From = new MailAddress(from)
            };
            mailMessage.To.Add(emailHDrezka);

            await client.SendMailAsync(mailMessage);
            client.Dispose();
        }
    }
}
