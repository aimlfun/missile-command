using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/// <summary>
/// Missile explosion surface (splodge / filled ellipse).
/// </summary>
internal class Splodge
{
    /// <summary>
    /// Used to make it grow/shrink.
    /// </summary>
    int steps;

    /// <summary>
    /// Size of the filled ellipse
    /// </summary>
    int radius;

    /// <summary>
    /// Where the ellipse is located.
    /// </summary>
    Point Location;

    private void Initialise()
    {
        radius = 30;
        steps = 20;
    }

    /// <summary>
    /// Call to make an explosion appear.
    /// </summary>
    /// <param name="explodeAt"></param>
    internal Splodge(Point explodeAt)
    {
        Location = new Point(explodeAt.X, explodeAt.Y);
        Initialise();
    }

    /// <summary>
    /// Call to make an explosion appear.
    /// </summary>
    /// <param name="explodeAt"></param>
    internal Splodge(PointF explodeAt)
    {
        Location = new Point((int)explodeAt.X, (int)explodeAt.Y);
        Initialise();
    }

    /// <summary>
    /// Explosions are circles that change colour in "Missile Command".
    /// </summary>
    /// <param name="g"></param>
    internal void Draw(Graphics gP1, Color colour)
    {
        --steps;

        int alpha;

        if (steps > 1)
        {
            alpha = 255;
            if (steps % 10 == 0) radius += 45;
            using SolidBrush brush = new(Color.FromArgb(alpha, colour.R, colour.G, colour.B));
            gP1.FillEllipse(brush, Location.X - radius / 2, Location.Y - radius / 2, radius, radius);
        }
        else
        {
            gP1.FillEllipse(Brushes.Black, Location.X - radius / 2, Location.Y - radius / 2, radius, radius);
        }
    }
}
