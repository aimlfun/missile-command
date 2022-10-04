using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using MissileDefence.UX;
using MissileDefence.Attackers.Training;
using MissileDefence.UX.Animations;
using MissileDefence.Defenders;
using MissileDefence.Attackers;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// Moves ABM, ICBMs etc and paints them on their "display".
    /// </summary>
    private void GameLoop()
    {        
        if (s_currentICBMWaveCPU is null) throw new ArgumentNullException(nameof(s_currentICBMWaveCPU) + " should not be null");
        if (s_currentICBMWaveUSER is null) throw new ArgumentNullException(nameof(s_currentICBMWaveUSER) + " should not be null");

        // end of wave is when ALL ICBMs are destroyed or all cities are destroyed.
        if ((s_currentICBMWaveCPU.WaveComplete || s_players[0].AllCitiesDestroyed() && (s_currentICBMWaveCPU.activeFlier == null || s_currentICBMWaveCPU.activeFlier.isEliminated)) && 
            (s_currentICBMWaveUSER.WaveComplete || s_players[1].AllCitiesDestroyed() && (s_currentICBMWaveUSER.activeFlier == null || s_currentICBMWaveUSER.activeFlier.isEliminated)))
        {
            if (!s_players[0].InGameOverAnimation && !s_players[1].InGameOverAnimation)
            {
                State = PossibleStatesForTheStateMachine.EndOfWave;
                return;
            }
        }

        ++s_framesRendered;

        // authentic-ish, based on 60hz refresh rate
        ++icbmFrame;

        if (icbmFrame >= icbmFrameDelay) // slows down the missiles, skipping a move in earlier levels
        {
            if (!s_players[0].GameOver) s_currentICBMWaveCPU.MoveAll();
            if (!s_players[1].GameOver) s_currentICBMWaveUSER.MoveAll();

            icbmFrame -= icbmFrameDelay; // not "--icbmFrame", we want to skip some frames to meet frame-rate
        }

        // fliers
        if (!s_players[0].GameOver) s_currentICBMWaveCPU.MoveFlier();
        if (!s_players[1].GameOver) s_currentICBMWaveUSER.MoveFlier();

        // CPU PLAYER
        MoveABMsDrawICBMandABMsForPlayer(s_currentICBMWaveCPU, s_players[0]);

        // USER PLAYER
        MoveABMsDrawICBMandABMsForPlayer(s_currentICBMWaveUSER, s_players[1]);
    }

    /// <summary>
    /// Although ABMs are controlled differently, the basic logic is the same for both
    /// playes, so we do it once here. 
    /// 
    /// Note: ICBMs are moved (not drawn) before calling due to the need to "slow" them 
    /// using the frame-delay.
    /// </summary>
    /// <param name="wave"></param>
    /// <param name="player"></param>
    private static void MoveABMsDrawICBMandABMsForPlayer(WaveOfICBMs? wave, Player player)
    {
        if(wave is null) throw new ArgumentNullException(nameof(wave),"should not be null");

        if (player.GameOver)
        {
            if(player.CrossHairVisible) player.CrossHairVisible = false;

            GameOverAnimation(player);
            return; // if game over for player, we don't move anything for them
        }

        // enable lauch / guidance controls to AI | enable user to target/fire ICBMs
        player.MoveTrackBallAndFire(wave);

        // render frame

        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        player.Draw(gPlayer);

        // draws the ICBMs
        wave?.DrawAll(gPlayer);

        // replace with new rendered frame
        gPlayer.Flush();
        player.Canvas.Image = displayPlayer;
    }
}