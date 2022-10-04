using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using MissileDefence.UX;
using MissileDefence.UX.Animations;
using MissileDefence.Defenders;
using MissileDefence.Controllers;
using MissileDefence.Attackers.Training;
using MissileDefence.Attackers;

namespace MissileDefence.Controllers.Game;

/// <summary>
/// Controls the game from training, demo to waves, and hi-score tracking.
/// IMPORTANT: THIS IS A BIG CLASS, SPLIT ACROSS MULTIPLE .CS FILES
/// </summary>
internal partial class GameController
{

    /*
        M I S S I L E   C O M M A N D
        The original arcade game (c) Atari 1980. 
    
        ROM source code is now in the public domain (GitHub) by authors.
        https://github.com/historicalsource/missile-command

        Also see: https://6502disassembly.com/va-missile-command/ for a great disassembly by Andy McFadden.

        ORIGINAL ARCADE GAME PLAY

        ICBMs can drop from the top of the screen or be launched by fliers, which move horizontally about halfway up the screen. 
        Incoming missiles move slowly on the first wave, but speed up as the game progresses, and become more numerous. 
    
        After a few waves, smart bombs start to appear. These move at the same speed as missiles, but are able to change direction
        to dodge around explosions.

        Use the trackball to position the crosshairs, and the three fire buttons to launch ABMs from the left, center, or right 
        missile silo. There are ten ABMs per silo. ABMs launched from the side silos move more slowly (3 units per frame) than
        those launched from the center silo (7 units per frame), making the center silo more effective for last-minute intercepts.

        ICBMs that reach the area at the bottom of the screen will destroy a city or missile silo. Silos are fully restored at the
        start of each wave.

        The wave ends after a certain number of missiles and smart bombs have been destroyed.

        Points are awarded for destroying attackers as follows: (x multiplier)

        Hostile Entity	Points
        Missile	        25
        Satellite	    100
        Bomber	        100
        Smart Bomb	    125

        MY TWIST ON ORIGINAL...

        This "implementation" was createdly solely for the "AI" aspect of targetting. The final training approach, scoring
        and configuration took many evenings to get right. The AI model files were not trained to 100% accuracy, that is 
        theoretically possible, but over kill. It can and will miss some very low ICBMs, this happens _if_ lots of ICBMs 
        arrive simultaneously and are MIRV. By that point the CPU has racked up lots of bonus cities, so no big deal!
    
        It crazily morphed into "if you're going to simulate MC, why not use the same graphics and basic game play". It is
        *intentionally* not identical.
    
        Default "High Scores" are from the Arcade original but they are not necessarily comparable due to game differences.

        For a user to play against the AI it is only fair they receive the "same" ICBM wave. That requires pre-computing 
        random targets and vertical distances including MIRV splits, rather than the original's  spawn with a limit to on 
        screen (8). It does respect the number per wave, and attempt the same frame rate for ICBMs.

        In the interest of fairness to humans: (as it is seriously quick / accurate)
        - the AI is required to directly hit the ICBM it chose, whereas the human can kill multiple in one blast (a large
          blast) which means the AI runs out of ABMs in a scenario where the user could kill 3 or more ICBMs with one ABM.
        - the accuracy expected from the human is significantly less (a larger distance to target).
        - the USER can shoot the fliers, the AI is not permitted.
        - the CPU is only allowed to have "one" high-score in the table; otherwise you probably won't have a high score 
          for long!
     */

    /// <summary>
    /// 
    /// </summary>
    private enum BonusPointsStates { init, counting }

    /// <summary>
    /// Used where we need to render based on "time" (for example slow ICBMs or pause as we "count" ammo/cities).
    /// </summary>
    float icbmFrame = 0;

    /// <summary>
    /// Track the frame we are painting, so we can toggle display of left/right arrow.
    /// </summary>
    static internal int s_framesRendered = 1;

