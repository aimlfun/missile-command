using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/// <summary>
/// Cross comes horizontally across screen beneath "defend" (right 2 left, diagonally from center).
/// </summary>
internal class AnimationMoveCursor : Animation
{
    /// <summary>
    /// These define where the target cross-hair needs to go to.
    /// </summary>
    readonly Point[] wayPoints = new Point[5] { new Point(256,231),
                                                new Point(103*2,167*2),
                                                new Point(5*2,167*2),
                                                new Point(256,144),
                                                new Point(256,231)};

    /// <summary>
    /// 
    /// </summary>
    int target = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeRelativeToStart"></param>
    internal override void Animate(float timeRelativeToStart)
    {
        base.Animate(timeRelativeToStart);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="g"></param>
    /// <param name="timeRelativeToStart"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal override void Draw(Graphics g, float timeRelativeToStart)
    {
        base.Draw(g, timeRelativeToStart);

        if (target >= wayPoints.Length)
        {
            return;
        }

        if (player is null) throw new Exception(nameof(player));

        // if near the destination, move to the next target
        if (Math.Abs(player.CrossHairLocation.X - wayPoints[target].X) < 3 &&
            Math.Abs(player.CrossHairLocation.Y - wayPoints[target].Y) < 3)
        {
            ++target;

            if (target < wayPoints.Length) player.TargetHairLocation = wayPoints[target];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void Finish()
    {
        base.Finish();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    internal override void Initialise()
    {
        base.Initialise();

        if (player is null) throw new ArgumentNullException(nameof(player));

        player.CrossHairLocation = new Point(128 * 2, 165 * 2);
        player.TargetHairLocation = wayPoints[0];

        player.CrossHairVisible = true;
    }

    /// <summary>
    /// 
    /// </summary>
    internal override void Reset()
    {
        base.Reset();
    }
}
