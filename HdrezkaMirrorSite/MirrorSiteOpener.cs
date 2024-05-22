using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.InteropServices;
using MimeKit;
using ShellProgressBar;

namespace HdrezkaMirrorSite;

public class MirrorSiteOpener(string from, string password)
{
    public async Task Open()
    {
        using var main = new ProgressBar(2, "Получаем закэшированный адрес...", ProgressBarTheme.MainTheme());
        string? address = await Get();
        if (address != null)
        {
            main.Tick("Проверяем работоспособность зеркала...");
            address = "http://" + address;
            bool isWebsiteWorking = await IsMirrorWork(address);
            if (isWebsiteWorking)
            {
                main.Tick("Открываем актулаьное зеркало...");
                OpenBrowser(address);
                return;
            }
        }

        main.Tick("Запрашиваем зеркало...");
        using var child = main.Spawn(20, "Ждём ответ", ProgressBarTheme.SubTheme());
        await Send();
        int initialMessageCount = await MessageNumber();
        for (int i = 0; i < 20; i++)
        {
            if (initialMessageCount != await MessageNumber())
            {
                address = await Get();
                if (address != null)
                {
                    OpenBrowser("http://" + address);
                }

                return;
            }

            child.Tick();
            await Task.Delay(250);
        }
    }


    static void OpenBrowser(string address)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(address) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", address);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", address);
        }
    }

    static async Task<bool> IsMirrorWork(string url)
    {
        using HttpClient client = new();
        client.Timeout = TimeSpan.FromMinutes(10);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(Configuration.UserAgent);

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

    protected virtual async Task<int> MessageNumber()
    {
        int messageCount = 0;
        await Email.FolderAction(from, password, folder =>
        {
            messageCount = folder.Count;
            return Task.CompletedTask;
        });
        return messageCount;
    }

    protected virtual async Task<string?> Get()
    {
        string? foundWord = null;
        await Email.FolderAction(from, password, async folder =>
        {
            if (folder.Count > 0)
            {
                MimeMessage message = await folder.GetMessageAsync(folder.Count - 1);
                string bodyMailText = message.TextBody.Replace(Configuration.LineEndings, " ");

                foundWord = bodyMailText.Split(' ')
                    .FirstOrDefault(word => Configuration.gTLD.Any(tld => word.Contains("." + tld)));
            }
        });
        return foundWord;
    }

    async Task Send()
    {
        using SmtpClient client = Email.Smtp(from, password);

        MailMessage mailMessage = new()
        {
            From = new(from)
        };
        mailMessage.To.Add(Configuration.emailHDrezka);

        await client.SendMailAsync(mailMessage);
    }
}