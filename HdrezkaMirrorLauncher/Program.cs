using HdrezkaMirrorSite;
using System;
using System.Diagnostics;

namespace HdrezkaMirrorLauncher;

class Program
{
    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.Unicode;
        new MirrorSiteOpener("адрес", "пароль");
        Process.GetCurrentProcess().Kill();
    }
}
