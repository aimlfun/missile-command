using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Configuration;

namespace MissileDefence.Defenders;

/// <summary>
/// Implementation of Anti-Ballistic-Missile; the kind of things that attempt to shoot down inbound ballstic missiles (such as 
/// ICBMs).
/// 
/// Responsibilities:
/// * Move missile - inherit from this class, and provide the "brains" to guide the missile: override GuideMissile()
/// * Draw missile - provides smoke trail.
/// * Notifies when hits intended target
/// </summary>
internal class ABM
{
    #region DELEGATES
    /// <summary>
    /// Invoked when the target is hit by ABM.
    /// </summary>
    public delegate void OnTargetHit(ABM abm);

    /// <summary>
    /// Invoked when elimination occurs for ABM.
    /// </summary>
    /// <param name="abm"></param>
    /// <param name="reason"></param>
    public delegate void OnElimination(ABM abm, string reason);
    #endregion

    #region EVENTS
    /// <summary>
    /// This let's you take action when the target is hit.
    /// </summary>
    public event OnTargetHit? TargetHit;

    /// <summary>
    /// This let's you know when the target is eliminated.
    /// </summary>
    public event OnElimination? Elimination;

    /// <summary>
    /// This let's you know an ICBM was hit.
    /// </summary>
    public event OnTargetHit? RegisterHit;
    #endregion

    /// <summary>
    /// A pseudo random number generator.
    /// </summary>
    readonly Random rand = new();

    /// <summary>
    /// The ABM smoke trail is drawn using this pen. 
    /// </summary>
    protected Pen abmSmokeTrailPen;

    /// <summary>
    /// Location in Missile Command (0-256 x 0-231) PX.
    /// </summary>
    internal PointA LocationInMCCoordinates;

    /// <summary>
    /// Last location ABM was at in Missile Command (0-256 x 0-231) PX.
    /// </summary>
    internal PointA locationLastInMCCoordinates;

    /// <summary>
    /// 
    /// </summary>
    internal List<PointF> abmPathInDeviceCoordinates = new();

    // ====

    /// <summary>
    /// Unique identifier for this ABM. Associated with the neural network having this ID.
    /// </summary>
    readonly private int id = 0;

    /// <summary>
    /// Unique "id" of the ABM, used to associate with a neural network. 
    /// </summary>
    internal int Id
    {
        get { return id; }
    }

    /// <summary>
    /// Indicates the ABM is eliminated if true.
    /// </summary>
    private bool isEliminated = false;

    /// <summary>
    /// Reason why the ABM was eliminated.
    /// </summary>
    private string eliminatedBecauseOf = "";

