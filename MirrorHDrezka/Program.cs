using System.Diagnostics;

namespace MirrorHDrezka
{
    internal class Program
    {
        static void Main()
        {
            MirrorHDRezka.OpenAsync("Адрес почты", "пароль").GetAwaiter().GetResult();
            Process.GetCurrentProcess().Kill();
        }
    }
}
