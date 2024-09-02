using Domain;
using Helper;
using Engine;

namespace ConsoleUI;

public static class ConsoleVisualization
{
    public static void AnnouncePlayersAndPoints(GameState state)
    {
        Console.Clear();
        var players = "";
        foreach (var player in state.PlayerList)
        {
            players += player.Nickname + " Points: " + player.Points + ", ";
        }

        players = players.Remove(players.Length - 2, 2);
        Console.WriteLine("Welcome TO UNO! Your current points are:");
        Console.WriteLine(players);
        Console.WriteLine("");
        Console.WriteLine("It is " + state.CurrentRound + " round! Good luck!");
        Console.WriteLine("Are you guys ready? Press any key to continue..");
        Console.ReadLine();
    }

    public static void AskNextPlayerToStartTheirTurn(GameState state)
    {
        Console.Clear();
        Console.WriteLine("It is " + state.PlayerList[state.ActivePlayerNumber].Nickname + "'s turn!");
        Console.WriteLine("Confirm that you are ready by pressing ENTER");
        Console.ReadLine();
    }

    public static void AnnounceWinner(GameState state, UnoEngine unoEngine)
    {
        state.CurrentRound++;
        Console.Clear();
        Console.WriteLine("WE GOT A WINNER!");
        Console.WriteLine(state.PreviousPlayer.Nickname + " IS THE WINNER OF THE CURRENT ROUND!");
        Console.WriteLine("We are calculating the points...");
        var totalPoints = unoEngine.CalculatePoints();
        state.PreviousPlayer.Points += totalPoints;
        Console.WriteLine("");
        Console.WriteLine(state.PreviousPlayer.Nickname + " WON " + totalPoints + " POINTS!");
        Console.WriteLine(state.PreviousPlayer.Nickname + " HAS NOW TOTAL OF: " +
                          state.PreviousPlayer.Points + " POINTS!");
        if (GameIsOver(state, unoEngine))
        {
            return;
        }

        if (unoEngine.CheckIfPlayerWonTheWholeGame(state.PreviousPlayer))
        {
            Console.WriteLine("Which means " + state.PreviousPlayer.Nickname +
                              " IS THE WHOLE GAME WINNER!");
            state.GameOver = true;
            return;
        }

        Console.WriteLine("");
        Console.WriteLine("Lets start a new round!");
        var input = ReadLine.ReadLineHelper("Are you guys ready? Type 'Ready' to Console!", new string("string"), 5, new List<int>(), new List<string>(){"READY"})[0];
        if (input.ToLower() != "ready") return;
    }

    public static bool GameIsOver(GameState state, UnoEngine unoEngine)
    {
        if (state.CurrentRound >= unoEngine.GameSettings.NumberOfRounds + 1)
        {
            Console.WriteLine("Game is over!");
            Console.WriteLine("");
            Console.WriteLine("The Winner is: " + unoEngine.FindWinnerByScore());
            Console.WriteLine("Press ENTER to get back to MAIN MENU");
            Console.ReadLine();
            state.GameOver = true;
            return true;
        }

        return false;
    }

    public static void AskUserToEndTheirTurn(GameState state, UnoEngine unoEngine)
    {
        Console.WriteLine("Your turn is done.");
        var input = ReadLine.ReadLineHelper("You can type(challenge prev. player(c) or UNO) or press ENTER to Confirm", new string("null/string"), 3, new List<int>(), new List<string>(){"C","UNO"})[0];
        Console.WriteLine(input);
        if (input.ToUpper().Contains("C"))
        {
            if (unoEngine.GameSettings.MustSayUno && state.PreviousPlayer is { HasSaidUno: false, UnoCards.Count: 1 })
            {
                ClearPlayerTopCard(state);
                unoEngine.GivePlayerTwoCardsNotSayingUno(state.PreviousPlayer);
                state.PreviousPlayer.DidNotSayUno = true;
            }
        }

        if (state.PlayerList[state.ActivePlayerNumber].UnoCards.Count == 1 && input.ToUpper().Contains("UNO"))
        {
            state.PlayerList[state.ActivePlayerNumber].HasSaidUno = true;
        }
    }


