using MissileDefence.Attackers;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// Prepare a wave of ICBMs that are of an appropriate difficulty based on level,
    /// and assign it to both players.
    /// </summary>
    private void PrepareWave()
    {
        icbmFrame = 0;
    
        List<ICBMDefinition> definitionOfWave = MissileWaveGenerator.GenerateWave(s_playerWave, out List<FlierDefinition> flierDefinition);

        s_currentICBMWaveCPU = new WaveOfICBMs(s_players[0]);
        s_currentICBMWaveUSER = new WaveOfICBMs(s_players[1]);

        // we want to generate a "wave" of missile (position split etc) so we can play the same on both players
        // screens
        s_currentICBMWaveCPU.InitiateWave(definitionOfWave, flierDefinition);
        s_currentICBMWaveUSER.InitiateWave(definitionOfWave, flierDefinition);

        // this controls the "speed" of ICBMs (they get faster), max speed is at "19" so we clamp, but indexes start at 0 (so we -1).
        icbmFrameDelay = MissileWaveGenerator.WaveDefinitions[s_playerWave.Clamp(1, 19) - 1, /* frame rate */ 1];

        State = PossibleStatesForTheStateMachine.PrePlayPause; // shows the player <x> | <n> points
        initPrePlayPause = true;

        // it's difficult to play without the cross hair!
        s_players[1].CrossHairVisible = true;

        // don't move the cursor to the center of the players "screen" if the form isn't focused or it's game
        // over, as it is annoying when you're doing something in the background!
        if (s_players[1].Canvas.FindForm().Focused && !s_players[1].GameOver) Cursor.Position = s_players[1].Canvas.PointToScreen(new Point(256, 231));
        
        s_framesRendered = 0;
    }
}