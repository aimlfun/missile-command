using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Attackers.Training;

/// <summary>
/// Represents the IBCM end point, with the start tracked too; used in training.
/// </summary>
internal struct ICBMEndPoints
{
    /// <summary>
    /// Where the ICBM starts from.
    /// </summary>
    internal readonly PointA StartPoint;

    /// <summary>
    /// Where the ICBM is trying to get to.
    /// </summary>
    internal readonly PointA EndPoint;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    internal ICBMEndPoints(PointA startPoint, PointA endPoint)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
    }
}
