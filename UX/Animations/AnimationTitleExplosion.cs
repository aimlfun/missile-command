using MissileDefence.Controllers;
using MissileDefence.UX.Explosions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX.Animations;

/*
 ; ORIGINAL:
 ; Func $16: draws title screen explosion sequence.
 ; 
 ; Draws and erases 20 explosions, sized and placed randomly.
 ; 
 ; This uses the 16-bit ICBM movement counter ($b2-b3) as a pair of counters,
 ; initialized by func $14 (above).
 ;
 ; IMPLEMENTATION:
 ; Draws and erases 20 explosions, sized and placed randomly.   
 */

internal class AnimationTitleExplosion : Animation
{
    private readonly List<Splodge> explosionInUI = new();

    int logoIndex = 1;

    internal override void Animate(float timeRelativeToStart)
    {
        // show lots of splodges in different colours eating away the missile command text for about 3 seconds
        int splodges = RandomNumberGenerator.GetInt32(1, 4);

        if (explosionInUI.Count < 25)
        {
            for (int i = 0; i < splodges; i++)
            {
                Point point = new(RandomNumberGenerator.GetInt32(0, 511),
                                  RandomNumberGenerator.GetInt32(56 * 2, 208 * 2));
                Splodge exp = new(point);
                explosionInUI.Add(exp);
            }
        }
    }

    int colorIndex = 0;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="g"></param>
    /// <param name="timer"></param>
    internal override void Draw(Graphics g, float timer)
    {
        base.Draw(g, timer);

        Bitmap logo = new(SharedUX.s_missileCommandLogos[logoIndex++]);

        if (logoIndex >= SharedUX.s_missileCommandLogos.Length) logoIndex = 0;

        g.DrawImage(logo, 0, 231 - 244 / 2, 512, 244);

        Color colour = ExplosionManager.ColorsToCycleExplosion[colorIndex++];
        colorIndex %= ExplosionManager.ColorsToCycleExplosion.Length;

        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;

        foreach (Splodge x in explosionInUI)
        {
            x.Draw(g,colour);
        }
    }

    internal override void Finish()
    {
        base.Finish();
        explosionInUI.Clear();
    }

    internal override void Initialise()
    {
        base.Initialise();
        explosionInUI.Clear();
        logoIndex = 1;
    }

    internal override void Reset()
    {
        base.Reset();
    }
}
