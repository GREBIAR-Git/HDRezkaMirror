using MailKit;
using MailKit.Net.Imap;
using ShellProgressBar;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace HdrezkaMirrorSite;

//Если на почте есть сортировка писем от HDrezki в папку HDrezka
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
        string adres = Get(from, password);
        if (adres != null)
        {
            bool websiteWorking = await IsMirrorWork("https://" + adres);
            if (websiteWorking)
            {
                Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
                return;
            }
        }
        await SendAsync(from, password);
        int countMsg = CountMessage(from, password);
        Console.WriteLine("Запрос зеркала...");
        var options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };
        using var pbar = new ProgressBar(20, "Получение зеркала", options);
        for (int i = 0; i < 20; i++)
        {
            if (countMsg != CountMessage(from, password))
            {
                adres = Get(from, password);
                if (adres != null)
                {
                    Process.Start(new ProcessStartInfo("https://" + adres) { UseShellExecute = true });
                }
                return;
            }
            pbar.Tick();
            Thread.Sleep(250);
        }
    }

    async Task<bool> IsMirrorWork(string url)
    {
        WebRequest request = WebRequest.Create(url);
        request.Method = "HEAD";
        try
        {
            using HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse;
            return true;
        }
        catch (WebException wex) when (wex.InnerException != null)
        {
            return false;
        }
        catch (WebException)
        {
            return true;
        }
    }

    int CountMessage(string from, string password)
    {
        using var client = new ImapClient();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        var folder = client.GetFolder("INBOX/HDrezka");
        folder.Open(FolderAccess.ReadOnly);

        client.Disconnect(true);
        return folder.Count;
    }

    protected virtual string Get(string from, string password)
    {
        using var client = new ImapClient();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        var folder = client.GetFolder("INBOX/HDrezka");
        folder.Open(FolderAccess.ReadOnly);

        var message = folder.GetMessage(folder.Count - 1);

        string bodyMailText = message.TextBody;
        bodyMailText = bodyMailText.Replace("\n\r", " ");

        foreach (string word in bodyMailText.Split(' '))
        {
            if (word.Contains("." + extension1) || word.Contains("." + extension2))
            {
                client.Disconnect(true);
                return word;
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
