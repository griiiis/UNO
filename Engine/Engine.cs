using Domain;

namespace Engine;

public class UnoEngine
{
    public readonly Settings GameSettings;
    public GameState State { get; set; } = new();

    public UnoEngine(Settings gameSettings, List<Player> players)
    {
        GameSettings = gameSettings;
        State.PlayerList = players;
        State.GameSettings = GameSettings;
    }

    public bool CheckIfPlayerWonTheWholeGame(Player player)
    {
        return player.Points >= GameSettings.PointsToWin;
    }
    
    public int CalculatePoints()
    {
        var points = 0;
        foreach (var player in State.PlayerList)
        {
            foreach (var unoCard in player.UnoCards)
            {
                points += PointsSystem(unoCard);
            }
        }

        return points;
    }

    private static int PointsSystem(UnoCard unoCard)
    {
        int cardValue;
        if (unoCard.ECardType is ECardType.Skip or ECardType.Reverse or ECardType.DrawTwo)
        {
            cardValue = 20;
        }
        else
        {
            cardValue = (int)unoCard.ECardType;
        }

        return cardValue;
    }

    public void GivePlayerTwoCardsAndSkip()
    {
        for (var i = 0; i < 2; i++)
        {
            CheckIfNewCardIsAvailable();
            var newCard = State.RandomizedCards[0];
            State.PlayerList[State.ActivePlayerNumber].UnoCards.Add(newCard);
            State.RandomizedCards.RemoveAt(0);
        }

        State.PlayerDrawTwo = false;
        State.PlayerList[State.ActivePlayerNumber].HasSaidUno = false;
    }

    public void GivePlayerFourCardsAndSkip()
    {
        for (var i = 0; i < 4; i++)
        {
            CheckIfNewCardIsAvailable();
            var newCard = State.RandomizedCards[0];
            State.PlayerList[State.ActivePlayerNumber].UnoCards.Add(newCard);
            State.RandomizedCards.RemoveAt(0);
        }

        State.PlayerDrawFour = false;
        State.PlayerList[State.ActivePlayerNumber].HasSaidUno = false;
    }

    public void GivePlayerTwoCardsNotSayingUno(Player player)
    {
        CheckIfNewCardIsAvailable();
        var newCard = State.RandomizedCards[0];
        player.UnoCards.Add(newCard);
        State.RandomizedCards.RemoveAt(0);
        CheckIfNewCardIsAvailable();
        var secondNewCard = State.RandomizedCards[0];
        player.UnoCards.Add(secondNewCard);
        State.RandomizedCards.RemoveAt(0);
        State.PlayerList[State.ActivePlayerNumber].HasSaidUno = false;
    }


    public void ActionAndWildCardEffects()
    {
        if (!State.NewCardPlaced) return;
        switch (State.DiscardPile.Last().ECardType)
        {
            case ECardType.Skip:
                State.PlayerSkipped = true;
                break;
            case ECardType.Reverse:
                State.MovementDirection = State.MovementDirection == MovementDirection.Anticlockwise
                    ? MovementDirection.Clockwise
                    : MovementDirection.Anticlockwise;
                break;
            case ECardType.DrawTwo:
                State.PlayerDrawTwo = true;
                break;
            case ECardType.WildDrawFour:
                State.PlayerDrawFour = true;
                break;
        }

        State.NewCardPlaced = false;
    }

