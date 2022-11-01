using System.Diagnostics;

namespace MirrorHDrezka
{
    internal class Program
    {
        static void Main()
        {
            MirrorHDRezka.OpenAsync("почта", "пароль").GetAwaiter().GetResult();//только майл
            Process.GetCurrentProcess().Kill();
        }
    }
}
