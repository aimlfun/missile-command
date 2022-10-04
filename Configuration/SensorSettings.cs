using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Configuration;

/// <summary>
/// Settings controlling the heat sensor.
/// </summary>
public class SensorSettings
{
    /// <summary>
    /// sample -15..+15 => 31
    /// </summary>
    public float SamplePoints = 17;

    /// <summary>
    /// We launch ABMs straight upwards, to have a chance of locating target we look up to 50 degrees left.
    /// </summary>
    public float FieldOfVisionStartInDegrees = -50;

    /// <summary>
    /// We launch ABMs straight upwards, to have a chance of locating target we look up to 50 degrees right.
    /// </summary>
    public float FieldOfVisionStopInDegrees = 50;

    /// <summary>
    /// The field of vision is split into sample segments, this is the angle that represents.
    /// </summary>
    public float VisionAngleInDegrees
    {
        get
        {
            return (SamplePoints == 1) ? 0 : (FieldOfVisionStopInDegrees - FieldOfVisionStartInDegrees) / (SamplePoints - 1);
        }
    }

    /// <summary>
    /// If true, it draws the cone of the heat sensor.
    /// </summary>
    public bool DrawHeatSensor = false;

    /// <summary>
    /// If true, it draws the red triangle showing the heat sensor.
    /// </summary>
    public bool DrawTargetPartOfHeatSensor = false;
}