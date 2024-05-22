using System.Runtime.InteropServices;

namespace HdrezkaMirrorSite;

public class Configuration
{
    public static readonly string emailHDrezka = "mirror@hdrezka.org";

    public static readonly List<string> gTLD = ["net", "org"];

    public static readonly string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 YaBrowser/24.1.0.0 Safari/537.36";

    public static string LineEndings { get; } = LineEndingsProvider();

    static string LineEndingsProvider()
    {
        string lineEndings;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            lineEndings = "\r";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            lineEndings = "\n";
        }
        else
        {
            lineEndings = "\r\n";
        }

        return lineEndings;
    }
}