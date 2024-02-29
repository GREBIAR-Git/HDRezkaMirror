using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using ShellProgressBar;

namespace HdrezkaMirrorSite;

public class MirrorSiteOpener
{
    protected readonly string emailHDrezka = "mirror@hdrezka.org";

    protected readonly string extension1 = "net";

    protected readonly string extension2 = "org";

    public MirrorSiteOpener(string from, string password)
    {
        OpenAsync(from, password).Wait();
    }

    async Task OpenAsync(string from, string password)
    {
        string address = Get(from, password);
        if (address != null)
        {
            bool websiteWorking = await IsMirrorWork("http://" + address);
            if (websiteWorking)
            {
                Process.Start(new ProcessStartInfo("http://" + address) { UseShellExecute = true });
                return;
            }
        }

        await SendAsync(from, password);
        int countMsg = CountMessage(from, password);
        Console.WriteLine("Запрос зеркала...");
        ProgressBarOptions options = new()
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };
        using ProgressBar progressBar = new(20, "Получение зеркала", options);
        for (int i = 0; i < 20; i++)
        {
            if (countMsg != CountMessage(from, password))
            {
                address = Get(from, password);
                if (address != null)
                {
                    Process.Start(new ProcessStartInfo("http://" + address) { UseShellExecute = true });
                }

                return;
            }

            progressBar.Tick();
            Thread.Sleep(250);
        }
    }

    async Task<bool> IsMirrorWork(string url)
    {
        string userAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 YaBrowser/24.1.0.0 Safari/537.36";

        using HttpClient client = new();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        try
        {
            HttpResponseMessage result = await client.GetAsync(url);

            return result.RequestMessage.RequestUri.OriginalString == url;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    protected virtual int CountMessage(string from, string password)
    {
        using ImapClient client = new();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        IMailFolder folder = client.GetFolder("INBOX/HDrezka");
        folder.Open(FolderAccess.ReadOnly);

        client.Disconnect(true);
        return folder.Count;
    }

    protected virtual string Get(string from, string password)
    {
        using ImapClient client = new();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        IMailFolder folder = client.GetFolder("INBOX/HDrezka");
        folder.Open(FolderAccess.ReadOnly);
        if (folder.Count > 0)
        {
            MimeMessage message = folder.GetMessage(folder.Count - 1);

            string bodyMailText = message.TextBody;
            bodyMailText = bodyMailText.Replace("\r\n", " ");

            foreach (string word in bodyMailText.Split(' '))
            {
                if (word.Contains("." + extension1) || word.Contains("." + extension2))
                {
                    client.Disconnect(true);
                    return word;
                }
            }
        }

        client.Disconnect(true);
        return null;
    }

    async Task SendAsync(string from, string password)
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