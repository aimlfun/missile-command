using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

internal class FlashingAnimation : Animation
{
    readonly float Frequency = 10;
    long start = 0;

    protected bool IsDisplaying = false;

    internal FlashingAnimation(float frequency)
    {
        Frequency = frequency;
        IsDisplaying = false;
    }

    internal FlashingAnimation(float start, float end, float frequency) : base(start, end)
    {
        Frequency = frequency;
        IsDisplaying = false;
    }

    internal override void Draw(Graphics g, float timer)
    {

        if (start == 0)
        {
            start = DateTime.Now.Ticks;
        }

        if (DateTime.Now.Ticks - start > 10000 * Frequency * 1000)
        {
            start = DateTime.Now.Ticks;
            IsDisplaying = !IsDisplaying;
        }
    }

    internal override void Finish()
    {
        base.Finish();
    }

    internal override void Initialise()
    {
        base.Initialise();
        IsDisplaying = false;
    }

    internal override void Reset()
    {
        base.Reset();
        IsDisplaying = false;
    }
}
