using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace DeathWatch;
public partial class DeathWatch
{
    [ConsoleCommand("css_deathwatch")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnDeathWatchCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        HandleDeathWatchCommand(info);
    }

    [ConsoleCommand("css_eventinfo")]
    [CommandHelper(minArgs: 1, usage: "<event_id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnEventInfoCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (!int.TryParse(info.GetArg(1), out int eventId))
        {
            info.ReplyToCommand($"{Config.pluginTag} "+ Localizer["deathwatch.invalidEventId"]);
            return;
        }
        HandleEventInfoCommand(info, eventId);
    }

    [ConsoleCommand("css_listevents")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnListEventsCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;
            
        Logger.LogInformation("[DeathWatch] Command 'css_listevents' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        if (deathEvents.Count == 0)
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noDeathEventsThisRound"]);
            return;
        }

        info.ReplyToCommand($"{Config.pluginTag} Recent death events:");
        foreach (var evt in deathEvents.OrderByDescending(e => e.EventId))
        {
            info.ReplyToCommand($"Event ID: {ChatColors.Green}{evt.EventId}{ChatColors.White}, Deaths: {ChatColors.Red}{evt.Deaths.Count}{ChatColors.White}, Time: {evt.Start}");
        }
    }

    [ConsoleCommand("css_erhere")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 0, usage: "<event_id>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnEventRespawnHereCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        Logger.LogInformation("[DeathWatch] Command 'css_erhere' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        RespawnEventPlayers(caller, info, RespawnLocation.CurrentPosition);
    }

    [ConsoleCommand("css_er")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 0, usage: "<event_id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnEventRespawnLastCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        Logger.LogInformation("[DeathWatch] Command 'css_er' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        RespawnEventPlayers(caller, info, RespawnLocation.Spawn);
    }

    [ConsoleCommand("css_erspot")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 0, usage: "<event_id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnEventRespawnSpotCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        Logger.LogInformation("[DeathWatch] Command 'css_erspot' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        RespawnEventPlayers(caller, info, RespawnLocation.DeathPosition);
    }

    [ConsoleCommand("css_srhere")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, usage: "<respawn_count>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnStackRespawnHereCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;
            
        Logger.LogInformation("[DeathWatch] Command 'css_srhere' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        if (caller == null)
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathWatch.invalidCaller"]);
            return;
        }

        RespawnDeadPlayer(caller, info, RespawnLocation.CurrentPosition);
    }

    [ConsoleCommand("css_sr")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, usage: "<respawn_count>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnStackRespawnLastCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        Logger.LogInformation("[DeathWatch] Command 'css_sr' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        RespawnDeadPlayer(caller, info, RespawnLocation.Spawn);
    }

    [ConsoleCommand("css_srspot")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, usage: "<respawn_count>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnStackRespawnSpotCommand(CCSPlayerController? caller, CommandInfo info)
    {
        if (caller is null || !caller.IsValid || caller.IsBot || caller.IsHLTV)
            return;

        Logger.LogInformation("[DeathWatch] Command 'css_srspot' executed by {Caller}.", caller?.PlayerName ?? "Server Console");

        RespawnDeadPlayer(caller, info, RespawnLocation.DeathPosition);
    }

    public void RespawnDeadPlayer(CCSPlayerController? caller, CommandInfo info, RespawnLocation location)
    {
        Logger.LogInformation("[DeathWatch] RespawnDeadPlayer called by {Caller}, Location: {Location}", caller?.PlayerName ?? "Server Console", location);

        string? numArg = info.GetArg(1);

        if (numArg is not null && int.TryParse(numArg, out int playersToRespawn))
        {
            if (playersToRespawn <= 0)
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.invalidRespawnCount", playersToRespawn]);
                return;
            }

            if (deathStack.Count == 0)
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.emptyDeathStack"]);
                return;
            }

            var deathInfos = deathStack.GetLastN(playersToRespawn);
            int respawnedCount = 0;

            foreach (var deathInfo in deathInfos)
            {
                if (deathInfo.TryGetPlayer(out var player) && player is not null && player.IsValid && player?.PlayerPawn is not null && !player.PawnIsAlive)
                {
                    Logger.LogInformation("[DeathWatch] Respawning player {PlayerName} at {Location}", player.PlayerName, location);

                    player.Respawn();
                    ClearDeathMarker(player);

                    if (player.PlayerPawn.Value is null)
                    {
                        deathStack.Remove(player);
                        continue;
                    }
                    if (location == RespawnLocation.CurrentPosition && caller is not null)
                    {  
                        var callerPosition = caller.PlayerPawn?.Value?.AbsOrigin;
                        var callerAngle = caller.PlayerPawn?.Value?.AbsRotation;
                        var velocity = new Vector(0, 0, 0);

                        player.PlayerPawn.Value.Teleport(callerPosition ?? deathInfo.Position, callerAngle ?? new QAngle(0, 0, 0), velocity);

                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayer", player.PlayerName]);
                    }
                    else if (location == RespawnLocation.DeathPosition && deathInfo.Position is not null)
                    {
                        var angles = deathInfo.PlayerAngle ?? new QAngle(0, 0, 0);
                        player.PlayerPawn.Value.Teleport(deathInfo.Position, angles, new Vector(0, 0, 0));
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayerDeathLoc", player.PlayerName]);
                    }
                    else if (location == RespawnLocation.DeathPosition)
                    {
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.noDeathLocationFound", player.PlayerName]);
                    }
                    else
                    {
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayerAtSpawn", player.PlayerName]);
                    }

                    deathStack.Remove(player);
                    respawnedCount++;
                }
                else
                {
                    if (deathInfo.TryGetPlayer(out var invalidPlayer))
                    {
                        Logger.LogWarning("[DeathWatch] Cannot respawn player {PlayerName}: invalid or already alive.", invalidPlayer?.PlayerName ?? "Invalid Player");
                    }
                    else
                    {
                        Logger.LogWarning("[DeathWatch] Cannot respawn player: player reference is no longer valid.");
                    }
                }
            }

            if (respawnedCount == 0)
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noValidPlayersToRespawn"]);
            }
            else
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.respawnedXPlayers", respawnedCount]);
            }
        }
        else
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.invalidRespawnNumber", numArg ?? ""]);
        }
    }

    public void RespawnEventPlayers(CCSPlayerController? caller, CommandInfo info, RespawnLocation location)
    {
        Logger.LogInformation("[DeathWatch] RespawnEventPlayers called by {Caller}, Location: {Location}", caller?.PlayerName ?? "Server Console", location);

        DeathEvent? deathEvent;
        int eventId;

        string? eventIdArg = info.GetArg(1);

        if (string.IsNullOrEmpty(eventIdArg))
        {
            deathEvent = deathEvents.LastOrDefault();
            if (deathEvent == null)
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noDeathEventsThisRound"]);
                return;
            }
            eventId = deathEvent.EventId;
        }
        else if (!int.TryParse(eventIdArg, out eventId))
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.invalidEventId"]);
            return;
        }
        else
        {
            deathEvent = deathEvents.FirstOrDefault(e => e.EventId == eventId);
            if (deathEvent == null)
            {
                info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noEventFound", eventId]);
                return;
            }
        }

        int respawnedCount = 0;

        foreach (var deathInfo in deathEvent.Deaths)
        {
            if (deathInfo.TryGetPlayer(out var player) && player is not null && player.IsValid && player.PlayerPawn != null && player.PlayerPawn.IsValid && !player.PawnIsAlive)
            {
                Logger.LogInformation("[DeathWatch] Respawning player {PlayerName} from event {EventId} at {Location}", player.PlayerName, eventId, location);

                player.Respawn();
                ClearDeathMarker(player);

                if (player.PlayerPawn.Value == null || !player.PlayerPawn.Value.IsValid)
                {
                    continue;
                }

                switch (location)
                {
                    case RespawnLocation.CurrentPosition when caller != null && caller.IsValid && caller.PlayerPawn != null && caller.PlayerPawn.IsValid:
                        var callerPosition = caller.PlayerPawn.Value?.AbsOrigin;
                        var callerAngle = caller.PlayerPawn.Value?.AbsRotation;
                        player.PlayerPawn.Value.Teleport(callerPosition ?? deathInfo.Position, callerAngle ?? deathInfo.PlayerAngle, new Vector(0, 0, 0));
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayerAtStaff", player.PlayerName]);
                        break;
                    case RespawnLocation.DeathPosition when deathInfo.Position != null:
                        var angles = deathInfo.PlayerAngle ?? new QAngle(0, 0, 0);
                        player.PlayerPawn.Value.Teleport(deathInfo.Position, angles, new Vector(0, 0, 0));
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayerDeathLoc", player.PlayerName]);
                        break;
                    default:
                        Server.PrintToChatAll($"{Config.pluginTag} " + Localizer["deathwatch.respawningPlayerAtSpawn", player.PlayerName]);
                        break;
                }

                respawnedCount++;
            }
            else
            {
                if (deathInfo.TryGetPlayer(out var invalidPlayer))
                {
                    Logger.LogWarning("[DeathWatch] Cannot respawn player {PlayerName}: invalid or already alive.", invalidPlayer?.PlayerName ?? "Invalid Player");
                }
                else
                {
                    Logger.LogWarning("[DeathWatch] Cannot respawn player: player reference is no longer valid.");
                }
            }
        }

        if (respawnedCount == 0)
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noValidPlayersToRespawn"]);
        }
        else
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.respawnedXPlayersFromEvent", respawnedCount, eventId]);
        }
    }

    public void ClearDeathMarker(CCSPlayerController? player)
    {
        if (player is null || !deathLaser.TryGetValue(player, out var laser)) 
            return;

        Logger.LogInformation("[DeathWatch] Clearing death marker for player {PlayerName}.", player.PlayerName);

        foreach(var beam in laser)
        {
            if (beam is null)
                continue;
            
            beam.AcceptInput("Kill");
        }

        deathLaser.Remove(player);
    }

    public void HandleDeathWatchCommand(CommandInfo info)
    {
        info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.availableCommands"]);
        info.ReplyToCommand($" {ChatColors.Green}css_deathwatch{ChatColors.White} - " + Localizer["deathwatch.commandHelp"]);
        info.ReplyToCommand($" {ChatColors.Green}css_eventinfo <event_id>{ChatColors.White} - " + Localizer["deathwatch.commandEventInfo"]);
        info.ReplyToCommand($" {ChatColors.Green}css_listevents{ChatColors.White} - " + Localizer["deathwatch.commandListEvents"]);
        info.ReplyToCommand($" {ChatColors.Green}css_er <event_id>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnLast"]);
        info.ReplyToCommand($" {ChatColors.Green}css_erhere <event_id>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnHere"]);
        info.ReplyToCommand($" {ChatColors.Green}css_erspot <event_id>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnSpot"]);
        info.ReplyToCommand($" {ChatColors.Green}css_sr <respawn_count>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnStack"]);
        info.ReplyToCommand($" {ChatColors.Green}css_srhere <respawn_count>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnHereStack"]);
        info.ReplyToCommand($" {ChatColors.Green}css_srspot <respawn_count>{ChatColors.White} - " + Localizer["deathwatch.commandRespawnSpotStack"]);
    }

    public void HandleEventInfoCommand(CommandInfo? info, int eventId)
    {
        if (info is null) return;

        var deathEvent = deathEvents.FirstOrDefault(e => e.EventId == eventId);
        if (deathEvent == null)
        {
            info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.noEventFound", eventId]);
            return;
        }
        
        info.ReplyToCommand($"{Config.pluginTag} " + Localizer["deathwatch.playersInEvent", eventId]);

        foreach (var deathInfo in deathEvent.Deaths)
        {
            if (deathInfo.TryGetPlayer(out var deadPlayer))
            {
                info.ReplyToCommand($"- {ChatColors.Yellow}{deadPlayer?.PlayerName ?? "Unknown Player" }{ChatColors.White}");
            }
            else
            {
                info.ReplyToCommand("- " + Localizer["deathwatch.unknownPlayer"]);
            }
        }
    }

    public enum RespawnLocation
    {
        Spawn,
        DeathPosition,
        CurrentPosition
    }
}