    public static void ClearPlayerTopCard(GameState state)
    {
        Console.Clear();
        Console.WriteLine("Currently is playing: " + state.PlayerList[state.ActivePlayerNumber].Nickname);
        Console.WriteLine("");
        foreach (var player in state.PlayerList)
        {
            if (player == state.PlayerList[state.ActivePlayerNumber]) continue;
            if (player.HasSaidUno)
            {
                Console.Write("" + player.Nickname + " has said UNO, " );
            }

            Console.Write("" + player.Nickname + " has " + player.UnoCards.Count +
                          (player.UnoCards.Count == 1 ? " card! " : " cards! "));
        }

        Console.WriteLine("");

        Console.WriteLine("The top card is: " + state.DiscardPile.Last());

        if (!state.PlayerList[state.ActivePlayerNumber].DidNotSayUno) return;
        Console.WriteLine("");
        Console.WriteLine("You did not say UNO and you were challenged!");
        Console.WriteLine("SO YOU GET EXTRA TWO CARDS!");
        state.PlayerList[state.ActivePlayerNumber].DidNotSayUno = false;
    }

    public static string ShowPlayersHand(GameState state)
    {
        var numCount = 0;
        var playerCardList = "";
        var unoCards = state.PlayerList[state.ActivePlayerNumber].UnoCards;
        unoCards = unoCards.OrderBy(card =>
        {
            if (card.ECardColor.HasValue)
            {
                return (int)card.ECardColor.Value * 100 + (int)card.ECardType;
            }

            return (int)ECardColor.None * 100 + (int)card.ECardType;
        }).ToList();
        state.PlayerList[state.ActivePlayerNumber].UnoCards = unoCards;
        foreach (var unoCard in unoCards)
        {
            numCount++;
            playerCardList += numCount + ") " + unoCard + ", ";
        }

        return playerCardList.Remove(playerCardList.Length - 2, 2);
    }

    public static void GetNewCardFromDeck(GameState state, UnoEngine unoEngine)
    {
        unoEngine.CheckIfNewCardIsAvailable();
        var newCard = state.RandomizedCards[0];
        state.RandomizedCards.RemoveAt(0);
        state.PlayerList[state.ActivePlayerNumber].UnoCards
           .Add(newCard);
        Console.Clear();
        Console.WriteLine("Top card: " + state.DiscardPile.Last());
        Console.WriteLine("");
        Console.WriteLine("You took " + newCard);
        var result =
            ReadLine.ReadLineHelper("Do you want to play " + newCard + "? (Y/N)", new string("string"), 1, new List<int>(), new List<string>(){"Y", "N"})[0];
        if (result.ToUpper() != "Y")
        {
            state.PlayerList[state.ActivePlayerNumber].HasSaidUno = false;
            return;
        }

        if (unoEngine.CheckIfCardIsAcceptableAndAdd(state.PlayerList[state.ActivePlayerNumber].UnoCards.Count))
        {
            Console.WriteLine("Your card is put into the game.");
            Console.WriteLine("Your hand: " + ShowPlayersHand(state));
        }
        else
        {
            state.PlayerList[state.ActivePlayerNumber].HasSaidUno = false;
            Console.WriteLine("This cart is not acceptable!");
        }
    }


    public static void WildCard(GameState state)
    {
        var type = state.DiscardPile.Last().ECardType;
        state.DiscardPile.RemoveAt(state.DiscardPile.Count - 1);
        while (true)
        {
            var input = ReadLine.ReadLineHelper("Pick next color by typing: 'Red', 'Yellow', 'Green', 'Blue'",
                new string("string"), 6, new List<int>(), new List<string>{"RED", "YELLOW", "GREEN","BLUE"})[0].ToLower();
            switch (input)
            {
                case "red":
                    state.DiscardPile.Add(new UnoCard(ECardColor.Red, type));
                    return;
                case "yellow":
                    state.DiscardPile.Add(new UnoCard(ECardColor.Yellow, type));
                    return;
                case "green":
                    state.DiscardPile.Add(new UnoCard(ECardColor.Green, type));
                    return;
                case "blue":
                    state.DiscardPile.Add(new UnoCard(ECardColor.Blue, type));
                    return;
            }
        }
    }
}