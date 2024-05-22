using ShellProgressBar;

namespace HdrezkaMirrorSite;

public static class ProgressBarTheme
{
    public static ProgressBarOptions MainTheme()
    {
        return new()
        {
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkYellow,
            ProgressCharacter = '─'
        };
    }

    public static ProgressBarOptions SubTheme()
    {
        return new()
        {
            ForegroundColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ProgressCharacter = '─'
        };
    }
}