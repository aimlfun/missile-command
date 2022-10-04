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
    /// Shows the bonus points for each ammo saved.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    private void ABMBonusPoints()
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
            s_players[0].BonusAmmoAward = 0;
            s_players[1].BonusAmmoAward = 0;
            s_framesRendered = 0;
            bonusPointsState = BonusPointsStates.counting;
        }

        /*
         ; Func $0a: award bonus points for unfired ABMs.
         ; 
         ; This is called repeatedly.  If not enough time has passed, it returns
         ; immediately.  Otherwise, it finds the next unfired ABM and awards bonus points
         ; for it.
        */

        if (++s_framesRendered % 4 != 0) return; // count slowly, no need to paint in between

        cpuPlayerMissilesRemaining = s_players[0].CountOfABMsRemainingInSilos; // 0 if "GameOver" for player
        userPlayerMissilesRemaining = s_players[1].CountOfABMsRemainingInSilos; // 0 if "GameOver" for player

        // we're removing ABMs as we count, eventually both will reach zero and we need to do bonus city check.
        if (cpuPlayerMissilesRemaining + userPlayerMissilesRemaining == 0)
        {
            State = PossibleStatesForTheStateMachine.CityBonusChk;
            bonusPointsState = BonusPointsStates.init;
            return;
        }

        // CPU PLAYER
        if (cpuPlayerMissilesRemaining > 0)
        {
            CountAmmoRemainingForPlayerAndShowBonusForPlayer(0);
        }

        // USER PLAYER
        if (userPlayerMissilesRemaining > 0)
        {
            CountAmmoRemainingForPlayerAndShowBonusForPlayer(1);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    private static void CountAmmoRemainingForPlayerAndShowBonusForPlayer(int player)
    {
        /*
         ; Func $0a: award bonus points for unfired ABMs.
         ; 
         ; This is called repeatedly.  If not enough time has passed, it returns
         ; immediately.  Otherwise, it finds the next unfired ABM and awards bonus points
         ; for it.
         ; 
         ; The global normally used to pass a slot index ($97) is used here to keep track
         ; of the screen position for the ABM icon display.
         */
        Bitmap displayPlayer1 = (Bitmap)s_players[player].Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer1);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        s_players[player].Draw(gPlayer);

        foreach (ABMSilo silo in s_players[player].Silos)
        {
            if (silo.ABMRemaining > 0)
            {
                silo.ABMRemaining--;

                s_players[player].BonusAmmoAward++;

                if (player != 0) // shown for CPU but we don't give it the points, else it will never lose
                s_players[player].score += Multiplier * 5; 
                
                break; // we're done
            }
        }

        OverlayBonusAmmo(s_players[player], gPlayer);

        // replace what the user sees with the newly drawn image
        gPlayer.Flush();
        s_players[player].Canvas.Image = displayPlayer1;
    }

    /// <summary>
    /// Write "BONUS POINTS" with total ammo points, and icons
    /// </summary>
    /// <param name="player"></param>
    /// <param name="g"></param>
    private static void OverlayBonusAmmo(Player player, Graphics g)
    {
        string bonusText = "BONUS POINTS";

        using Font f = new("Courier New", 20, FontStyle.Bold);
       
        using SolidBrush mainNumbers = new(SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ICBMColour);
        using SolidBrush mainWriting = new(SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ABMColour);

        g.DrawString(bonusText, f, mainWriting, new Point(128, (231 - 165) * 2));

        /*
            5c37: a2 40                        ldx     #64               ;X coord
            5c39: a0 80                        ldy     #128              ;Y coord
            5c3b: a9 40                        lda     #%01000000        ;color #2
            5c3d: 20 f6 6a                     jsr     PrintBcdNumber    ;print score

            Score is at 64, 128. We're double screen size (256x31 vs. 512x462), so we need to double the amount.

            But remember, Y is inverted (0 top physical,0 bottom on MC)
        */
        string bonusAmountText = (player.BonusAmmoAward * 5 * Multiplier).ToString();

        g.DrawString(bonusAmountText, f, mainNumbers, new Point(64 * 2, (231 - 128) * 2));

        Bitmap abmAmmoIcon = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].Missile;

        for (int ammo = 0; ammo < player.BonusAmmoAward; ammo++)
        {
            /*
               ; 
               ; Draws an ABM ammo icon.
               ; 
               ;    #
               ;    #
               ;    #
               ;   ###
               ;   # #
               ;
            5c40: a9 82                        lda     #130              ;vertical position

            ...

            5c46: a5 97                        lda     slot_index        ;get icon counter
            5c48: 0a                           asl     A                 ;multiply by 4
            5c49: 0a                           asl     A
            5c4a: 18                           clc
            5c4b: 69 7a                        adc     #122              ;start near center of screen

            We're double screen size (256x31 vs. 512x462), so we need to double the amounts 122x2, 130x2 and x8. Again we invert the Y.
             */
            g.DrawImageUnscaled(abmAmmoIcon, 122 * 2 + 4 * 2 * ammo, (231 - 122) * 2);
        }
    }
}