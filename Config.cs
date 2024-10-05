using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace DeathWatch;

public class DeathWatchConfig : BasePluginConfig
{
    [JsonPropertyName("ChatMessagePrefix")] public string pluginTag { get; set; } = $" {ChatColors.LightRed}DeathWatch |{ChatColors.White}";
    [JsonPropertyName("MaxDeathGroupTimeGap")] public int MAX_DEATH_GROUP_GAP { get; set; } = 3;
    [JsonPropertyName("DeathEventAlertSize")] public int DEATH_ALERT_COUNT { get; set; } = 5;
    [JsonPropertyName("PlaceMarkerOnDeathLocation")] public bool PLACE_MARKER_ON_DEATH { get; set; } = true;
    [JsonPropertyName("TagsToAlerOfMassDeath")] public List<string> TAGS_TO_ALERT_OF_MASS_DEATH { get; set; } = ["@css/cheats", "@css/root"];
}