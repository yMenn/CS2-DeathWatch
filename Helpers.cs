using System.ComponentModel;
using System.Data;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Utils;

namespace DeathWatch;

public class DeathStack
{
    private readonly Dictionary<CCSPlayerController, DeathNode> dictionary = [];
    private DeathNode? top;
    private DeathNode? bottom;
    public int Count { get; private set; } = 0;

    public void Push(CCSPlayerController player, Vector? position, QAngle? angle, int eventId, DateTime deathTime)
    {
        var info = new DeathInfo(player, position, angle, eventId, deathTime);
        var node = new DeathNode(info);

        if (bottom is null)
        {
            bottom = node;
            top = node;
        }
        else
        {
            node.Next = top;
            top!.Previous = node;
            top = node;
        }

        dictionary[player] = node;
        Count++;
    }

    public void Push(DeathInfo info)
    {
        var node = new DeathNode(info);

        if (bottom is null)
        {
            bottom = node;
            top = node;
        }
        else
        {
            node.Next = top;
            top!.Previous = node;
            top = node;
        }

        info.PlayerRef.TryGetTarget(out var player);
        if (player is not null)
        {
            dictionary[player] = node;
            Count++;
        }
    }

    public DeathInfo? Pop()
    {
        if (top is null) return null;

        var info = top.Info;
        Remove(top);
        return info;
    }

    public DeathInfo? Remove(CCSPlayerController player)
    {
        if (!dictionary.TryGetValue(player, out var node)) return null;
        return Remove(node);
    }

    private DeathInfo Remove(DeathNode node)
    {
        if (node.Previous != null)
            node.Previous.Next = node.Next;
        else
            bottom = node.Next;

        if (node.Next != null)
            node.Next.Previous = node.Previous;
        else
            top = node.Previous;

        dictionary.Remove(node.Info.PlayerRef!.TryGetTarget(out var player) ? player : null!);
        Count--;
        return node.Info;
    }

    public void Clear()
    {
        dictionary.Clear();
        top = null;
        bottom = null;
        Count = 0;
    }

    public DeathInfo? Peek()
    {
        return top?.Info;
    }

    public List<DeathInfo> GetLastN(int n)
    {
        var result = new List<DeathInfo>();
        var current = top;
        while (current != null && result.Count < n)
        {
            result.Add(current.Info);
            current = current.Next;
        }
        return result;
    }
}

public class DeathNode(DeathInfo info)
{
    public DeathInfo Info { get; set; } = info;
    public DeathNode? Previous { get; set; }
    public DeathNode? Next { get; set; }
}

public class DeathInfo(CCSPlayerController player, Vector? position, QAngle? angle, int eventId, DateTime deathTime)
{
    public WeakReference<CCSPlayerController> PlayerRef { get; private set; } = new WeakReference<CCSPlayerController>(player);
    public Vector? Position { get; private set; } = position;
    public QAngle? PlayerAngle { get; private set; } = angle;
    public int EventId { get; set; } = eventId;
    public DateTime DeathTime { get; private set; } = deathTime;

    public bool TryGetPlayer(out CCSPlayerController? player)
    {
        return PlayerRef.TryGetTarget(out player);
    }
}

public class DeathEvent(DateTime startTime, int eventId, int alertThreshold)
{
    public int EventId { get; private set; } = eventId;
    public DateTime Start { get; private set; } = startTime;
    public DateTime Send { get; set; }
    public List<DeathInfo> Deaths { get; set; } = [];
    public int DeathCount { get; set; } = 0;
    public int AlertThreshold { get; set; } = alertThreshold;
    private CounterStrikeSharp.API.Modules.Timers.Timer? alertTimer;

    public DateTime GetLastDeathTime()
    {
        return Deaths.Count > 0 ? Deaths[^1].DeathTime : Start;
    }

    public void AddDeath(DeathInfo info, DeathWatch instance)
    {
        Deaths.Add(info);
        DeathCount++;
        AlertIfPastThreshold(instance);
    }

    public void AlertIfPastThreshold(DeathWatch instance)
    {
        if (DeathCount >= AlertThreshold)
        {
            alertTimer?.Kill();
            alertTimer = instance.AddTimer(instance.Config.MAX_DEATH_GROUP_GAP * 0.5f, () => {
                Server.NextFrame(() => {
                    Utils.PrintToAllStaff(instance.Config.pluginTag + instance.Localizer["deathwatch.massDeathEvent", DeathCount, EventId], instance.Config.TAGS_TO_ALERT_OF_MASS_DEATH);
                });
            });
        }
    }
}

public class Utils
{
    public static void PrintToAllStaff(string message, List<string> flags)
    {
        List<CCSPlayerController> allPlayers = Utilities.GetPlayers();

        foreach (var player in allPlayers)
        {
            if (PlayerHasAnyFlag(player, flags))
            {
                player.PrintToChat(message);
            }
        }
    }

    public static bool PlayerHasFlag(CCSPlayerController? player, string flag)
    {
        if (player is null || !player.IsValid || player.IsBot || player.IsHLTV || player.Connected != PlayerConnectedState.PlayerConnected)
        {
            return false;
        }

        AdminData? adminData = AdminManager.GetPlayerAdminData(player);
        bool hasFlag = adminData?.GetAllFlags().Contains(flag) ?? false;

        return hasFlag;
    }

    public static bool PlayerHasAnyFlag(CCSPlayerController? player, List<string> flags)
    {
        bool hasAny = false;

        foreach (var flag in flags)
        {
            if (PlayerHasFlag(player, flag))
            {
                hasAny = true;
                break;
            }
        }

        return hasAny;
    }
}