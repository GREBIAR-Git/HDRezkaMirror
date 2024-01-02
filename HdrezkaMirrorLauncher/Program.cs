using HdrezkaMirrorSite;
using System.Diagnostics;

namespace HdrezkaMirrorLauncher;

class Program
{
    public static void Main()
    {
        new MirrorSiteOpener("адрес", "пароль");
        Process.GetCurrentProcess().Kill();
    }
}
