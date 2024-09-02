using System.Text.Json.Serialization;

namespace Domain;

public class GameState
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonInclude] public List<UnoCard> RandomizedCards { get; set; } = new();
    [JsonInclude] public List<UnoCard> DiscardPile { get; set; } = new();
    [JsonInclude] public int ActivePlayerNumber { get; set; }
    [JsonInclude] public int CurrentRound = 1;
    [JsonInclude] public List<Player> PlayerList { get; set; } = new();
    [JsonInclude] public Settings GameSettings { get; set; } = default!;
    [JsonInclude] public MovementDirection MovementDirection;
    [JsonInclude] public bool NewCardPlaced { get; set; }
    [JsonInclude] public bool PlayerSkipped { get; set; }
    [JsonInclude] public bool PlayerDrawTwo { get; set; }
    [JsonInclude] public bool PlayerDrawFour { get; set; }
    [JsonInclude] public Player PreviousPlayer { get; set; } = default!;
    [JsonInclude] public bool GameOver;
}