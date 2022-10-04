namespace MissileDefence.Attackers;

/// <summary>
/// Bomber/Satellite definition.
/// </summary>
internal class FlierDefinition
{
    /// <summary>
    /// The types of fliers we support.
    /// </summary>
    internal enum FlierTypes { bomber, satellite };

    /// <summary>
    /// Represents whether the flier is eliminated or not.
    /// </summary>
    internal bool isEliminated = false;

    /// <summary>
    /// Indicator whether we are a bomber or satellite.
    /// </summary>
    internal FlierTypes bomberOrSatellite;

    /// <summary>
    /// Where the "flier" is on screen.
    /// </summary>
    internal PointA location = new(0, 0);

    /// <summary>
    /// How much the flier moves each frame (l2r -> +2 or +3, r2l -> -2 or -3). 
    /// Bombers move quicker than satellites.
    /// </summary>
    internal int deltaX;

    /// <summary>
    /// We have to wait a certain amount of time before introducing a flier - this 
    /// controls that timing.
    /// </summary>
    internal int framesRenderedBeforeAppearing;

    /// <summary>
    /// Tracks the frame it is on.
    /// </summary>
    internal int frames = 0;

    /// <summary>
    /// Assigned flier (bomber/satellite image; either l2r or r2l).
    /// </summary>
    internal Bitmap? image;

    /// <summary>
    /// List of bombs to drop: index on xoffset, with baseTargetted (0-9 = 6 cities + 3 silos).
    /// </summary>
    internal Dictionary<int, int> icbmDrop = new();

    /// <summary>
    /// We need these to exist for both players, and if we don't deep clone, they will
    /// share the objects like location. That would be OK (same on both) but the icbmDrop 
    /// is not due to the mechanism that we remove from it as we drop leading to 2nd
    /// player missing the dropped ordnance.
    /// </summary>
    /// <returns></returns>
    internal FlierDefinition Clone()
    {
        FlierDefinition newDef = new();
        
        newDef.icbmDrop = new(this.icbmDrop);
        newDef.location = new(this.location);
        newDef.image = this.image;
        newDef.deltaX = this.deltaX;
        newDef.framesRenderedBeforeAppearing = this.framesRenderedBeforeAppearing;
        newDef.frames = this.frames;
        newDef.bomberOrSatellite = this.bomberOrSatellite;

        return newDef;
    }
}
