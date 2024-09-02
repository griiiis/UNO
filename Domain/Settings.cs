using System.Text.Json.Serialization;

namespace Domain;

public class Settings
{
    [JsonInclude] public int NumberOfRounds { get; set; } = 3;
    [JsonInclude] public int PointsToWin { get; set; } = 500;
    [JsonInclude] public int HandSize { get; set; } = 7;
    [JsonInclude] public bool MustSayUno { get; set; } = true;
}