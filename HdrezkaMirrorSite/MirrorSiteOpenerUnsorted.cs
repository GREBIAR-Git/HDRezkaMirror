using System.Collections.Generic;
using System.Linq;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;

namespace HdrezkaMirrorSite;

public class MirrorSiteOpenerUnsorted : MirrorSiteOpener
{
    public MirrorSiteOpenerUnsorted(string from, string password) : base(from, password)
    {
    }

    protected override string Get(string from, string password)
    {
        using ImapClient client = new();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        client.Inbox.Open(FolderAccess.ReadOnly);

        TextSearchQuery query = SearchQuery.FromContains(emailHDrezka);
        IList<UniqueId> uids = client.Inbox.Search(query);

        foreach (UniqueId uid in uids.Reverse())
        {
            MimeMessage message = client.Inbox.GetMessage(uid);
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
        using ImapClient client = new();
        client.Connect("imap.mail.ru", 993, true);

        client.Authenticate(from, password);

        client.Inbox.Open(FolderAccess.ReadOnly);

        TextSearchQuery query = SearchQuery.FromContains(emailHDrezka);
        IList<UniqueId> uids = client.Inbox.Search(query);

        return uids.Count;
    }
}