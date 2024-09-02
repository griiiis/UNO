using DAL;
using Domain;
using Helper;
using Engine;

namespace ConsoleUI;

public class GameController
{
    private readonly UnoEngine _unoEngine;
    private readonly IGameRepository _gameRepository;

    public GameController(UnoEngine unoEngine, IGameRepository gameRepository)
    {
        _unoEngine = unoEngine;
        _gameRepository = gameRepository;
    }

    public void RunGame()
    {
        Console.Clear();
        RoundStart();
        while (true)
        {
            if (ConsoleVisualization.GameIsOver(_unoEngine.State, _unoEngine))
            {
                break;
            }
            if (_unoEngine.State.PreviousPlayer != null && _unoEngine.State.PreviousPlayer.UnoCards.Count == 0)
            {
                ConsoleVisualization.AnnounceWinner(_unoEngine.State, _unoEngine);
                if (_unoEngine.State.CurrentRound - 1 == _unoEngine.GameSettings.NumberOfRounds ||
                    _unoEngine.State.GameOver)
                {
                    break;
                }

                StartNewRound();
                continue;
            }
            if (_unoEngine.GetActivePlayer().PlayerType == PlayerType.Ai) //AI TURN
            {
                if (_unoEngine.State.PreviousPlayer != null && _unoEngine.State.PreviousPlayer.UnoCards.Count == 1 &&
                    _unoEngine.State.PreviousPlayer.HasSaidUno != true && _unoEngine.State.GameSettings.MustSayUno)
                {
                    _unoEngine.GivePlayerTwoCardsNotSayingUno(_unoEngine.State.PreviousPlayer);
                }

                if (_unoEngine.State.PlayerSkipped) //AI SKIPPED
                {
                    _unoEngine.State.PlayerSkipped = false;
                    _unoEngine.NextPlayer();
                    _gameRepository.SaveGame(_unoEngine.State.Id, _unoEngine.State);
                    continue;
                }

                if (_unoEngine.State.PlayerDrawTwo) //AI DRAW TWO
                {
                    _unoEngine.GivePlayerTwoCardsAndSkip();
                    _unoEngine.State.PlayerDrawTwo = false;
                    _unoEngine.NextPlayer();
                    _gameRepository.SaveGame(_unoEngine.State.Id, _unoEngine.State);
                    continue;
                }

                if (_unoEngine.State.PlayerDrawFour) //AI DRAW FOUR
                {
                    _unoEngine.GivePlayerFourCardsAndSkip();
                    _unoEngine.State.PlayerDrawFour = false;
                    _unoEngine.NextPlayer();
                    _gameRepository.SaveGame(_unoEngine.State.Id, _unoEngine.State);
                    continue;
                }

                var ai = _unoEngine.GetActivePlayer();
                for (int idx = 1; idx <= ai.UnoCards.Count; idx++)
                {
                    if (_unoEngine.CheckIfCardIsAcceptableAndAddAi(idx))
                    {
                        if (ai.UnoCards.Count == 1 && _unoEngine.State.GameSettings.MustSayUno)
                        {
                            ai.HasSaidUno = true;
                        }
                        _unoEngine.State.NewCardPlaced = true;
                        break;
                    }
                }
                if (!_unoEngine.State.NewCardPlaced)
                {
                    ai.UnoCards.Add(_unoEngine.State.RandomizedCards[0]);
                    _unoEngine.State.RandomizedCards.RemoveAt(0);
                    ai.HasSaidUno = false;
                    if (_unoEngine.CheckIfCardIsAcceptableAndAddAi(ai.UnoCards.Count))
                    {
                        if (ai.UnoCards.Count == 1)
                        {
                            ai.HasSaidUno = true;
                        }
                        _unoEngine.State.NewCardPlaced = true;
                    }
                }
                _unoEngine.ActionAndWildCardEffects();
                _unoEngine.State.PreviousPlayer = _unoEngine.State.PlayerList[_unoEngine.State.ActivePlayerNumber];
                _unoEngine.NextPlayer();
                _gameRepository.SaveGame(_unoEngine.State.Id, _unoEngine.State);
            }
            else
            {
                ConsoleVisualization.AskNextPlayerToStartTheirTurn(_unoEngine.State);
                if (_unoEngine.State.PlayerSkipped)
                {
                    ConsoleVisualization.ClearPlayerTopCard(_unoEngine.State);
                    Console.WriteLine("Which means that you are SKIPPED.");
                    _unoEngine.State.PlayerSkipped = false;
                    ConsoleVisualization.AskUserToEndTheirTurn(_unoEngine.State, _unoEngine);
                    _unoEngine.NextPlayer();
                    if (SaveGame() == "n")
                    {
                        break;
                    }

                    continue;
                }

                if (_unoEngine.State.PlayerDrawTwo)
                {
                    ConsoleVisualization.ClearPlayerTopCard(_unoEngine.State);
                    Console.WriteLine("");
                    Console.WriteLine("YOU GET EXTRA TWO CARDS AND SKIP YOUR TURN!");
                    _unoEngine.GivePlayerTwoCardsAndSkip();
                    Console.WriteLine("You took 2 new cards!");
                    Console.WriteLine("Your hand: " + ConsoleVisualization.ShowPlayersHand(_unoEngine.State));
                    ConsoleVisualization.AskUserToEndTheirTurn(_unoEngine.State, _unoEngine);
                    _unoEngine.NextPlayer();
                    if (SaveGame() == "n")
                    {
                        break;
                    }

                    continue;
                }

                if (_unoEngine.State.PlayerDrawFour)
                {
                    ConsoleVisualization.ClearPlayerTopCard(_unoEngine.State);
                    Console.WriteLine("");
                    Console.WriteLine("YOU GET EXTRA FOUR CARDS AND SKIP YOUR TURN!");
                    _unoEngine.GivePlayerFourCardsAndSkip();
                    Console.WriteLine("You took 4 new cards!");
                    Console.WriteLine("Your hand: " + ConsoleVisualization.ShowPlayersHand(_unoEngine.State));
                    ConsoleVisualization.AskUserToEndTheirTurn(_unoEngine.State, _unoEngine);
                    _unoEngine.NextPlayer();
                    if (SaveGame() == "n")
                    {
                        break;
                    }

                    continue;
                }

                ConsoleVisualization.ClearPlayerTopCard(_unoEngine.State);
                Console.WriteLine("Your hand: " + ConsoleVisualization.ShowPlayersHand(_unoEngine.State));
                var cards = new List<int>();
                for (int i = 1; i < _unoEngine.GetActivePlayer().UnoCards.Count + 1; i++)
                {
                    cards.Add(i);
                }

                var result = ReadLine.ReadLineHelper("Do you want to put a card (Card num) or take a card? (T)",
                    new string("string/int"), 1, cards, new List<string>(){"T"})[0];
                if (result.ToUpper() == "T")
                {
                    ConsoleVisualization.GetNewCardFromDeck(_unoEngine.State, _unoEngine);
                }
                else
                {
                    int? newCardPicked = null;
                    while (true)
                    {
                        var cardPicked = newCardPicked ?? int.Parse(result);
                        if (_unoEngine.CheckIfCardIsAcceptableAndAdd(cardPicked))
                        {
                            break;
                        }

                        Console.WriteLine("This cart is not acceptable!");
                        var input = ReadLine.ReadLineHelper("Do you want to pick another card from your set? (Y/N)",
                            new string("string"), 1, new List<int>(), new List<string>(){"Y", "N"})[0];
                        if (input.ToUpper() == "N")
                        {
                            ConsoleVisualization.GetNewCardFromDeck(_unoEngine.State, _unoEngine);
                            break;
                        }

                        if (input.ToUpper() != "Y") continue;
                        Console.Clear();
                        Console.WriteLine("Your hand: " + ConsoleVisualization.ShowPlayersHand(_unoEngine.State));
                        Console.WriteLine("The top card is: " + _unoEngine.State.DiscardPile.Last());
                        var cardNums = new List<int>();
                        for (int i = 1; i < _unoEngine.GetActivePlayer().UnoCards.Count + 1; i++)
                        {
                            cardNums.Add(i);
                        }

                        var newCardPickedString = ReadLine.ReadLineHelper("Which card do you want to put (Card Num)?",
                            new string("int"), 0, cardNums, new List<string>())[0];
                        newCardPicked = int.Parse(newCardPickedString);
                    }
                }
                
                if (_unoEngine.State.DiscardPile.Last().ECardType == ECardType.Wild && _unoEngine.State.NewCardPlaced
                    || _unoEngine.State.DiscardPile.Last().ECardType == ECardType.WildDrawFour &&
                    _unoEngine.State.NewCardPlaced)
                {
                    ConsoleVisualization.WildCard(_unoEngine.State);
                }

                _unoEngine.ActionAndWildCardEffects();
                ConsoleVisualization.AskUserToEndTheirTurn(_unoEngine.State, _unoEngine);
                _unoEngine.State.PreviousPlayer = _unoEngine.State.PlayerList[_unoEngine.State.ActivePlayerNumber];
                _unoEngine.NextPlayer();
                if (SaveGame() == "n")
                {
                    break;
                }
            }
        }
    }


    private string SaveGame()
    {
        _gameRepository.SaveGame(_unoEngine.State.Id, _unoEngine.State);

        return ReadLine.ReadLineHelper("State saved. Continue (Y/N)[Y]?", "null/string", 1, new List<int>(), new List<string>(){"Y", "N"})[0];
    }

    private void StartNewRound()
    {
        _unoEngine.MakeNewDeck();
        _unoEngine.GivePlayersTheirCards();
        _unoEngine.GetFirstCard();
        RoundStart();
    }

    private void RoundStart()
    {
        var random = new Random();
        _unoEngine.State.ActivePlayerNumber = random.Next(_unoEngine.State.PlayerList.Count);
        ConsoleVisualization.AnnouncePlayersAndPoints(_unoEngine.State);
    }
}