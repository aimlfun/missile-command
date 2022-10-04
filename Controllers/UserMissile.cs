using MissileDefence.Defenders;

namespace MissileDefence.Controllers;

/// <summary>
/// Represents a user-missile.
/// </summary>
internal class UserMissile
{
    /// <summary>
    /// Tracks where the user clicked (point missile travels to)
    /// </summary>
    internal Point LocationClicked;
    
    /// <summary>
    /// true - the missiled has been launched.
    /// </summary>
    internal bool missileLaunched = false;
    
    /// <summary>
    /// The ABM this user missile represents.
    /// </summary>
    internal ABMUserControlled? abm;
}
