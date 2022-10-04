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
    /// Instance of this object.
    /// </summary>
    internal static TrainingController? s_trainingController;

    /// <summary>
    /// No playing game, just a self contained training app.
    /// </summary>
    internal static void TrainingLoop()
    {
        s_trainingController?.TimerRocketMove_Tick();
    }
}
