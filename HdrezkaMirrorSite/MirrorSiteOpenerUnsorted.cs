using MailKit;
using MailKit.Search;
using MimeKit;

namespace HdrezkaMirrorSite;

public class MirrorSiteOpenerUnsorted(string from, string password) : MirrorSiteOpener(from, password)
{
    protected override async Task<string?> Get()
    {
        string? foundWord = null;
        await Email.InboxAction(from, password, async (uids, inbox) =>
        {
            foreach (UniqueId uid in uids.Reverse())
            {
                MimeMessage message = await inbox.GetMessageAsync(uid);
                string bodyMailText = message.TextBody;
                bodyMailText = bodyMailText.Replace("\r\n", " ");

                foundWord = bodyMailText.Split(' ')
                    .FirstOrDefault(word => Configuration.gTLD.Any(tld => word.Contains("." + tld)));
            }
        });
        return foundWord;
    }

    protected override async Task<int> MessageNumber()
    {
        int messageNumber = 0;
        await Email.InboxAction(from, password, (uids, inbox) =>
        {
            inbox.Open(FolderAccess.ReadOnly);

            TextSearchQuery query = SearchQuery.FromContains(Configuration.emailHDrezka);
            uids = inbox.Search(query);
            messageNumber = uids.Count;
            return null;
        });
        return messageNumber;
    }
}