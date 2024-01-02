using MailKit;
using MailKit.Net.Imap;

namespace HdrezkaMirrorSite;

//Если на почте нет сортировки писем от HDrezki в папку HDrezka
public class MirrorSiteOpenerUnsort : MirrorSiteOpener
{
    public MirrorSiteOpenerUnsort(string from, string password) : base(from, password)
    {
    }

    protected override string Get(string from, string password)
    {
        using var client = new ImapClient();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        var folder = client.GetFolder("INBOX");
        folder.Open(FolderAccess.ReadOnly);

        int messageCount = folder.Count - 1;

        for (int i = messageCount; i > 0 && messageCount - 40 < i; i--)
        {
            var message = folder.GetMessage(i);

            if (message.From.ToString() == emailHDrezka)
            {
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
                break;
            }
        }
        client.Disconnect(true);
        return null;
    }
}
