using System.Text.Json.Serialization;

namespace Domain;

public class Player(string nickname, PlayerType playerType)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [JsonInclude] public string Nickname { get; set; } = nickname;
    [JsonInclude] public PlayerType PlayerType { get; set; } = playerType;
    [JsonInclude] public List<UnoCard> UnoCards { get; set; } = new();
    [JsonInclude] public int Points { get; set; }
    [JsonInclude] public bool HasSaidUno { get; set; }
    [JsonInclude] public bool DidNotSayUno { get; set; }
    [JsonInclude] public bool WrongUno { get; set; }
    [JsonInclude] public bool NewCardTaken { get; set; }
}