using MissileDefence.Defenders;
using MissileDefence.UX;

namespace MissileDefence.Attackers;

/// <summary>
/// A representation of an Inter-Continental Ballistic Missile.
/// ICBMS are a dot that flies from a fixed point (decided for the wave), to a point in the 
/// center of a silo/city using Bresenham's algorithm. (The original didn't use it).
/// </summary>
internal class ICBM
{
    /// <summary>
    /// If set to true, e.g. for training, the missile will not move.
    /// </summary>
    internal bool MoveDisabled = false;

    /// <summary>
    /// Used to notify when a base is hit by the ICBM.
    /// </summary>
    public delegate void BaseHitCallback(BasesBeingDefended? targetHit);

    /// <summary>
    /// Stores the points of smoke.
    /// </summary>
    internal List<PointF> smokeTrailPoints = new();

    /// <summary>
    /// Used to navigate in a straight line from start to the target.
    /// </summary>
    readonly BresenhamLine lineData;

    /// <summary>
    /// Where the ICBM is located in device coordinates.
    /// </summary>
    internal Point LocationInDeviceCoordinates
    {
        get
        {
            return lineData.Location;
        }
    }

    /// <summary>
    /// The altitude at which the ICBM splits, known as a Multiple Independent Rentry Vehicle (MIRV).
    /// </summary>
    internal int splitAtAltitude = -1;

    /// <summary>
    /// If not empty, contains a list of MIRV targets (the split will make this many new missiles launched at the targets). int = silo/city array index.
    /// </summary>
    internal List<int> MIRVTargets = new();

    /// <summary>
    /// True means this ICBM has been eliminated.
    /// </summary>
    private bool eliminated = false;

    /// <summary>
    /// Returns the elimination status.
    /// </summary>
    internal bool IsEliminated
    {
        get
        {
            return eliminated;
        }
        set
        {
            eliminated = value;
        }
    }

    /// <summary>
    /// Tracks the "smoke" colour.
    /// </summary>
    private readonly Color smokeTrailColour;

    /// <summary>
    /// Tracks the last location the missile was at in device coordinates.
    /// </summary>
    private Point lastLocationInDeviceCoordinates;

    /// <summary>
    /// Contains the base being targetted.
    /// </summary>
    readonly BasesBeingDefended? baseTargettedByMissile = null;

    /// <summary>
    /// Event handler called when the ICBM hits a base (silo/city).
    /// </summary>
    internal event BaseHitCallback? BaseHit;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="startLocationInMCCoordinates"></param>
    /// <param name="targetBase"></param>
    /// <param name="splitAltitude"></param>
    /// <param name="targets"></param>
    /// <param name="baseHit"></param>
    /// <param name="smokeColor"></param>
    internal ICBM(PointA startLocationInMCCoordinates,
                  BasesBeingDefended targetBase,
                  int splitAltitude,
                  List<int> targets,
                  BaseHitCallback? baseHit,
                  Color smokeColor)
    {
        baseTargettedByMissile = targetBase;
        smokeTrailColour = smokeColor;

        // guidance of ICBM to hit the base.
        lineData = new BresenhamLine(startLocationInMCCoordinates.MCCoordsToDeviceCoordinatesP(),
                                     targetBase.LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP());

        splitAtAltitude = splitAltitude;
        MIRVTargets = targets;
        lastLocationInDeviceCoordinates = startLocationInMCCoordinates.MCCoordsToDeviceCoordinatesP();
        BaseHit = baseHit;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="startLocationInMCCoordinates"></param>
    /// <param name="targetLocationInMCCoordinates"></param>
    /// <param name="baseHit"></param>
    /// <param name="smokeColor"></param>
    internal ICBM(PointA startLocationInMCCoordinates,
                  PointA targetLocationInMCCoordinates,
                  BaseHitCallback baseHit,
                  Color smokeColor)
    {
        baseTargettedByMissile = null;
        smokeTrailColour = smokeColor;  // caller:  SharedUX.UXColorsPerLevel[GameController.UXWave].ICBMColour;
        lineData = new BresenhamLine(startLocationInMCCoordinates.MCCoordsToDeviceCoordinatesP(),
                                     targetLocationInMCCoordinates.MCCoordsToDeviceCoordinatesP());
        splitAtAltitude = -1;
        MIRVTargets = new List<int>();
        lastLocationInDeviceCoordinates = startLocationInMCCoordinates.MCCoordsToDeviceCoordinatesP();
        BaseHit = baseHit;
    }

    /// <summary>
    /// Moves the ICBM.
    /// </summary>
    internal bool Move()
    {
        if (eliminated) return false; // eliminated ICBMs do not move.

        if (MoveDisabled) return false; // disabled cannot move.

        Point locationInDeviceCoordinates = new();

        // we do this in a loop, as we need to move more than once.
        for (int i = 0; i < 2; i++)
        {
            if (lineData.NextPoint(out locationInDeviceCoordinates))
            {
                HitBase();
                return true;
            }
        }

        // onscreen, and has moved record the position for smoke trail
        if (locationInDeviceCoordinates.Y >= 21 &&
           (locationInDeviceCoordinates.X != lastLocationInDeviceCoordinates.X ||
            locationInDeviceCoordinates.Y != lastLocationInDeviceCoordinates.Y))
        {
            smokeTrailPoints.Add(new Point(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y));
        }

        lastLocationInDeviceCoordinates = new Point(locationInDeviceCoordinates.X, locationInDeviceCoordinates.Y);

        return false;
    }

    /// <summary>
    /// Draws the ICBM with its smoke trail.
    /// </summary>
    /// <param name="g"></param>
    internal void Draw(Graphics g)
    {
        if (smokeTrailPoints.Count > 1) // 2 points minimum to draw a "line".
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            using Pen p = new(smokeTrailColour, 2);
            g.DrawLines(p, smokeTrailPoints.ToArray()); // these are stored in device coordinates
        }

        if (LocationInDeviceCoordinates.Y < 21) return; // we don't show them if above this point

        using SolidBrush b = new(Color.White);

        Point location = lineData.Location;
        Rectangle rectSquare = new(location.X - 1, location.Y - 1, 2, 2);

        // war head.
        g.FillRectangle(b, rectSquare);
    }

    /// <summary>
    /// Called when base is hit.
    /// </summary>
    void HitBase()
    {
        eliminated = true;
        BaseHit?.Invoke(baseTargettedByMissile);
    }

    /// <summary>
    /// Called when this ICBM is destroyed.
    /// </summary>
    internal void Killed()
    {
        eliminated = true;
    }
}