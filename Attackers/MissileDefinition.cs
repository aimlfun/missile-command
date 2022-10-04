namespace MissileDefence.Attackers;

/// <summary>
/// An instance of "ICBMs" (Inter-Continental Ballistic Missile).
/// </summary>
internal class ICBMDefinition
{
    /// <summary>
    /// At what altitude this ICBM will split into multiple war heads.
    /// </summary>
    internal int mirvSplitAtAltitude = -1;

    /// <summary>
    /// Where the ICBM starts from
    /// </summary>
    internal PointA startLocation;

    /// <summary>
    /// "MIRVs" (Multiple Independent Reentry Vehicle).
    /// </summary>
    internal List<int> MIRVTargets = new();

    /// <summary>
    /// 
    /// </summary>
    internal int baseNumberTargettedByMissile;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="startLoc"></param>
    /// <param name="btm"></param>
    /// <param name="splitAlt"></param>
    /// <param name="mirvTargets"></param>
    public ICBMDefinition(PointA startLoc, int btm, List<int> mirvTargets, int splitAlt = -1)
    {
        mirvSplitAtAltitude = splitAlt;
        baseNumberTargettedByMissile = btm;
        startLocation = startLoc;

        MIRVTargets.AddRange(mirvTargets); ;
    }
}
