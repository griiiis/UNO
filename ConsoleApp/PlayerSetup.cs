using Domain;
using Helper;

namespace ConsoleApp;

public static class PlayerSetup
{
    public static List<Player> PlayerQuestions()
    {
        Console.Clear();
        int playersInput;
        while (true)
        {
            var result = ReadLine.ReadLineHelper("HOW MANY PLAYERS?", new string("int"), 0,
                new List<int>(){2, 3, 4, 5, 6, 7, 8, 9, 10}, new List<string>())[0];
            playersInput = int.Parse(result);
            break;
        }

        int humanInput;
        while (true)
        {
            var result = ReadLine.ReadLineHelper("HOW MANY HUMAN PLAYERS?", new string("int"), 0,
                new List<int>{0,1,2,3,4,5,6,7,8,9,10}, new List<string>())[0];
            humanInput = int.Parse(result);
            break;
        }

        var players = new List<Player>();
        for (var i = 1; i <= humanInput; i++)
        {
            var input =
                ReadLine.ReadLineHelper("Pick nickname for Player" + i + ":", new string("string"), 10, new List<int>(), new List<string>())[0];
            players.Add(new Player(input, PlayerType.Human));
        }

        var aiPlayers = playersInput - humanInput;
        if (aiPlayers <= 0)
        {
            return players;
        }

        for (var i = 1; i <= aiPlayers; i++)
        {
            Console.Clear();
            var input = ReadLine.ReadLineHelper("Pick nickname for AI" + i + ":", new string("string"), 10, new List<int>(), new List<string>())[0];
            players.Add(new Player(input, PlayerType.Ai));
        }

        return players;
    }
}