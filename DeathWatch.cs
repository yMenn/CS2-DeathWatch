using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

namespace DeathWatch;

public partial class DeathWatch : BasePlugin, IPluginConfig<DeathWatchConfig>
{
    #if DEBUG
    private const string BuildConfig = "Debug";
    #else
    private const string BuildConfig = "Release";
    #endif

    public override string ModuleName => $"DeathWatch ({BuildConfig})";
    public override string ModuleVersion => "0.1.0";
    public override string ModuleAuthor => "menn";

    public char NewLine = '\u2029';
    public DeathWatchConfig Config { get; set; } = new();

    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            CleanUp();
        }
        Logger.LogInformation("[DeathWatch] Plugin loaded.");
    }

    public void OnConfigParsed(DeathWatchConfig config)
    {
        if (config.MAX_DEATH_GROUP_GAP < 0)
            config.MAX_DEATH_GROUP_GAP = 1;

        if (config.DEATH_ALERT_COUNT < 0)
            config.DEATH_ALERT_COUNT = 2;

        Config = config;
    }
}

