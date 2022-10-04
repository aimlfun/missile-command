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

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// CPU and USER have both finished their wave.
    /// We need to now see what bonuses to give.
    /// </summary>
    private void EndOfWave()
    {
        /*
         ; Func $08: end wave.
        */

        // whilst showing bonus, we turn off the cursor
        s_players[1].CrossHairVisible = false;

        // having missiles not killed will cause issues.
        ((PlayerCPU)s_players[0]).ClearICBMs();
        ((PlayerUser)s_players[1]).ResetMissiles();


        // remove on-screen explosions
        s_players[0].KillExplosions();
        s_players[1].KillExplosions();

        /*
          ORIGINAL:

          Bonus City Award Calculation

          The code that awards bonus cities doesn't increment the count as you play. Instead, at the end of each wave, it
          figures out the total number of bonus cities your score represents, and then subtracts the number of bonus cities 
          awarded in the past. For example, if your score is 805,000, and bonus cities are awarded every 10,000 points, 
          it would conclude that you have earned 80 bonus cities.
        */

        // we add these BEFORE game-over, as the user MAY have acquired a city before losing all their cities, and
        // they can rebuild.
        AddBonusCities(0);
        AddBonusCities(1);

        // if they have no cities left, and won't get any rebuilt via the "bonus" ones, their game has ended.

        if (s_players[0].AllCitiesIncludingBonusDestroyed()) s_players[0].GameOver = true;

        if (s_players[1].AllCitiesIncludingBonusDestroyed()) s_players[1].GameOver = true;

        /*
         ; 
         ; Transitions to the ABM bonus tally, city bonus tally, or game-over check.
        */

        // if both players have lost, then it's game-over for both.
        if (s_players[0].GameOver && s_players[1].GameOver && !s_players[0].InGameOverAnimation && !s_players[1].InGameOverAnimation)
        {
            State = PossibleStatesForTheStateMachine.GameOver;
            return;
        }

        bonusPointsState = BonusPointsStates.init;
        State = PossibleStatesForTheStateMachine.AbmBonusPts;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    private static void AddBonusCities(int player)
    {
        // game has ended for player, sorry no awards for dying!
        if (s_players[player].GameOver)
        {
            s_players[player].BonusCityAward = 0;
            return;
        }

        //if (player == 0) return; // no bonus cities for the AI, otherwise it'll never lose

        // calculation per original
        int bonusCities = s_players[player].score / 10000; // pts

        s_players[player].BonusCityAward = bonusCities - s_players[player].BonusCitiesAwardedInThePast;

        if(player!=0) s_players[player].BonusCitiesAvailableForRebuilding += s_players[player].BonusCityAward;
        s_players[player].BonusCitiesAwardedInThePast = bonusCities;
    }
}