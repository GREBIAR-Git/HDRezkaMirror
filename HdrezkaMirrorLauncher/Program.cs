using System;
using System.Diagnostics;
using System.Text;
using HdrezkaMirrorSite;

namespace HdrezkaMirrorLauncher;

class Program
{
    public static void Main()
    {
        Console.OutputEncoding = Encoding.Unicode;
        new MirrorSiteOpener("адрес", "пароль для внешних приложений");
        Process.GetCurrentProcess().Kill();
    }
}