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
    /// Shows the multiplier for the player.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    private void PrePlayPause()
    {
        if (s_currentICBMWaveCPU is null) throw new ArgumentNullException(nameof(s_currentICBMWaveCPU) + " should not be null");
        if (s_currentICBMWaveUSER is null) throw new ArgumentNullException(nameof(s_currentICBMWaveUSER) + " should not be null");

        // first time we arrive at "ABMBonusPoints()" we initialise a few things
        if (initPrePlayPause)
        {
            s_framesRendered = 0;
            initPrePlayPause = false;

            if (!s_players[0].GameOver) DrawLevelCardAndPoints(s_players[0]);
            if (!s_players[1].GameOver) DrawLevelCardAndPoints(s_players[1]);
        }

        ++s_framesRendered;

        if (++s_framesRendered % (4 * 20) != 0) return; // count slowly, no need to paint in between

        State = PossibleStatesForTheStateMachine.PlayGame;
    }

    /// <summary>
    /// Shows something like the following the pauses.
    /// PLAYER 1
    /// 3 X POINTS
    /// </summary>
    /// <param name="player"></param>
    private static void DrawLevelCardAndPoints(Player player)
    {
        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        player.Draw(gPlayer);

        Color color1 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ABMColour;
        Color color2 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ICBMColour;

        using SolidBrush mainNumbers = new(color2);
        using SolidBrush mainWriting = new(color1);

        using Font f = new("Courier New", 20, FontStyle.Bold);
        
        gPlayer.DrawString("PLAYER", f, mainWriting, 190, 130);
        gPlayer.DrawString(player is PlayerCPU ? "1" : "2", f, mainNumbers, 312, 130);
        gPlayer.DrawString(Multiplier.ToString(), f, mainNumbers, 175, 200);
        gPlayer.DrawString("X POINTS", f, mainWriting, 210, 200);
        gPlayer.DrawString("WAVE", f, mainWriting, 190, 270);
        gPlayer.DrawString(s_playerWave.ToString().PadLeft(4), f, mainNumbers, 266, 270);

        using Font f2 = new("Lucida Console", 12, FontStyle.Bold);

        // \u00A9 is an OK (C), but \u00AE is a tiny (R)
        string copyright = "ATARI (c)(r) 1980";
        SizeF size = gPlayer.MeasureString(copyright, f2);
        gPlayer.DrawString(copyright, f2, mainWriting, 256 - size.Width / 2, 462 - 16);

        // replace with new rendered frame
        gPlayer.Flush();

        player.Canvas.Image = displayPlayer;
    }
}