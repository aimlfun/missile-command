using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Explosions;

public class ExplosionManager
{
    internal static readonly Color[] ColorsToCycleExplosion = new Color[] {
                                   Color.Yellow,
                                   Color.Red,
                                   Color.Blue,
                                   Color.LimeGreen,
                                   Color.Magenta,
                                   Color.Cyan,
                                   Color.White };

    /// <summary>
    /// 
    /// </summary>
    private readonly List<SurfaceToAirMissileExplosion> explosionInUI = new();

    /// <summary>
    /// Tracks an explosion, and makes it appear.
    /// </summary>
    /// <param name="location"></param>
    public void Add(Point location)
    {
        explosionInUI.Add(new SurfaceToAirMissileExplosion(location));
    }

    /// <summary>
    /// Tracks an explosion, and makes it appear.
    /// </summary>
    /// <param name="location"></param>
    public void Add(PointA location)
    {
        PointF deviceLocation = location.MCCoordsToDeviceCoordinates();
        explosionInUI.Add(new SurfaceToAirMissileExplosion(deviceLocation));
    }

    /// <summary>
    /// Removes all explosions e.g. end of round.
    /// </summary>
    public void Clear()
    {
        explosionInUI.Clear();
    }

    static int colorIndex = 0;

    /// <summary>
    /// Draws all explosions to the graphics canvas provided.
    /// </summary>
    /// <param name="g"></param>
    public void Draw(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.HighSpeed; // chunky like the original

        List<SurfaceToAirMissileExplosion> listToRemove = new();

        Color colour = ColorsToCycleExplosion[colorIndex++];
        colorIndex %= ColorsToCycleExplosion.Length;

        foreach (SurfaceToAirMissileExplosion exp in explosionInUI)
        {
            if (exp.Draw(g, colour)) listToRemove.Add(exp);
        }

        // remove those that have fizzled out.
        foreach (SurfaceToAirMissileExplosion exp in listToRemove) explosionInUI.Remove(exp);
    }
}
