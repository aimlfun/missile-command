using MissileDefence.Controllers.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/// <summary>
/// Animation that writes "defend cities"
/// </summary>
internal class AnimationDefendCities : Animation
{
    internal override void Animate(float timeRelativeToStartTime)
    {
        base.Animate(timeRelativeToStartTime);
    }

    long start = 0;
    int defendLetters = 0;
    int citiesLetters = 0;

    internal override void Draw(Graphics g, float timeRelativeToStartTime)
    {
        base.Draw(g, timeRelativeToStartTime);

        if (start == 0)
        {
            start = DateTime.Now.Ticks;
        }

        using Font f2 = new("Lucida Console", 13, FontStyle.Bold);
        SizeF size;

        if (defendLetters > 0)
        {
            size = g.MeasureString("DEFEND", f2);

            // city "4" (in the order that MC stores them happens to be the 2nd city left to right).
            g.DrawString("DEFEND"[..defendLetters.Clamp(1, 6)], f2, Brushes.Blue,
                                  2 * GameController.s_players[0].AllInfrastructureBeingDefended[4].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - size.Width / 2, 300);
        }

        if (DateTime.Now.Ticks - start < 10000 * 200)
        {
            ++defendLetters;

            if (defendLetters > 7)
            {
                ++citiesLetters;
            }

        }

        start = DateTime.Now.Ticks;

        if (citiesLetters > 0)
        {
            size = g.MeasureString("CITIES", f2);
            // city "1" (in the order that MC stores them happens to be the 5th city left to right, middle of the 3 right cities)
            g.DrawString("CITIES"[..citiesLetters.Clamp(1, 6)], f2, Brushes.Blue,
                                  2 * GameController.s_players[0].AllInfrastructureBeingDefended[1].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - size.Width / 2, 300);
        }
    }

    internal override void Finish()
    {
        base.Finish();
    }

    internal override void Initialise()
    {
        base.Initialise();
        GameController.s_showingBases = true;
        start = 0;
    }

    internal override void Reset()
    {
        base.Reset();
    }
}
