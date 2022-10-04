using MissileDefence.Configuration;
using MissileDefence.Defenders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Controllers;

/// <summary>
/// Score the AI neural network based on whether it hit the target, and if not, how close.
/// </summary>
internal static class AIScoring
{
    /// <summary>
    /// Used to provide ranking, higher == better.
    /// </summary>
    /// <param name="Generation"></param>
    /// <param name="r"></param>
    /// <param name="killratio"></param>
    /// <returns></returns>
    internal static float Fitness(int Generation, ABM r, float killratio)
    {
        float fitness;

        bool hasHitTarget = r.HasHitTarget();

        if (hasHitTarget)
        {
            fitness = (2 + killratio) * 160; // keep the best ones top
        }
        else
        {
            float extra = 0;
            float dist = r.DistanceFromActiveTarget();

            if (dist < 320) extra = (320 - dist) / 2;

            // extra for being closer
            fitness = killratio * 160 + extra; // keep the best ones top
        }

        // change to lower number of mutations / ABM's when we reached a hopefully stable point.
        if (Settings.s_aI.Mutate50pct && Generation > 500 && hasHitTarget) Settings.s_aI.Mutate50pct = false;

        return fitness;
    }
}
