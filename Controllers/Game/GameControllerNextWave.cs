using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using MissileDefence.Attackers;
using MissileDefence.Attackers.Training;
using MissileDefence.UX.Animations;
using MissileDefence.Defenders;
using MissileDefence.UX;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{

    /// <summary>
    /// We've given the bonuses etc, now we need to reset our ABMs, and provide fresh missiles.
    /// Bonus cities will have already been awarded.
    /// </summary>
    private static void NextWave()
    {
        // When you finish a wave, you collect bonus points, and your launcher silos are repaired and re-armed.
        // Bonus cities are deployed. As you complete each wave, the attacks become faster and more numerous, and the colors change.

        s_players[0].EndOfWaveReset();
        s_players[1].EndOfWaveReset();

        ++s_playerWave;
        ++s_UXWaveTheme;

        if (s_UXWaveTheme > SharedUX.s_uxColorsPerLevel.Count - 1) s_UXWaveTheme = 1;

        State = PossibleStatesForTheStateMachine.PrepWave;
    }
   
}