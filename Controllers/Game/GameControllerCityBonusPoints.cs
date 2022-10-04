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
    /// Add up the cities that are not destroyed. We don't do the "BONUS CITY" text when adding a bonus city.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    private void CityBonusPoints()
    {
        /*
         ; In addition, at the end of each wave the game awards 5 points for un-fired ABMs, and 100 points for each surviving city. 
         ;
         ; All points are multiplied by the current scoring multiplier, which starts at 1x and increases every other level until it reaches 6x at wave 11.
         ; Bonus cities are awarded every N points, where N is set with physical DIP switches in the cabinet (default 10,000). 
         ; When a city is destroyed, it's replaced with a bonus city if any are available.
         ;
         ; You can have a large number of bonus cities pending.
        */

        if (s_currentICBMWaveCPU is null) throw new ArgumentNullException(nameof(s_currentICBMWaveCPU) + " should not be null");
        if (s_currentICBMWaveUSER is null) throw new ArgumentNullException(nameof(s_currentICBMWaveUSER) + " should not be null");

        // first time we arrive at "ABMBonusPoints()" we initialise a few things
        if (bonusPointsState == BonusPointsStates.init)
        {
            cpuCitiesRemaining = s_players[0].CountOfRemainingVisibleCitiesExcludingBonus;
            userCitiesReamining = s_players[1].CountOfRemainingVisibleCitiesExcludingBonus;
            s_framesRendered = 0;
            bonusPointsState = BonusPointsStates.counting;
        }

        bool readyForNextWave = cpuCitiesRemaining + userCitiesReamining == 0;
        /*
         ; All city bonuses have been awarded; move on.
            5cfa: a9 10                        lda     #FN_WAVE_DONE_UPD ;do the end-of-wave housekeeping func...
            5cfc: 85 92                        sta     next_func_index
            5cfe: a9 22                        lda     #FN_PAUSE         ;...after we delay for a bit
            5d00: 85 91                        sta     func_index
            5d02: a9 0f                        lda     #15               ;pause for 15*4 frames (about 1 sec)
            5d04: 85 af                        sta     frame4_delay_ctr
        */

        if (++s_framesRendered % (4 * (readyForNextWave ? 15 : 1)) != 0) return; // count slowly, no need to paint in between

        // we're removing ABMs as we count, eventually both will reach zero and we need to do bonus city check.
        if (readyForNextWave)
        {
            State = PossibleStatesForTheStateMachine.NextWave;
            return;
        }

        // CPU PLAYER
        if (cpuCitiesRemaining > 0)
        {
            CountCitiesRemainingForPlayerAndShowBonusForPlayer(0);
            cpuCitiesRemaining--;
        }

        // USER PLAYER
        if (userCitiesReamining > 0)
        {
            CountCitiesRemainingForPlayerAndShowBonusForPlayer(1);
            userCitiesReamining--;
        }

    }

    /// <summary>
    /// Counts up the remaining cities, showing a tally of cities.
    /// </summary>
    /// <param name="player"></param>
    private static void CountCitiesRemainingForPlayerAndShowBonusForPlayer(int player)
    {
        Bitmap displayPlayer1 = (Bitmap)s_players[player].Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer1);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        // hide one of the visible cities, whilst counting
        foreach (BasesBeingDefended defendedBase in s_players[player].AllInfrastructureBeingDefended)
        {
            if (defendedBase is not City) continue; // silo

            City city = (City)defendedBase;

            if (!city.Visible || city.IsDestroyed) continue;

            city.Visible = false;

            s_players[player].CitiesSaved++;

            //if (player != 0) // shown for CPU but we don't give it the points, else it will never lose
            s_players[player].score += Multiplier * 100; 
            break; // we're done
        }

        // leverage main drawing engine for the scenery
        s_players[player].Draw(gPlayer);

        // draw the ammo (leverage the bit we already show)
        OverlayBonusAmmo(s_players[player], gPlayer);

        // add the cities to it
        OverlayBonusCity(s_players[player], gPlayer);

        // replace what the user sees with the newly drawn image
        gPlayer.Flush();
        s_players[player].Canvas.Image = displayPlayer1;
    }

    /// <summary>
    /// Write cities saved count pkus icons.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="g"></param>
    private static void OverlayBonusCity(Player player, Graphics g)
    {
        using Font f = new("Courier New", 20, FontStyle.Bold);
        using SolidBrush mainNumbers = new(SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ICBMColour);

        string bonusAmountText = (player.CitiesSaved * 100 * Multiplier).ToString();

        /*
            ; Draw one city icon in bonus tabulation area.
            5cd7: a5 8d                        lda     wave_icbm_count   ;get counter
            5cd9: e6 8d                        inc     wave_icbm_count   ;increment for next iteration
            5cdb: 0a                           asl     A                 ;multiply by 16
            5cdc: 0a                           asl     A                 ; because city icons are 16x8
            5cdd: 0a                           asl     A
            5cde: 0a                           asl     A
            5cdf: 18                           clc
            5ce0: 65 8d                        adc     wave_icbm_count   ;add one to make it x17
            5ce2: 18                           clc                       ; to leave a little space between city icons
            5ce3: 69 80                        adc     #128              ;relative to center of screen
            5ce5: aa                           tax                       ;use as horizontal position
            5ce6: a0 60                        ldy     #96               ;set vertical position
         */
        g.DrawString(bonusAmountText, f, mainNumbers, new Point(64 * 2, (231 - 96) * 2));

        Bitmap abmCityIcon = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].City;

        for (int city = 0; city < player.CitiesSaved; city++)
        {
            g.DrawImageUnscaled(abmCityIcon, 122 * 2 + 17 * 2 * city, (231 - 92) * 2);
        }
    }
}