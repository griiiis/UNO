using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Engine;

namespace WepApp.Pages.Play;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IGameRepository _gameRepository;
    public UnoEngine Engine { get; set; } = default!;


    public IndexModel(AppDbContext context)
    {
        _context = context;
        _gameRepository = new GameRepositoryDb(_context);
    }

    [BindProperty(SupportsGet = true)] public Guid GameId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid PlayerId { get; set; }
    [BindProperty(SupportsGet = true)] public int CardId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Color { get; set; }
    [BindProperty(SupportsGet = true)] public string? NewCard { get; set; }
    [BindProperty(SupportsGet = true)] public string? FinishMove { get; set; }
    [BindProperty(SupportsGet = true)] public string? Skipped { get; set; }
    [BindProperty(SupportsGet = true)] public string? TwoCards { get; set; }
    [BindProperty(SupportsGet = true)] public string? FourCards { get; set; }
    [BindProperty(SupportsGet = true)] public string? SaidUno { get; set; }
    [BindProperty(SupportsGet = true)] public string? ChallengePlayer { get; set; }


    public IActionResult OnGet()
    {
        var gameState = _gameRepository.LoadGame(GameId);
        var players = gameState.PlayerList;

        Engine = new UnoEngine(gameState.GameSettings, players)
        {
            State = gameState
        };
        Engine.PreviousPlayer();
        Engine.State.PreviousPlayer.WrongUno = false;
        if (Engine.State.CurrentRound == Engine.GameSettings.NumberOfRounds + 1 ||
            Engine.CheckIfPlayerWonTheWholeGame(Engine.State.PreviousPlayer))
        {
            _gameRepository.SaveGame(GameId, gameState);
            return Page();
        }
        if (Engine.State.PlayerList.FindAll(player1 => !player1.UnoCards.Any()).Count == 1
            || Engine.State.PlayerList.FindAll(player1 => player1.Points >= 500).Count == 1)
        {
            Engine.State.PreviousPlayer.Points += Engine.CalculatePoints();
            Engine.State.CurrentRound++;
            Engine.State.PreviousPlayer.HasSaidUno = false;
            Engine.MakeNewDeck();
            Engine.GivePlayersTheirCards();
            Engine.GetFirstCard();
            var random = new Random();
            Engine.State.ActivePlayerNumber = random.Next(Engine.State.PlayerList.Count);
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId});
        }

        if (NewCard != null && Engine.GetActivePlayer().NewCardTaken == false)
        {
            Engine.GetActivePlayer().NewCardTaken = true;
            Engine.GetActivePlayer().HasSaidUno = false;
            Engine.CheckIfNewCardIsAvailable();
            var newCard = gameState.RandomizedCards[0];
            gameState.RandomizedCards.RemoveAt(0);
            gameState.PlayerList[gameState.ActivePlayerNumber].UnoCards
                .Add(newCard);
            _gameRepository.SaveGame(GameId, gameState);
        }

        Engine.State.PreviousPlayer.NewCardTaken = false;

        if (ChallengePlayer != null && Engine.State.PreviousPlayer.HasSaidUno == false)
        {
            Engine.GivePlayerTwoCardsNotSayingUno(Engine.State.PreviousPlayer);
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (SaidUno != null) // UNO
        {
            var player = Engine.GetActivePlayer();
            if (player.UnoCards.Count == 2)
            {
                player.HasSaidUno = true;
            }
            else
            {
                player.WrongUno = true;
                Engine.GivePlayerTwoCardsNotSayingUno(Engine.GetActivePlayer());
            }
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }
        
        if (Skipped != null) //PLAYER SKIPPED
        {
            Engine.State.PlayerSkipped = false;
            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (TwoCards != null) //PLAYER DRAW TWO
        {
            Engine.GivePlayerTwoCardsAndSkip();
            Engine.State.PlayerDrawTwo = false;
            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (FourCards != null) //PLAYER WILD FOUR
        {
            
            Engine.GivePlayerFourCardsAndSkip();
            Engine.State.PlayerDrawFour = false;
            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (FinishMove != null)
        {
            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (CardId != 0)
        {
            if (Color != null) //Wild Card color
            {
                Engine.GetCardByIndex(CardId).ECardColor = (ECardColor)int.Parse(Color);
            }

            Engine.CheckIfCardIsAcceptableAndAdd(CardId);
            Engine.ActionAndWildCardEffects();
            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        if (Engine.GetActivePlayer().PlayerType == PlayerType.Ai) //AI KORD
        {
            Console.WriteLine(Engine.GetActivePlayer().Nickname + " KORD");
            Console.WriteLine("MUL ON NII PALJU KAARTE "+Engine.GetActivePlayer().UnoCards.Count);
            if (Engine.State.PreviousPlayer.UnoCards.Count == 1 && Engine.State.PreviousPlayer.HasSaidUno != true && Engine.State.GameSettings.MustSayUno)
            {
                Engine.GivePlayerTwoCardsNotSayingUno(Engine.State.PreviousPlayer);
            }

            if (Engine.State.PlayerSkipped) //AI SKIPPED
            {
                Engine.State.PlayerSkipped = false;
                Engine.NextPlayer();
                _gameRepository.SaveGame(GameId, gameState);
                return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
            }
            if (Engine.State.PlayerDrawTwo) //AI DRAW TWO
            {
                Engine.GivePlayerTwoCardsAndSkip();
                Engine.State.PlayerDrawTwo = false;
                Engine.NextPlayer();
                _gameRepository.SaveGame(GameId, gameState);
                return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
            }
            if (Engine.State.PlayerDrawFour) //AI DRAW FOUR
            {
                Engine.GivePlayerFourCardsAndSkip();
                Engine.State.PlayerDrawFour = false;
                Engine.NextPlayer();
                _gameRepository.SaveGame(GameId, gameState);
                return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
            }
            var ai = Engine.GetActivePlayer();
            Engine.ActionAndWildCardEffects(); /////////////// ON VAJA?!!!!!!!!!!?!?!?!?
            
            for (int idx = 1; idx <= ai.UnoCards.Count; idx++)
            {

                if (Engine.CheckIfCardIsAcceptableAndAddAi(idx))
                {
                    if (ai.UnoCards.Count == 1 && Engine.State.GameSettings.MustSayUno)
                    {
                        ai.HasSaidUno = true;
                    }

                    Engine.State.NewCardPlaced = true;
                    Engine.ActionAndWildCardEffects();
                    Engine.NextPlayer();
                    _gameRepository.SaveGame(GameId, gameState);
                    return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
                }
            }

            ai.UnoCards.Add(Engine.State.RandomizedCards[0]);
            Engine.State.RandomizedCards.RemoveAt(0);
            ai.HasSaidUno = false;
            if (Engine.CheckIfCardIsAcceptableAndAddAi(ai.UnoCards.Count))
            {
                if (ai.UnoCards.Count == 1)
                {
                    ai.HasSaidUno = true;
                }

                Engine.State.NewCardPlaced = true;
                Engine.ActionAndWildCardEffects();
            }

            Engine.NextPlayer();
            _gameRepository.SaveGame(GameId, gameState);
            return RedirectToPage("/Play/Index", new { gameId = GameId, playerId = PlayerId });
        }

        return Page();
    }


    public string LastDiscardCard(UnoCard card)
    {
        var newCard = "";
        var cardIntVal = (int)card.ECardType;
        if (cardIntVal <= 9)
        {
            var cardClass = "card num-" + cardIntVal + " " + card.ECardColor;
            newCard += $@"<div class=""{@cardClass}"">
                            <div class=""inner"">
                                <div class=""mark"">{@cardIntVal}</div>
                            </div>
                        </div>";
            return newCard;
        }

        if (card.ECardType == ECardType.Skip)
        {
            var cardClass = "card num-skip " + card.ECardColor;
            newCard += $@"<div class =""{@cardClass}"">
                            <div class =""inner"">
                                <div class =""skip"">⦸</div >
                            </div >
                           </div >";
            return newCard;
        }

        if (card.ECardType == ECardType.DrawTwo)
        {
            var cardClass = "card num-drawTwo " + card.ECardColor;
            newCard += $@"<div class =""{@cardClass}"">
                              <div class =""inner"">
                                <div class =""drawTwo"" >
                                    <img src=https://uno-cards.vercel.app/assets/twocards-7eb808bd.svg>
                                </div >
                             </div >
                    </div >";
            return newCard;
        }

        if (card.ECardType == ECardType.Reverse)
        {
            var cardClass = "card num-reverse " + card.ECardColor;
            newCard += $@"<div class =""{@cardClass}"">
                    <div class =""inner"">
                    <div class =""reverse"">
                    <img src=https://uno-cards.vercel.app/assets/twocards-7eb808bd.svg>
                    </div>
                    </div>
                    </div>";
            return newCard;
        }

        if (card.ECardType == ECardType.Wild)
        {
            newCard += $@"<div class =card>
                <div class=""inner"">
                <img src=lib/wildCard.PNG width=105px height=165px >
                </div>
                </div>";
            return newCard;
        }
        else
        {
            newCard += $@"<div class=card>
                <div class=""inner"">
                <img src=lib/wildCard+4.PNG width=105px height=165px>
                </div>
                </div>";
            return newCard;
        }
    }
}