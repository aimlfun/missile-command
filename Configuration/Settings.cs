using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Configuration;

/// <summary>
/// A simple global "settings" object.
/// </summary>
internal static class Settings
{
    /// <summary>
    /// Settings impacting the heat-sensor.
    /// </summary>
    internal static SensorSettings s_sensor = new();

    /// <summary>
    /// Settings impacting the AI.
    /// </summary>
    internal static AISettings s_aI = new();

    /// <summary>
    /// Settings impacting everything else.
    /// </summary>
    internal static WorldSettings s_world = new();

    /// <summary>
    /// Settings for debugging.
    /// </summary>
    internal static DebugSettings s_debug = new();
}
