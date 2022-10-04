using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Configuration;

/// <summary>
/// AI related settings.
/// </summary>
public class AISettings
{
    /// <summary>
    /// How many ABMs to start with. 30 = number that combined bases has.
    /// </summary>
    public int NumberOfABMsToCreate = 30; // MUST BE MULTIPLE OF TWO!! (mutate demands it)

    /// <summary>
    /// How many generations to compute in the background, before showing the user.
    /// </summary>
    public int GenerationsToTrainBeforeShowingVisuals = 0; // zero whilst we iron out bugs. 20;

    /// <summary>
    /// How much any directional thruster is amplified.
    /// </summary>
    public double AIthrusterNozzleAmplifier = 60F; // multiplier also used: 0.2 

    /// <summary>
    /// If true, we mutate half the rockets.
    /// If false, we clone all from the best and mutate each.
    /// </summary>
    public bool Mutate50pct = true;
}