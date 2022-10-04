using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Configuration;

/// <summary>
/// Settings that relate to the world.
/// </summary>
public class WorldSettings
{
    /// <summary>
    /// If true, it uses Parallel.Foreach(). This is faster, and with more complicate neural networks
    /// enables it to scale better
    /// </summary>
    public bool UseParallelMove = true;
}
