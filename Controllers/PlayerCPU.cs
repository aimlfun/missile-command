using MissileDefence.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.AI;
using MissileDefence.Controllers.Game;
using MissileDefence.Attackers;
using MissileDefence.Defenders;
using MissileDefence.UX;

namespace MissileDefence.Controllers;

/// <summary>
/// A "CPU" player.
/// </summary>
internal class PlayerCPU : Player
{
    /// <summary>
    /// Tracks which ICBMs have had an ABM dispatched by the CPU to destroy them.
    /// This prevents it launching multiple at the same target.
    /// </summary>
    internal List<ICBM> listOfICBMsBeingAttacked = new();

    /// <summary>
    /// Tracks each of the available ABMs by their "id".
    /// </summary>
    readonly private Dictionary<int, ABMCPUControlled> cpuControlledABMsIndexById = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="canvas"></param>
    internal PlayerCPU(PictureBox canvas) : base(PlayerMode.cpu, canvas)
    {
        InitialiseTheNeuralNetworksForTheRockets();
    }

    /// <summary>
    /// 
    /// </summary>
    internal void ClearICBMs()
    {
        listOfICBMsBeingAttacked.Clear();
        cpuControlledABMsIndexById.Clear();
    }

    /// <summary>
    /// Initialises the neural network (one per ABM).
    /// </summary>
    internal static void InitialiseTheNeuralNetworksForTheRockets()
    {
        NeuralNetwork.s_networks.Clear();

        // we have 30 ABMs each with their own trained neural network
        for (int abmId = 0; abmId < Settings.s_aI.NumberOfABMsToCreate; abmId++)
        {
            _ = new NeuralNetwork(abmId, NeuralNetwork.Layers);
        }

        float maxFitness = -int.MaxValue;
        int maxNN = -1;

        // load the trained missiles
        foreach (int id in NeuralNetwork.s_networks.Keys)
        {
            NeuralNetwork nn = NeuralNetwork.s_networks[id];
            nn.Load(Path.Combine(Program.aiFolder, $"missile{nn.Id}.ai"));

            if (nn.Fitness > maxFitness)
            {
                maxFitness = nn.Fitness;
                maxNN = id;
            }
        }

        // copy the "best" performing AI to *all* ABMs.
        foreach (int id in NeuralNetwork.s_networks.Keys)
        {
            if (id != maxNN) NeuralNetwork.CopyFromTo(NeuralNetwork.s_networks[maxNN], NeuralNetwork.s_networks[id]);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wave"></param>
    internal override void MoveTrackBallAndFire(WaveOfICBMs wave)
    {
        base.MoveTrackBallAndFire(wave);

        /*
         ; ORIGINAL:     
         ; Updates the position of the crosshairs, clamping the movement to min/max
         ; values from a table.  We don't want the crosshairs to be up in the score area,
         ; or down with the cities, or wrapping around left and right.

     5256: 08           :min_coord      .dd1    8                 ;min X coord
     5257: 2d                           .dd1    45                ;min Y coord (above cities)

     5258: f7           :max_coord      .dd1    247               ;max X coord
     5259: ce                           .dd1    206               ;max Y coord (comfortably below top)

         AS THIS IS APPLIED TO THE USER, SO IT SHALL BE A FAIR CONTEST AND APPLIED TO AI!

         Remember MC has 0 at the BOTTOM, not the top, so we need to invert.
           minY = 45 -> 45 pixels from bottom = 231-45 (x2 scale) = 372
           maxT = 206 -> 206 from bottom = 25 (x2 scale) = 50
         */

        foreach (ICBM icbm in wave.ICBMs)
        {
            // offscreen cannot be defended against, and we don't want to waste multiple against same target
            if (icbm.IsEliminated ||
                // fairness, imposed on user
                icbm.LocationInDeviceCoordinates.Y > 372 ||
                icbm.LocationInDeviceCoordinates.Y < 50 ||
                icbm.LocationInDeviceCoordinates.X < 16 ||
                icbm.LocationInDeviceCoordinates.X > 494 ||
                AlreadyTracking(icbm)) continue;

            ABMSilo? optimalABMBase = null;
            float minDist = int.MaxValue;

            foreach (ABMSilo abmBase in Silos)
            {
                if (abmBase.IsDestroyed || abmBase.ABMRemaining == 0) continue; // no missiles to fire

                // find the closest.
                PointF p = abmBase.LocationInMCCoordinates.MCCoordsToDeviceCoordinates();

                float dist = MathUtils.DistanceBetweenTwoPoints(p, icbm.LocationInDeviceCoordinates);

                if (dist < minDist)
                {
                    minDist = dist;
                    optimalABMBase = abmBase;
                }
            }

            if (optimalABMBase == null) continue; // no base has missiles.

            listOfICBMsBeingAttacked.Add(icbm);

            double launchAngle = ABMSilo.LaunchAngle(optimalABMBase, icbm);
            int index = -1;

            for (int slot = 0; slot < cpuControlledABMsIndexById.Count; slot++)
            {
                if (cpuControlledABMsIndexById[slot].IsEliminated)
                {
                    index = slot;
                    break;
                }
            }

            if (index == -1)
            {
                cpuControlledABMsIndexById.Add(cpuControlledABMsIndexById.Count, new(cpuControlledABMsIndexById.Count, optimalABMBase, icbm, launchAngle, SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ABMColour, CallbackIfHitTarget));
            }
            else
            {
                cpuControlledABMsIndexById[index] = new(index, optimalABMBase, icbm, launchAngle, SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ABMColour, CallbackIfHitTarget);
            }

            optimalABMBase.ABMRemaining--;
        }

        MoveABMs();
        MoveABMs();
        MoveABMs();
        MoveABMs();
    }

    /// <summary>
    /// Call back event handler for target hit.
    /// </summary>
    private void CallbackIfHitTarget(ABM abm)
    {
        score += 25 * GameController.Multiplier;
    }

    /// <summary>
    /// Used to stop multiple ABMs targeting the samee ICBM
    /// </summary>
    /// <param name="icbm"></param>
    /// <returns></returns>
    private bool AlreadyTracking(ICBM icbm)
    {
        return listOfICBMsBeingAttacked.Contains(icbm);
    }

    /// <summary>
    /// Draws smoke trails and blobs for inflight ABMs.
    /// </summary>
    /// <param name="g"></param>
    internal override void Draw(Graphics g)
    {
        base.Draw(g);

        // overlay the ABM smoke paths
        foreach (int id in cpuControlledABMsIndexById.Keys)
        {
            cpuControlledABMsIndexById[id].DrawPaths(g);
        }

        // lastly add a square at the head of the missile
        foreach (int id in cpuControlledABMsIndexById.Keys)
        {
            cpuControlledABMsIndexById[id].Draw(g);
        }
    }

    /// <summary>
    /// Moves the rockets either using "parallel" or serial. 
    /// </summary>
    private bool MoveABMs()
    {
        if (Settings.s_world.UseParallelMove)
        {
            // this should run much faster (multi-threading). Particularly good if AI is lots of neurons
            Parallel.ForEach(cpuControlledABMsIndexById.Keys, id =>
            {
                ABMCPUControlled abm = cpuControlledABMsIndexById[id];

                if (!abm.IsEliminated)
                {
                    abm.Move();
                }
                else
                {
                    if (abm.EliminatedBecauseOf != "" && abm.EliminatedBecauseOf != "hit" && listOfICBMsBeingAttacked.Contains(abm.activeTarget))
                    {
                        if(listOfICBMsBeingAttacked.Contains(abm.activeTarget))  listOfICBMsBeingAttacked.Remove(abm.activeTarget);
                    }
                }
            });
        }
        else
        {
            foreach (int id in cpuControlledABMsIndexById.Keys)
            {
                ABMCPUControlled abm = cpuControlledABMsIndexById[id];

                if (!abm.IsEliminated)
                {
                    abm.Move();
                }
                else
                {
                    if (abm.EliminatedBecauseOf != "" && abm.EliminatedBecauseOf != "hit" && listOfICBMsBeingAttacked.Contains(abm.activeTarget))
                    {
                        listOfICBMsBeingAttacked.Remove(abm.activeTarget);
                    }
                }
            }
        }

        // we return true if there are rockets not eliminated that can be moved
        foreach (var id in cpuControlledABMsIndexById.Keys)
        {
            ABM r = cpuControlledABMsIndexById[id];

            if (!r.IsEliminated) return true;
        }

        return false;
    }
}
