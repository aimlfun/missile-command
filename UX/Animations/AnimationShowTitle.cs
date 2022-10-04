using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers;

namespace MissileDefence.UX.Animations;

/*
 ; ORIGINAL:
 ; Func $14: draws the "MISSILE COMMAND" title.
 ; 
 ; The words are drawn while the palette is entirely blacked out, so that instead
 ; of visibily rendering they just pop onto the screen.
 ; 
 ; IMPLEMENTATION:
 ; Show the pre-prepared red image in one go, and wait.
 */
internal class AnimationShowTitle : Animation
{
    internal override void Draw(Graphics g, float timerRelativeToStartTime)
    {
        base.Draw(g, timerRelativeToStartTime);

        if (drawn) return;

        g.DrawImage(SharedUX.s_missileCommandLogo, 0, 231 - 244 / 2, 512, 244);

        drawn = true;
    }

    internal override void Finish()
    {
        base.Finish();
    }

    internal override void Initialise()
    {
        base.Initialise();
    }

    internal override void Reset()
    {
        base.Reset();
    }
}