    /// <summary>
    /// The wave of missiles that will or are attacking the CPU (identical to USER).
    /// </summary>
    internal static WaveOfICBMs? s_currentICBMWaveCPU;

    /// <summary>
    /// The wave of missiles that will or are attacking the USER (identical to CPU).
    /// </summary>
    internal static WaveOfICBMs? s_currentICBMWaveUSER;

    /// <summary>
    /// 
    /// </summary>
    bool initPrePlayPause = true;

    /// <summary>
    /// The theme changes every 2 waves, and cycles.
    /// </summary>
    internal static int s_UXWaveTheme = 1;

    /// <summary>
    /// The level the player has reached (which affects the difficulty).
    /// </summary>
    internal static int s_playerWave = 1;

    /// <summary>
    /// User receives up to 6 times the value of target depending on their wave.
    /// </summary>
    internal static int Multiplier
    {
        get
        {
            return ((s_playerWave + 1) / 2).Clamp(1, 6);
        }
    }

    /// <summary>
    /// Used to slow down missiles in earlier stages.
    /// </summary>
    float icbmFrameDelay = 0;

    /// <summary>
    /// Tracks the players.
    /// </summary>
    internal static Player[] s_players = new Player[2];

    /// <summary>
    /// When false, we don't show bases (such as during title animation).
    /// </summary>
    internal static bool s_showingBases = false;

    /// <summary>
    /// 
    /// </summary>
    private BonusPointsStates bonusPointsState = BonusPointsStates.init;

    /// <summary>
    /// 
    /// </summary>
    private int cpuPlayerMissilesRemaining = -1;

    /// <summary>
    /// 
    /// </summary>
    private int userPlayerMissilesRemaining = -1;

    /// <summary>
    /// 
    /// </summary>
    private int cpuCitiesRemaining = -1;

    /// <summary>
    /// 
    /// </summary>
    private int userCitiesReamining = -1;

    /// <summary>
    /// Constructor. Initialises everything, remembers the 2 canvases.
    /// </summary>
    /// <param name="canvasCPUPlayer">PictureBox canvas for CPU player.</param>
    /// <param name="canvasUSERPlayer">PictureBox canvas for USER player.</param>
    /// <param name="enterTrainingMode">true - we have no AI model, we need to enter "training" mode.</param>
    internal GameController(PictureBox canvasCPUPlayer, PictureBox canvasUSERPlayer, bool enterTrainingMode)
    {
        Initialise();

        s_players[0] = new PlayerCPU(canvasCPUPlayer)
        {
            CrossHairVisible = true
        };

        s_60hzTimer.Interval = c_gameTimerInterval;
        s_60hzTimer.Tick += Timer_TickEvery60hz;

        if (enterTrainingMode)
        {
            State = PossibleStatesForTheStateMachine.Training;
            p1AnimationControllerDemoMode = null;
            p2AnimationControllerDemoMode = null;
            s_trainingController = new TrainingController(canvasCPUPlayer, canvasUSERPlayer);
            TrainingDataPoints.GenerateTrainingPoints();
            return;
        }

        s_players[1] = new PlayerUser(canvasUSERPlayer);

        State = PossibleStatesForTheStateMachine.Reset;
    }

    /// <summary>
    /// Resets the game.
    /// </summary>
    private void Reset()
    {
        s_playerWave = 1;
        s_UXWaveTheme = 1;
        icbmFrame = 0;
        
        s_players[1].CrossHairVisible = true;
        p1AnimationControllerDemoMode = new AnimationController(s_players[0], c_gameTimerInterval);
        p2AnimationControllerDemoMode = new AnimationController(s_players[1], c_gameTimerInterval);

        s_players[0].Reset();
        s_players[1].Reset();

        InitialiseAnimations(p1AnimationControllerDemoMode);
        State = PossibleStatesForTheStateMachine.ShowTitle;
    }

    /// <summary>
    /// 
    /// </summary>
    private static void Initialise()
    {
    }
}
