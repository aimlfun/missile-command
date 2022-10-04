using MissileDefence.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/// <summary>
/// Controls pre/post-game animation
/// </summary>
internal class AnimationController
{
    /// <summary>
    /// 
    /// </summary>
    public delegate void AnimationCompleteCallback();

    /// <summary>
    /// 
    /// </summary>
    private readonly List<Animation> animations = new();

    /// <summary>
    /// 
    /// </summary>
    private float animationTimer = 0;

    /// <summary>
    /// 
    /// </summary>
    float animationTimerLast = -1;

    /// <summary>
    /// 
    /// </summary>
    float nextTimeSlot = 0;

    /// <summary>
    /// 
    /// </summary>
    readonly float timeInterval = 0;

    /// <summary>
    /// 
    /// </summary>
    internal Player attachedPlayer;

    /// <summary>
    /// 
    /// </summary>
    internal event AnimationCompleteCallback? Completed;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tickInterval"></param>
    internal AnimationController(Player player, float tickInterval)
    {
        attachedPlayer = player;

        // tickInterval = 10ms; 
        timeInterval = tickInterval / 500;
        animationTimer = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animation"></param>
    private void AddAnimation(Animation animation)
    {
        nextTimeSlot = Math.Max(animation.EndTime, nextTimeSlot);

        animations.Add(animation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animation"></param>
    /// <param name="duration"></param>
    internal void Add(Animation animation, float duration)
    {
        animation.StartTime = nextTimeSlot;
        animation.Duration = duration;
        animation.EndTime = nextTimeSlot + duration;
        animation.player = attachedPlayer;

        AddAnimation(animation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animation"></param>
    internal void Add(Animation animation)
    {
        animation.Duration = animation.EndTime - animation.StartTime;
        animation.player = attachedPlayer;

        AddAnimation(animation);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="animation"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    internal void Add(Animation animation, int startTime, int endTime)
    {
        animation.StartTime = startTime;
        animation.EndTime = endTime;
        animation.Duration = animation.EndTime - animation.StartTime;
        animation.player = attachedPlayer;

        AddAnimation(animation);
    }

    /// <summary>
    /// 
    /// </summary>
    internal void Reset()
    {
        animationTimer = 0;
        foreach (var animation in animations) animation.Reset();
    }

    /// <summary>
    /// 
    /// </summary>
    internal void Animate()
    {
        if (animationTimer > nextTimeSlot) return;

        foreach (var animation in animations)
        {
            if (animation.EndTime <= animationTimer && animation.EndTime > animationTimerLast) animation.Finish();
        }

        animationTimerLast = animationTimer;
        animationTimer += timeInterval;

        // give animations a chance to initialise
        foreach (var animation in animations)
        {
            if (animation.StartTime <= animationTimer && animation.StartTime > animationTimerLast) animation.Initialise();
        }

        // perform any animations
        foreach (var animation in animations)
        {
            if (animationTimer >= animation.StartTime && animationTimer <= animation.EndTime) animation.Animate(animationTimer - animation.StartTime);
        }

        if (animationTimer > nextTimeSlot) Completed?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="g"></param>
    internal void Draw(Graphics g)
    {
        foreach (var animation in animations)
        {
            if (animationTimer >= animation.StartTime && animationTimer <= animation.EndTime) animation.Draw(g, animationTimer - animation.StartTime);
        }
    }
}
