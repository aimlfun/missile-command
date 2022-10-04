using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers.Game;

namespace MissileDefence.UX.Animations;

internal class AnimationRedArrows : FlashingAnimation
{
    internal AnimationRedArrows(float start, float end, float frequency) : base(start, end, frequency)
    {
    }

    internal override void Animate(float timeRelativeToStart)
    {
        base.Animate(timeRelativeToStart);
    }

    internal override void Draw(Graphics g, float timer)
    {
        base.Draw(g, timer);

        if (!IsDisplaying) return;

        int y = 360; // px out of 231x2 (462)

        Bitmap arrow = new(SharedUX.s_arrow);

        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[0].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);
        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[1].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);
        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[2].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);

        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[3].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);
        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[4].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);
        g.DrawImage(arrow, 2 * GameController.s_players[0].AllInfrastructureBeingDefended[5].LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX - arrow.Width / 2, y);
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
