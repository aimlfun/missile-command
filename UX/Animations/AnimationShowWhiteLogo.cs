using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers;

namespace MissileDefence.UX.Animations;

internal class AnimationShowWhiteLogo : Animation
{
    internal override void Draw(Graphics g, float timerRelativeToStartTime)
    {
        base.Draw(g, timerRelativeToStartTime);

        Bitmap logo = new(SharedUX.s_missileCommandLogos[0]);


        g.DrawImage(logo, 0, 231 - 244 / 2, 512, 244);
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
