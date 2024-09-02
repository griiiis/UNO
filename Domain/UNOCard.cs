using System.Text.Json.Serialization;

namespace Domain;

[method: JsonConstructor]
public class UnoCard(ECardColor? ECardColor, ECardType ECardType)
{
    [JsonPropertyName("ECardType")] public ECardType ECardType { get; set; } = ECardType;
    [JsonPropertyName("ECardColor")] public ECardColor? ECardColor { get; set; } = ECardColor;


    public override string ToString()
    {
        var color = ECardColor != Domain.ECardColor.None ? ECardColor.ToString() : "";
        return color + " " + ECardType;
    }
}