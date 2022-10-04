using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers;

namespace MissileDefence.UX.Animations;

internal class Animation
{
    /// <summary>
    /// Time animation starts
    /// </summary>
    internal float StartTime;

    /// <summary>
    /// Time animation ends;
    /// </summary>
    internal float EndTime;

    /// <summary>
    /// 
    /// </summary>
    internal float Duration;

    /// <summary>
    /// 
    /// </summary>
    internal Player? player;

    /// <summary>
    /// 
    /// </summary>
    protected bool drawn = false;

    /// <summary>
    /// 
    /// </summary>
    protected bool initialised = false;

    /// <summary>
    /// Called to ensure state is reset at the start of an animation loop
    /// </summary>
    internal virtual void Reset()
    {
        // override as required
        initialised = false;
        drawn = false;
    }

    /// <summary>
    /// Set up anything; called prior to "Draw".
    /// </summary>
    internal virtual void Initialise()
    {
        if (initialised) return; // don't do twice
        drawn = false;
    }

    internal virtual void Animate(float timeRelativeToStart)
    {

    }

    /// <summary>
    /// Called to draw during duration.
    /// </summary>
    internal virtual void Draw(Graphics g, float timeRelativeToStart)
    {
    }

    /// <summary>
    /// Called when end time is reached.
    /// </summary>
    internal virtual void Finish()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    internal Animation()
    {
        StartTime = 0;
        EndTime = int.MaxValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    internal Animation(float start, float end)
    {
        StartTime = start;
        EndTime = end;
    }
}
