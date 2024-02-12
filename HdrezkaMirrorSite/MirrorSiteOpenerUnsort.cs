using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace HdrezkaMirrorSite;

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

        client.Inbox.Open(FolderAccess.ReadOnly);

        var query = SearchQuery.FromContains(emailHDrezka);
        var uids = client.Inbox.Search(query);

        foreach (var uid in uids)
        {
            var message = client.Inbox.GetMessage(uid);
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
            break;
        }
        client.Disconnect(true);
        return null;
    }

    protected override int CountMessage(string from, string password)
    {
        using var client = new ImapClient();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        client.Inbox.Open(FolderAccess.ReadOnly);

        var query = SearchQuery.FromContains(emailHDrezka);
        var uids = client.Inbox.Search(query);

        return uids.Count;
    }
}
