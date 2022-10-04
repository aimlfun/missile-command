using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Controllers;

/// <summary>
/// Provides the multiplier based on the wave.
/// </summary>
internal static class WaveScore
{
    /// <summary>
    /// Returns the score multiplier of the original 1980's game
    /// </summary>
    /// <param name="wave"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal static int Multiplier(int wave)
    {
        /*
         * Wave	Mult
            1 / 2	1x
            3 / 4	2x
            5 / 6	3x
            7 / 8	4x
            9 / 10	5x
            11+	6x
            [255 / 256]	256x
        */

        if (wave == 255 || wave == 256) return 256; // The rev 2 software has a glitch that causes waves 255 and 256 to multiply their scores by 256. See the code at $5b9e.

        if (wave >= 11) return 6;

        return wave switch
        {
            1 or 2 => 1,
            3 or 4 => 2,
            5 or 6 => 3,
            7 or 8 => 4,
            9 or 10 => 5,
            _ => throw new ArgumentException(nameof(wave)+" must be <0, should be 1+"),
        };
    }
}