    public void NextPlayer()
    {
        switch (State.MovementDirection)
        {
            case MovementDirection.Clockwise:
            {
                State.ActivePlayerNumber++;
                if (State.ActivePlayerNumber > State.PlayerList.Count - 1)
                {
                    State.ActivePlayerNumber = 0;
                }

                break;
            }
            case MovementDirection.Anticlockwise:
            {
                State.ActivePlayerNumber--;
                if (State.ActivePlayerNumber < 0)
                {
                    State.ActivePlayerNumber = State.PlayerList.Count - 1;
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool CheckIfCardIsAcceptableAndAdd(int cardPicked)
    {
        var acceptable = CheckIfCardIsAcceptable(cardPicked);
        if (!acceptable) return false;
        State.DiscardPile.Add(State.PlayerList[State.ActivePlayerNumber].UnoCards[cardPicked - 1]);
        State.PlayerList[State.ActivePlayerNumber].UnoCards.RemoveAt(cardPicked - 1);
        State.NewCardPlaced = true;
        return true;
    }

    public bool CheckIfCardIsAcceptableAndAddAi(int cardPicked)
    {
        var acceptable = CheckIfCardIsAcceptable(cardPicked);
        if (!acceptable) return false;
        var card = State.PlayerList[State.ActivePlayerNumber].UnoCards[cardPicked - 1];
        if (card.ECardType == ECardType.Wild || card.ECardType == ECardType.WildDrawFour)
        {
            var mostCommonColor = GetActivePlayer().UnoCards.GroupBy(c => c.ECardColor).MaxBy(group => group.Count());
            if (mostCommonColor != null && mostCommonColor.Key != ECardColor.None)
            {
                card.ECardColor = mostCommonColor.Key;
            }
            else
            {
                var color = (ECardColor)new Random().Next(0, 3);
                card.ECardColor = color;
            }
        }

        State.DiscardPile.Add(card);
        State.PlayerList[State.ActivePlayerNumber].UnoCards.RemoveAt(cardPicked - 1);
        State.NewCardPlaced = true;
        return true;
    }

    public bool CheckIfCardIsAcceptable(int cardPicked)
    {
        if (cardPicked == int.MaxValue)
        {
            return true;
        }

        if (cardPicked > State.PlayerList[State.ActivePlayerNumber].UnoCards.Count)
        {
            return false;
        }

        var discardCard = State.DiscardPile.Last();
        var activePlayerCard = State.PlayerList[State.ActivePlayerNumber].UnoCards[cardPicked - 1];
        var acceptable = activePlayerCard.ECardType ==
                         ECardType.WildDrawFour ||
                         activePlayerCard.ECardType ==
                         ECardType.Wild ||
                         activePlayerCard.ECardColor ==
                         discardCard.ECardColor ||
                         (int)activePlayerCard.ECardType ==
                         (int)discardCard.ECardType;
        return acceptable;
    }

    public UnoCard GetCardByIndex(int index)
    {
        return State.PlayerList[State.ActivePlayerNumber].UnoCards[index - 1];
    }

    public void CheckIfNewCardIsAvailable()
    {
        if (State.RandomizedCards.Count > 1)
        {
            return;
        }
        var newTopCard = State.DiscardPile[0];
        State.DiscardPile.RemoveAt(0); 
        RandomizeAllCards(State.DiscardPile);
        State.DiscardPile = new List<UnoCard>() { newTopCard };
    }

    public void GetFirstCard()
    {
        var startingCard = State.RandomizedCards[0];
        State.RandomizedCards.RemoveAt(0);

        switch (startingCard.ECardType)
        {
            case ECardType.Skip:
                State.ActivePlayerNumber++;
                break;
            case ECardType.Reverse:
                State.MovementDirection = MovementDirection.Anticlockwise;
                break;
            case ECardType.DrawTwo:
                State.PlayerList[State.ActivePlayerNumber].UnoCards.Add(State.RandomizedCards[0]);
                State.RandomizedCards.RemoveAt(0);
                State.PlayerList[State.ActivePlayerNumber].UnoCards.Add(State.RandomizedCards[0]);
                State.RandomizedCards.RemoveAt(0);
                break;
            case ECardType.Wild:
                var random = new Random();
                State.DiscardPile.Add(new UnoCard((ECardColor)random.Next(4), ECardType.Wild));
                break;
        }

        if (startingCard.ECardType != ECardType.Wild)
        {
            State.DiscardPile.Add(startingCard);
        }
    }

    public void GivePlayersTheirCards()
    {
        foreach (var player in State.PlayerList)
        {
            player.UnoCards = new List<UnoCard>();
            for (var count = 0; count < GameSettings.HandSize; count++)
            {
                player.UnoCards.Add(State.RandomizedCards[count]);
                State.RandomizedCards.RemoveAt(count);
            }
        }
        CheckFirstCardIsNotWildFour();
    }

    private void CheckFirstCardIsNotWildFour()
    {
        while (true)
        {
            if (State.RandomizedCards[0].ECardType != ECardType.WildDrawFour) break;
            State.RandomizedCards.RemoveAt(0);
            State.RandomizedCards.Add(new UnoCard(ECardColor.None, ECardType.WildDrawFour));
        }
    }

    public string FindWinnerByScore()
    {
        var highestScore = 0;
        var winnerPlayerName = "";
        foreach (var player in State.PlayerList)
        {
            if (player.Points <= highestScore) continue;
            winnerPlayerName = player.Nickname;
            highestScore = player.Points;
        }
        return winnerPlayerName;
    }

    public void MakeNewDeck()
    {
        List<UnoCard> allCards = new(); //Currently total of 108cards.

        for (var color = 0; color <= (int)ECardColor.Blue; color++)
        {
            for (var type = 0; type <= (int)ECardType.Reverse; type++)
            {
                if (type != (int)ECardType.Zero)
                {
                    allCards.Add(new UnoCard((ECardColor)color, (ECardType)type));
                }

                allCards.Add(new UnoCard((ECardColor)color, (ECardType)type));
            }
        }

        for (var i = 0; i < 4; i++)
        {
            allCards.Add(new UnoCard(ECardColor.None, ECardType.Wild));
            allCards.Add(new UnoCard(ECardColor.None, ECardType.WildDrawFour));
        }

        RandomizeAllCards(allCards);
    }

    private void RandomizeAllCards(IList<UnoCard> cardDeck)
    {
        var random = new Random();
        var allCardLength = cardDeck.Count;

        for (var count = 0; count < allCardLength; count++)
        {
            var num = random.Next(cardDeck.Count);
            State.RandomizedCards.Add(cardDeck[num]);
            cardDeck.RemoveAt(num);
        }
    }

    public Player GetActivePlayer()
    {
        return State.PlayerList[State.ActivePlayerNumber];
    }

    public void PreviousPlayer()
    {
        State.PreviousPlayer = State.PlayerList[State.ActivePlayerNumber];

        switch (State.MovementDirection)
        {
            case MovementDirection.Clockwise:
            {
                if (State.ActivePlayerNumber == 0)
                {
                    State.PreviousPlayer = State.PlayerList.Last();
                }
                else
                {
                    State.PreviousPlayer = State.PlayerList[State.ActivePlayerNumber - 1];
                }

                break;
            }
            case MovementDirection.Anticlockwise:
            {
                if (State.ActivePlayerNumber == State.PlayerList.Count - 1)
                {
                    State.PreviousPlayer = State.PlayerList[0];
                }
                else
                {
                    State.PreviousPlayer = State.PlayerList[State.ActivePlayerNumber + 1];
                }

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}