using System.Net;
using System.Net.Mail;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace HdrezkaMirrorSite;

public class Email
{
    public delegate Task ClientOperation(ImapClient imapClient);

    public delegate Task FolderOperation(IMailFolder folder);

    public delegate Task InboxOperation(IList<UniqueId> uids, IMailFolder inbox);

    public static SmtpClient Smtp(string from, string password)
    {
        return new()
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
    }

    public static async Task ImapClientAction(string from, string password, ClientOperation operation)
    {
        using ImapClient client = new();
        await client.ConnectAsync("imap.mail.ru", 993, true);

        await client.AuthenticateAsync(from, password);

        await operation(client);

        await client.DisconnectAsync(true);
    }


    public static async Task FolderAction(string from, string password, FolderOperation operation)
    {
        await ImapClientAction(from, password, async client =>
        {
            IMailFolder folder = await client.GetFolderAsync("INBOX/HDrezka");
            await folder.OpenAsync(FolderAccess.ReadOnly);

            await operation(folder);
        });
    }

    public static async Task InboxAction(string from, string password, InboxOperation operation)
    {
        await ImapClientAction(from, password, async client =>
        {
            await client.Inbox.OpenAsync(FolderAccess.ReadOnly);

            TextSearchQuery query = SearchQuery.FromContains(Configuration.emailHDrezka);
            IList<UniqueId> uids = await client.Inbox.SearchAsync(query);

            await operation(uids, client.Inbox);
        });
    }
}