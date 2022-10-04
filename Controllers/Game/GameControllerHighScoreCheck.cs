using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Controllers.Game
{
    partial class GameController
    {
        /// <summary>
        /// If one of the users has a high-score, prompt them, else show the high scores.
        /// </summary>
        private static void HighScoreCheck()
        {
            if (s_highScoreManager.IsHighScore(s_players[0]) || s_highScoreManager.IsHighScore(s_players[1]))
            {
                State = PossibleStatesForTheStateMachine.EnterInitials;
            }
            else
            {
                State = PossibleStatesForTheStateMachine.ShowHighScores;
            }
        }
    }
}
