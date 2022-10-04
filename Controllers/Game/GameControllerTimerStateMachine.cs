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
    /// States that the "machine" steps thru, skipping some.
    /// </summary>
    internal enum PossibleStatesForTheStateMachine
    {
        Training,
        Reset, ShowTitle, TitleExpls, InitGame, PrePlayPause,
        PrepWave, PlayGame, EndOfWave, AbmBonusPts, CityBonusChk, CityBonusPts, WaveDoneUpdate,
        GameOver, HighScoreCheck, EnterInitials,
        ShowHighScores, HalfCredit, WaitFullCredit, ShowEndTimes,
        NextWave
    }

    /// <summary>
    /// Tracks the state, so we know what to do/show.
    /// </summary>
    internal static PossibleStatesForTheStateMachine State;

    /// <summary>
    /// Windows timers are not amazingly accurate, but fire roughly to the schedule.
    /// We leverage to move the ABM/ICBMs. This code is meant to be about the simple 
    /// application of AI and not a purist best game building (WinForms
    /// wouldn't be used if that were the case).
    /// </summary>
    const int c_gameTimerInterval = 16; // ms 60hz each "frame" occurs as this fires. (16.6666ms, rounded down)

    /// <summary>
    /// Fires every c_gameTimerInterval (60hz), to move missiles.
    /// </summary>
    private static readonly System.Windows.Forms.Timer s_60hzTimer = new();

    /// <summary>
    /// If you press "S" the UI goes into slow mode, this is typically
    /// useful in training and watching the missile aim/miss.
    /// It does so by reducing the timer interval to 10x longer.
    /// </summary>
    private bool slowMode = false;

    /// <summary>
    /// If you press "S" the UI goes into slow mode, this is typically
    /// useful in training and watching the missile aim/miss.
    /// It does so by reducing the timer interval to 10x longer.
    /// </summary>
    internal bool SlowMode
    {
        get
        {
            return slowMode;
        }
        set
        {
            slowMode = value;

            s_60hzTimer.Interval = slowMode ? c_gameTimerInterval * 10 : c_gameTimerInterval;
        }
    }

    /// <summary>
    /// Called once every 60hz (unless in slow mode), and it routes to the context relevant
    /// routine.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Timer_TickEvery60hz(object? sender, EventArgs e)
    {
        /*
        ; ARCADE ORIGINAL:
        ; Main code loop.  We spin until the IRQ tells us to go, which it does once per
        ; frame (~60Hz).
        ; 
        ; The game uses a state machine to manage the higher-level activity, e.g.
        ; showing the title sequence vs. showing high scores vs. playing the game.  The
        ; code for each state will update the function number (in $91) when it's time to
        ; move on.
        ; 
        ; The main loop executes the current function, calls a routine to track any
        ; quarters that have dropped, and then loops around to wait until it's time for
        ; the next frame to execute.  In some cases the code may take longer than one
        ; frame to execute, e.g. drawing the high score list takes 44 frames.  At 128
        ; frames the IRQ handler will conclude that the main thread has stalled and
        ; reset the machine. 
         */

        switch (State)
        {
            case PossibleStatesForTheStateMachine.Training: TrainingLoop(); break;

            // GAME STATES
            case PossibleStatesForTheStateMachine.PlayGame: GameLoop(); break;

            case PossibleStatesForTheStateMachine.PrePlayPause: PrePlayPause(); break; // PLAYER 1,  <mult> X POINTS (defend cities visible etc)
            case PossibleStatesForTheStateMachine.PrepWave: PrepareWave(); break; // creates the random wave of ICBMs
            case PossibleStatesForTheStateMachine.EndOfWave: EndOfWave(); break; // after all ICBMs (or cities) destroyed 
            case PossibleStatesForTheStateMachine.AbmBonusPts: ABMBonusPoints(); break; // shows bonus points for remaining ABMs
            case PossibleStatesForTheStateMachine.CityBonusChk: CityBonusCheck(); break; // checks to see if a bonus city is to be assigned
            case PossibleStatesForTheStateMachine.CityBonusPts: CityBonusPoints(); break; // shows bonus points for remaining cities
            case PossibleStatesForTheStateMachine.NextWave: NextWave(); break; // resets and changes theme colour for next wave
            case PossibleStatesForTheStateMachine.GameOver: GameOver(); break; // it's all over for both players
            case PossibleStatesForTheStateMachine.HighScoreCheck: HighScoreCheck(); break; // determine if we need to show enter initials
            case PossibleStatesForTheStateMachine.EnterInitials:
                EnterHighScoreInitialsForBothPlayers();
                break;

            case PossibleStatesForTheStateMachine.ShowHighScores: ShowHighScores(); break; // show high scores
            case PossibleStatesForTheStateMachine.ShowTitle: DemoAnimations(); break;
            case PossibleStatesForTheStateMachine.Reset: Reset(); break;
        }
    }

    /// <summary>
    /// Toggles the pause/unpause. To pause we disable the timer.
    /// </summary>
    internal static void PauseUnpause()
    {
        s_60hzTimer.Enabled = !s_60hzTimer.Enabled;
    }

    /// <summary>
    /// Pause the game by disabling the timer.
    /// </summary>
    internal static void Pause()
    {
        s_60hzTimer.Enabled = false;
    }

    /// <summary>
    /// Unpause the game by enabling the timer.
    /// </summary>
    internal static void Unpause()
    {
        s_60hzTimer.Enabled = true;
    }

    /// <summary>
    /// Starts the game timer.
    /// </summary>
    internal static void Start()
    {
        s_60hzTimer.Start();
    }
}