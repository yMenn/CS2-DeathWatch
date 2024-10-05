using System.Drawing;
using System.Formats.Tar;
using System.Linq.Expressions;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace DeathWatch;

public partial class DeathWatch
{
    public CounterStrikeSharp.API.Modules.Timers.Timer? activeTimer;

    public DeathStack deathStack = new();
    public Dictionary<CCSPlayerController, List<CBeam>> deathLaser = [];
    public List<DeathEvent> deathEvents = [];

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        CleanUp();

        return HookResult.Handled;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (@event.Userid?.PlayerPawn?.Value is null || @event.Userid?.PlayerPawn?.Value?.AbsOrigin is null || !@event.Userid.PawnIsAlive)
            return HookResult.Continue;
        
        Vector playerPos = @event.Userid.PlayerPawn.Value.AbsOrigin + new Vector(0, 0, 2);
        QAngle? playerAng = @event.Userid.PlayerPawn.Value.AbsRotation;

        if (playerPos is not null)
        {
            DateTime currentTime = DateTime.Now;

            if (Config.PLACE_MARKER_ON_DEATH)
            {
                var modelRadius = 10.0f;

                List<CBeam> laser = Draw.DrawCircle(playerPos, modelRadius, @event.Userid.Team == CsTeam.CounterTerrorist ? Color.RoyalBlue : Color.OrangeRed);
                deathLaser[@event.Userid] = laser;
            }

            DeathInfo deathInfo = new(@event.Userid, playerPos, playerAng ?? new QAngle(0, 0, 0), -1, DateTime.Now);

            deathStack.Push(deathInfo);

            if (deathEvents.Count > 0)
            {
                var latestDeathEvent = deathEvents[^1];
                var timeSinceLastDeath = currentTime - latestDeathEvent.GetLastDeathTime();

                if (timeSinceLastDeath.TotalSeconds > Config.MAX_DEATH_GROUP_GAP)
                {
                    var deathEvent = new DeathEvent(currentTime, deathEvents.Count, Config.DEATH_ALERT_COUNT);
                    deathInfo.EventId = deathEvents.Count;
                    deathEvent.AddDeath(deathInfo, this);
                    deathEvents.Add(deathEvent);
                }
                else
                {
                    deathInfo.EventId = deathEvents.Count-1;
                    latestDeathEvent.AddDeath(deathInfo, this);
                }
            }
            else
            {
                var deathEvent = new DeathEvent(currentTime, 0, Config.DEATH_ALERT_COUNT);
                deathInfo.EventId = 0;
                deathEvent.AddDeath(deathInfo, this);
                deathEvents.Add(deathEvent);
            }
        }

        return HookResult.Continue;
    }

    public void CleanUp()
    {
        try
        {
            foreach (var laserList in deathLaser.Values)
            {
                foreach (var beam in laserList)
                {
                    if (beam is null || !beam.IsValid) continue;
                    beam?.AcceptInput("Kill");
                }
            }
            deathStack.Clear();
            deathLaser.Clear();
            deathEvents.Clear();
            activeTimer?.Kill();

            Logger.LogInformation("[DeathWatch] Performing cleanup.");
        }
        catch(Exception ex)
        {
            Logger.LogWarning("[DeathWatch] Error occured during cleanup. Message:{Error}", ex);
        }
    }
} 