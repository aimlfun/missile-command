using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// Starts the game.
    /// </summary>
    internal void StartGame()
    {
        if (State == PossibleStatesForTheStateMachine.InitGame) return;

        State = PossibleStatesForTheStateMachine.InitGame;

        s_playerWave = 1; // incremented during first wave
        s_UXWaveTheme = 1;

        s_players[0].StartGame();
        s_players[1].StartGame();

        Point p = s_players[1].Canvas.PointToScreen(new Point(s_players[1].CrossHairLocation.X, s_players[1].CrossHairLocation.Y));

        s_players[0].CrossHairVisible = false;
        if (s_players[1].Canvas.FindForm().Focused) Cursor.Position = p;

        State = PossibleStatesForTheStateMachine.PrepWave;
    }
}