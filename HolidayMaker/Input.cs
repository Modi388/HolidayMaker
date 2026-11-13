using System.Globalization;

namespace HolidayMaker;

// Hjälpmetoder för inmatning. De frågar igen vid felaktig inmatning,
// så att vanliga användarfel inte kraschar programmet.
public static class Input
{
    public static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
            Console.WriteLine("  Fältet får inte vara tomt.");
        }
    }

    public static int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
            {
                return value;
            }
            Console.WriteLine($"  Ange ett heltal mellan {min} och {max}.");
        }
    }

    // Tom inmatning betyder "inget filter" och ger null.
    public static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            if (int.TryParse(text, out int value) && value >= 0)
            {
                return value;
            }
            Console.WriteLine("  Ange ett heltal (0 eller mer), eller tryck Enter för inget filter.");
        }
    }

    public static DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (DateOnly.TryParseExact(Console.ReadLine()?.Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
            {
                return date;
            }
            Console.WriteLine("  Skriv datumet som ÅÅÅÅ-MM-DD, t.ex. 2026-12-10.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt + " (j/n): ");
            string? answer = Console.ReadLine()?.Trim().ToLower();
            if (answer == "j") return true;
            if (answer == "n") return false;
            Console.WriteLine("  Svara j eller n.");
        }
    }
}
