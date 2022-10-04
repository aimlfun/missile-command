using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX;

/// <summary>
/// Moves from start point to target point in incremental steps when NextPoint() is called.
/// It uses Bresenham's integer line algorithm to achieve this.
/// https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
/// </summary>
internal class BresenhamLine
{
    /// <summary>
    /// 
    /// </summary>
    internal Point Location;

    /// <summary>
    /// 
    /// </summary>
    private Point TargetLocation;

    /// <summary>
    /// 
    /// </summary>
    readonly private int deltaX;

    /// <summary>
    /// 
    /// </summary>
    readonly private int deltaY;

    /// <summary>
    /// 
    /// </summary>
    readonly private int signX;

    /// <summary>
    /// 
    /// </summary>
    readonly private int signY;

    /// <summary>
    /// 
    /// </summary>
    private int error;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="StartPoint"></param>
    /// <param name="EndPoint"></param>
    internal BresenhamLine(Point StartPoint, Point EndPoint)
    {
        Location = new Point(StartPoint.X, StartPoint.Y);
        TargetLocation = new Point(EndPoint.X, EndPoint.Y);

        // Bresenham's line algorithm to target
        deltaX = Math.Abs(TargetLocation.X - Location.X);
        deltaY = Math.Abs(TargetLocation.Y - Location.Y);

        signX = Location.X < TargetLocation.X ? 1 : -1;
        signY = Location.Y < TargetLocation.Y ? 1 : -1;

        error = deltaX - deltaY;
    }

    /// <summary>
    /// Moves current location using Bresenham's algorithm (unless has already arrived).
    /// </summary>
    /// <param name="Location">Current location.</param>
    /// <returns>true - at the destination.</returns>
    internal bool NextPoint(out Point CurrentLocation)
    {
        // don't add to it, if we're at our destianation
        if (Location.X != TargetLocation.X || Location.Y != TargetLocation.Y)
        {
            var e2 = 2 * error;

            if (e2 > -deltaY)
            {
                error -= deltaY;
                Location.X += signX;
            }

            if (e2 < deltaX)
            {
                error += deltaX;
                Location.Y += signY;
            }
        }

        // return where we are currently at
        CurrentLocation = new Point(Location.X, Location.Y);

        // true if we reach the target
        return Location.X == TargetLocation.X && Location.Y == TargetLocation.Y;
    }
}