    /// <summary>
    /// Reason the missile was eliminated.
    /// </summary>
    internal string EliminatedBecauseOf
    {
        get
        {
            return eliminatedBecauseOf;
        }

        set
        {
            if (eliminatedBecauseOf == value) return; // ignore multiple

            eliminatedBecauseOf = value;

            Elimination?.Invoke(this, value);

            isEliminated = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal double LaunchAngle;

    /// <summary>
    /// Detect if the AI should be eliminated, e.g. because it overspeeds, or goes out into space.
    /// </summary>
    internal void DetectElimination()
    {
        if (!string.IsNullOrWhiteSpace(eliminatedBecauseOf)) return;

        // upwards not downwards
        if (LocationInMCCoordinates.MCCoordsToDeviceCoordinates().Y < 10)
        {
            EliminatedBecauseOf = "orbit";
        }
        else
        if (LocationInMCCoordinates.AltitudeInMissileCommandDisplayPX < 0)
        {
            EliminatedBecauseOf = "floor";
        }
        else
        if (LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX > 256 || LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX < 0)
        {
            EliminatedBecauseOf = "edge";
        }
        else if ((this is ABMCPUControlled || this is ABMCPUInTraining) && !((ABMCPUControlled)this).IRsensor.WithinCone)
        {
            EliminatedBecauseOf = "nolock";
        }

        if (!IsEliminated && HasHitTarget())
        {
            EliminatedBecauseOf = "hit";
            TargetHit?.Invoke(this); // <=== not this
        }

        if (!IsEliminated) return;

        RegisterHit?.Invoke(this);
    }

    /// <summary>
    /// Setter/getter for whether ABM is eliminated or not.
    /// </summary>
    internal bool IsEliminated
    {
        get
        {
            return isEliminated;
        }

        set
        {
            if (value == IsEliminated) return;

            isEliminated = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal int LocationTargetX { get; set; }

    /// <summary>
    /// 
    /// </summary>
    internal int LaunchAngleX { get; set; }

    /// <summary>
    /// Detects whether ABM is close-enough to the target
    /// </summary>
    /// <returns></returns>
    internal bool HasHitTarget()
    {
        return DistanceFromActiveTarget() < 10; // radius is because missiles blow up NEAR not actually hit
    }
       
    /// <summary>
    /// Determines how close to target
    /// </summary>
    /// <returns></returns>
    internal virtual float DistanceFromActiveTarget()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Moves the missile applying "guidance" to steer.
    /// </summary>
    internal void Move()
    {
        if (isEliminated) return;  // eliminated don't move.

        GuideMissile(); // OVERRIDE method handles the nuance of CPU vs. USER

        DetectElimination(); // see if player scored a hit, and eliminate.
    }

    /// <summary>
    /// Inherited class provides a mechanism to guides the missile to target. OVERRIDE for CPU and USER.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    internal virtual void GuideMissile()
    {
        throw new NotImplementedException(); // add logic to guide the missile
    }

    /// <summary>
    /// Draws the trailing smoke (line) showing the ABM path.
    /// </summary>
    /// <param name="g"></param>
    internal void DrawPaths(Graphics g)
    {
        if (IsEliminated) return; // don't need to draw

        // if it hasn't moved, don't add to the location list
        if ((int)locationLastInMCCoordinates.HorizontalInMissileCommandDisplayPX != (int)LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX || (int)locationLastInMCCoordinates.AltitudeInMissileCommandDisplayPX != (int)LocationInMCCoordinates.AltitudeInMissileCommandDisplayPX)
        {
            abmPathInDeviceCoordinates.Add(new PointA(LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX,
                                                           LocationInMCCoordinates.AltitudeInMissileCommandDisplayPX + 1).MCCoordsToDeviceCoordinates());

            locationLastInMCCoordinates = new PointA(LocationInMCCoordinates);
        }

        // to draw lines, we need at least TWO points (start+end)
        if (abmPathInDeviceCoordinates.Count > 1)
        {
            g.DrawLines(abmSmokeTrailPen, abmPathInDeviceCoordinates.ToArray()); // these are stored in device coordinates
        }
    }

    /// <summary>
    /// Draws the flashing "warhead" of the ABM. 
    /// </summary>
    /// <param name="g"></param>
    internal virtual void Draw(Graphics g)
    {
        if (IsEliminated) return;

        // colour chosen at random
        using SolidBrush randomColorSquareBrush = new(Color.FromArgb(20 + rand.Next(0, 235), rand.Next(0, 255), rand.Next(0, 255)));

        Point location = LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP();
        Rectangle abmSquare = new(location.X - 1, location.Y - 1, 2, 2);

        g.FillRectangle(randomColorSquareBrush, abmSquare);
    }


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="misileId"></param>
    /// <param name="smokeColour"></param>
    internal ABM(int misileId, Color smokeColour)
    {
        abmSmokeTrailPen = new Pen(Color.FromArgb(smokeColour.R, smokeColour.G, smokeColour.B), 2);

        id = misileId;
        LocationInMCCoordinates = new PointA(0, 0);
        locationLastInMCCoordinates = new PointA(0, 0);
    }

    /// <summary>
    /// 
    /// </summary>
    internal void NotifyHit()
    {
        TargetHit?.Invoke(this);
    }
}
