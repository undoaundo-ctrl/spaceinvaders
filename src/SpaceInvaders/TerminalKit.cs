using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

/// <summary>
/// TerminalKit - a tiny console helper library used by the game in this repo.
/// It only uses the .NET base class library, so there are no NuGet packages to restore.
/// </summary>
internal static class Term
{
    // ---------------------------------------------------------------- colours
    public const string Reset = "\u001b[0m";
    public const string Bold = "\u001b[1m";
    public const string Dim = "\u001b[2m";
    public const string Reverse = "\u001b[7m";
    public const string Red = "\u001b[31m";
    public const string Green = "\u001b[32m";
    public const string Yellow = "\u001b[33m";
    public const string White = "\u001b[37m";
    public const string Blue = "\u001b[34m";
    public const string Magenta = "\u001b[35m";
    public const string Cyan = "\u001b[36m";
    public const string Gray = "\u001b[90m";
    public const string BrightRed = "\u001b[91m";
    public const string BrightGreen = "\u001b[92m";
    public const string BrightYellow = "\u001b[93m";
    public const string BrightBlue = "\u001b[94m";
    public const string BrightMagenta = "\u001b[95m";
    public const string BrightCyan = "\u001b[96m";
    public const string BrightWhite = "\u001b[97m";

    private static readonly Regex AnsiPattern = new Regex("\u001b\\[[0-9;]*m");

