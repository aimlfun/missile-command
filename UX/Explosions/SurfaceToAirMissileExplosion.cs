using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Explosions;

/// <summary>
/// Missile explosion surface.
/// </summary>
internal class SurfaceToAirMissileExplosion
{
    int alpha = 230;

    /// <summary>
    /// 
    /// </summary>
    int radius;

    /// <summary>
    /// 
    /// </summary>
    Point Location;

    int increaseSteps = 15;
    int pauseSteps = 20;
    int decreaseSteps = 15;

    /// <summary>
    /// 
    /// </summary>
    private void Initialise()
    {
        increaseSteps = 23;
        pauseSteps = 5;
        decreaseSteps = 23;
    }

    /// <summary>
    /// Call to make an explosion appear.
    /// </summary>
    /// <param name="explodeAt"></param>
    internal SurfaceToAirMissileExplosion(Point explodeAt)
    {
        Location = new Point(explodeAt.X, explodeAt.Y);
        Initialise();
    }

    /// <summary>
    /// Call to make an explosion appear.
    /// </summary>
    /// <param name="explodeAt"></param>
    internal SurfaceToAirMissileExplosion(PointF explodeAt)
    {
        Location = new Point((int)explodeAt.X, (int)explodeAt.Y);
        Initialise();
    }

    /// <summary>
    /// Explosions are circles that change colour in "Missile Command".
    /// </summary>
    /// <param name="g"></param>
    internal bool Draw(Graphics g, Color colour)
    {        
        if (--increaseSteps > 0)
        {
            radius += 2;
        }
        else if (--pauseSteps > 0)
        {
            return false;
        }
        else if (--decreaseSteps > 0)
        {
            alpha -=0;
            radius -= 2;
        }

        // random colour
        using SolidBrush brush = new(Color.FromArgb(alpha, colour.R, colour.G, colour.B));
        g.FillEllipse(brush, Location.X - radius / 2, Location.Y - radius / 2, radius, radius);

        return decreaseSteps <= 0; // remove, we're done with this explosion when it reaches 0
    }

}
