using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using MissileDefence.UX;
using MissileDefence.Attackers;
using MissileDefence.Attackers.Training;
using MissileDefence.UX.Animations;
using MissileDefence.Defenders;
using MissileDefence.Controllers;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// Determine whether to show city bonus points or not.
    /// </summary>
    private static void CityBonusCheck()
    {
        bool showCityBonus = false;

        // does one of them have cities saved during last wave?

        if (!s_players[0].GameOver && !s_players[0].AllCitiesDestroyed()) showCityBonus = true;

        if (!s_players[1].GameOver && !s_players[1].AllCitiesDestroyed()) showCityBonus = true;

        // they may have cities, via "bonus", but that doesn't count.
        if (showCityBonus) State = PossibleStatesForTheStateMachine.CityBonusPts; else State = PossibleStatesForTheStateMachine.NextWave;
    }
}