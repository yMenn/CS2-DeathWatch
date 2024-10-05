using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace DeathWatch;

public class Draw
{
    private static readonly Vector VectorZero = new(0, 0, 0);
    private static readonly QAngle RotationZero = new(0, 0, 0);
    
    public static List<CBeam> DrawCircle(Vector center, float radius, Color color)
    {
        List<CBeam> laser = [];
        int pointCount = 20;
        List<Vector> points = [];

        for (int i = 0; i < pointCount; i++)
        {
            double angle = i * (2 * Math.PI / pointCount);

            float x = center.X + (float)(radius * Math.Cos(angle));
            float y = center.Y + (float)(radius * Math.Sin(angle));
            float z = center.Z;

            Vector point = new(x, y, z);
            points.Add(point);
        }

        for (int i = 0; i < pointCount; i++)
        {
            Vector start = points[i];
            Vector end = points[(i + 1) % pointCount];

            CBeam? beam = CreateBeamBetweenPoints(start, end, color);
            if (beam is not null)
                laser.Add(beam);
        }
        return laser;
    }

    public static CBeam? CreateBeamBetweenPoints(Vector start, Vector end, Color color)
    {
        CBeam? beam = Utilities.CreateEntityByName<CEnvBeam>("env_beam");

        if (beam == null)
        {
            return null;
        }

        beam.Render = color;
        beam.Width = 3.0f / 2.0f;
        beam.EndWidth = 3.0f / 2.0f;

        beam.Teleport(start, RotationZero, VectorZero);
        beam.EndPos.X = end.X;
        beam.EndPos.Y = end.Y;
        beam.EndPos.Z = end.Z;
        beam.DispatchSpawn();

        return beam;        
    }
}