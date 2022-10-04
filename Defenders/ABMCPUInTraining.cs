using MissileDefence.AI;
using MissileDefence.Attackers;
using MissileDefence.Controllers;
using MissileDefence.Controllers.Game;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Defenders;

internal class ABMCPUInTraining : ABMCPUControlled
{
    internal ABMCPUInTraining(int missileId, BasesBeingDefended sentFromBase, ICBM icbm, double angle, Color smokeColour, OnTargetHit callback) : base(missileId, sentFromBase, icbm, angle, smokeColour, callback)
    {
        Elimination += ABMInTraining_Elimination;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="abm"></param>
    /// <param name="reason"></param>
    private void ABMInTraining_Elimination(ABM abm, string reason)
    {
        if (reason == "hit")
        {
            TrainingController.s_killRatio[NeuralNetwork.s_networks[abm.Id].Id]++;
            TrainingController.s_targetsHit++;

            if (activeTarget != null)
            {
                activeTarget.Killed();

                if (!TrainingController.InQuietMode()) GameController.s_players[0].explosionManager.Add(activeTarget.LocationInDeviceCoordinates);
            }
        }
        else
        {
            TrainingController.s_missRatio[Id]++;
        }
    }

}
