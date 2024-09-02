using Domain;
using Helper;
using static System.Int32;

namespace ConsoleApp;

public static class ChangeSettings
{
    public static string? ChangeNumberOfRounds(Settings settings)
    {
        while (true)
        {
            Console.Write($"Enter how many rounds you want to play (0=Unlimited, until points are reached): ");
            var rounds = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(rounds)) return null;
            if (!TryParse(rounds, out var result))
            {
                Console.WriteLine("Parse error...");
                continue;
            }

            if (result < 0)
            {
                Console.WriteLine("Can't be lower than 0.");
                continue;
            }

            settings.NumberOfRounds = result == 0 ? MaxValue : result;
            return null;
        }
    }

    public static string? ChangePointsToWin(Settings settings)
    {
        var points =
            ReadLine.ReadLineHelper("Enter how many points you need to win: ", new string("int"), 1, new List<int>(), new List<string>())[0];

        settings.PointsToWin = Parse(points);
        return null;
    }

    public static string? ChangeHandSize(Settings settings)
    {
        var size = ReadLine.ReadLineHelper("Enter hand size (2 - 10):", new string("int"), 1,
            new List<int>{2, 3, 4, 5, 6, 7, 8, 9, 10}, new List<string>())[0];
        settings.HandSize = Parse(size);
        return null;
    }

    public static string? ChangeUno(Settings settings)
    {
        var uno = ReadLine.ReadLineHelper("Do you have to say UNO (F, T)?", new string("string"), 1, new List<int>(), new List<string>{"F", "T"})[0];
        switch (uno.ToUpper())
        {
            case "F":
                settings.MustSayUno = false;
                break;
            case "T":
                settings.MustSayUno = true;
                break;
        }

        return null;
    }
}