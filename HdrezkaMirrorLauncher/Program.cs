using System.Text;
using HdrezkaMirrorSite;

Console.OutputEncoding = Encoding.Unicode;
MirrorSiteOpener mirrorSiteOpener = new("адрес", "пароль");
await mirrorSiteOpener.Open();

await Task.Delay(1000);

Environment.Exit(0);