    /// <summary>Prepare the console (UTF-8 output and a window title).</summary>
    public static void Setup(string title)
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch (Exception) { }
        try { Console.Title = title; } catch (Exception) { }
    }

    public static void Clear()
    {
        try { Console.Clear(); } catch (Exception) { }
    }

    /// <summary>Clear the screen and move the cursor home using ANSI codes.</summary>
    public static void ClearScreen()
    {
        Write("\u001b[H\u001b[2J");
    }

    // ---------------------------------------------------------------- output
    public static void Write(string text)
    {
        try { Console.Write(text); } catch (Exception) { }
    }

    public static void Line(string text = "")
    {
        Write(text + Environment.NewLine);
    }

    public static string Read(string prompt = "")
    {
        Write(prompt);
        try
        {
            string? value = Console.ReadLine();
            return value ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static void Sleep(int milliseconds)
    {
        try { Thread.Sleep(milliseconds); } catch (Exception) { }
    }

    // ---------------------------------------------------------------- input
    public static bool KeyAvailable
    {
        get
        {
            try { return Console.KeyAvailable; } catch (Exception) { return false; }
        }
    }

    /// <summary>Read a single key without echoing it. Returns null when input is redirected.</summary>
    public static ConsoleKeyInfo? Key()
    {
        try { return Console.ReadKey(true); } catch (Exception) { return null; }
    }

    public static void AnyKey(string prompt = "Press any key to continue...")
    {
        Write(Dim + prompt + Reset);
        Key();
        Line();
    }

    /// <summary>Show a prompt and wait for a single key, returning it.</summary>
    public static ConsoleKeyInfo? WaitKey(string prompt = "Press any key...")
    {
        Write(Dim + prompt + Reset);
        return Key();
    }

    /// <summary>Throw away every key that is already buffered.</summary>
    public static void FlushKeys()
    {
        int guard = 0;
        while (KeyAvailable && guard < 64)
        {
            Key();
            guard++;
        }
    }

    public static bool IsQuit(ConsoleKeyInfo? key)
    {
        if (!key.HasValue) return false;
        return key.Value.Key == ConsoleKey.Q || key.Value.Key == ConsoleKey.Escape;
    }

    // ---------------------------------------------------------------- text
    public static string Strip(string text)
    {
        return text == null ? string.Empty : AnsiPattern.Replace(text, string.Empty);
    }

    public static int VisibleWidth(string text)
    {
        return Strip(text).Length;
    }

    /// <summary>Pad a string to a visible width (ANSI aware).</summary>
    public static string Pad(string text, int width)
    {
        int missing = width - VisibleWidth(text);
        if (missing <= 0) return text;
        return text + new string(' ', missing);
    }

    public static string PadLeft(string text, int width)
    {
        int missing = width - VisibleWidth(text);
        if (missing <= 0) return text;
        return new string(' ', missing) + text;
    }

    public static string Center(string text, int width)
    {
        int missing = width - VisibleWidth(text);
        if (missing <= 0) return text;
        int left = missing / 2;
        return new string(' ', left) + text + new string(' ', missing - left);
    }

    public static void Rule(int width = 64, char ch = '-')
    {
        Line(Gray + new string(ch, width) + Reset);
    }

    /// <summary>Print a boxed banner, used at the top of every game.</summary>
    public static void Banner(string title, string subtitle = "")
    {
        int width = Math.Max(VisibleWidth(title), VisibleWidth(subtitle)) + 4;
        Line();
        Line(Cyan + "+" + new string('-', width) + "+" + Reset);
        Line(Cyan + "|" + Reset + Bold + Center(title, width) + Reset + Cyan + "|" + Reset);
        if (subtitle.Length > 0)
        {
            Line(Cyan + "|" + Reset + Dim + Center(subtitle, width) + Reset + Cyan + "|" + Reset);
        }
        Line(Cyan + "+" + new string('-', width) + "+" + Reset);
        Line();
    }

    /// <summary>Print an aligned table with a header row.</summary>
    public static void Table(string[] headers, IEnumerable<string[]> rows)
    {
        int columns = headers.Length;
        int[] widths = new int[columns];
        for (int i = 0; i < columns; i++) widths[i] = VisibleWidth(headers[i]);
        List<string[]> data = new List<string[]>(rows);
        foreach (string[] row in data)
        {
            for (int i = 0; i < columns && i < row.Length; i++)
            {
                int w = VisibleWidth(row[i]);
                if (w > widths[i]) widths[i] = w;
            }
        }

        Line(Bold + string.Join("  ", Headers(headers, widths)) + Reset);
        foreach (string[] row in data)
        {
            Line(string.Join("  ", Cells(row, widths)));
        }
    }

    private static string[] Headers(string[] headers, int[] widths)
    {
        string[] result = new string[headers.Length];
        for (int i = 0; i < headers.Length; i++) result[i] = Pad(headers[i], widths[i]);
        return result;
    }

    private static string[] Cells(string[] row, int[] widths)
    {
        string[] result = new string[row.Length];
        for (int i = 0; i < row.Length; i++)
        {
            int width = i < widths.Length ? widths[i] : 0;
            result[i] = Pad(row[i] ?? string.Empty, width);
        }
        return result;
    }

    // ------------------------------------------------------------- questions
    public static int AskInt(string prompt, int min, int max)
    {
        while (true)
        {
            string answer = Read(prompt).Trim();
            int value;
            if (int.TryParse(answer, out value) && value >= min && value <= max) return value;
            Line(Yellow + "  Please enter a number between " + min + " and " + max + "." + Reset);
        }
    }

    public static bool AskYesNo(string prompt)
    {
        while (true)
        {
            string answer = Read(prompt + " [y/n] ").Trim().ToLowerInvariant();
            if (answer == "y" || answer == "yes") return true;
            if (answer == "n" || answer == "no") return false;
            Line(Yellow + "  Please answer y or n." + Reset);
        }
    }

    /// <summary>Show a numbered menu and return the selected index (0 based).</summary>
    public static int AskChoice(string prompt, string[] options)
    {
        Line();
        Line(Bold + prompt + Reset);
        for (int i = 0; i < options.Length; i++)
        {
            Line("  " + BrightCyan + (i + 1) + Reset + ". " + options[i]);
        }
        Line();
        return AskInt("  > ", 1, options.Length) - 1;
    }

    /// <summary>Random element of a list (null safe).</summary>
    public static T Pick<T>(IList<T> items, Random random)
    {
        if (items == null || items.Count == 0) return default(T)!;
        return items[random.Next(items.Count)];
    }
}
