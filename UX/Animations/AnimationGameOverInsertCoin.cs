using MissileDefence.Controllers.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/// <summary>
/// Animation that writes "game over  insert coin..."
/// </summary>
internal class AnimationGameOverInsertCoin : Animation
{
    const string scrollingText = "                         GAME OVER       INSERT COINS       1 COIN";

    internal override void Animate(float timeRelativeToStartTime)
    {
        base.Animate(timeRelativeToStartTime);
    }

    long start = 0;
    int scrollingTextX = 0;

    internal AnimationGameOverInsertCoin(float start, float end) : base(start, end)
    {
    }

    internal override void Draw(Graphics g, float timeRelativeToStartTime)
    {
        base.Draw(g, timeRelativeToStartTime);

        if (start == 0)
        {
            start = DateTime.Now.Ticks;
        }

        using Font f2 = new("Lucida Console", 12, FontStyle.Bold);

        g.DrawString(scrollingText, f2, Brushes.Blue, scrollingTextX, 462 - 16);

        scrollingTextX -= 3;
        start = DateTime.Now.Ticks;
    }

    internal override void Finish()
    {
        base.Finish();
    }

    internal override void Initialise()
    {
        base.Initialise();
        GameController.s_showingBases = true;
        scrollingTextX = 0;
    }

    internal override void Reset()
    {
        base.Reset();
    }
}
