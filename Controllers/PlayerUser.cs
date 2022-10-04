using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers.Game;
using MissileDefence.Attackers;
using MissileDefence.Defenders;
using MissileDefence.UX;

namespace MissileDefence.Controllers;

/// <summary>
/// Represents a "player" that is a "USER" 
/// </summary>
internal class PlayerUser : Player
{
    /// <summary>
    /// "id" of the next missile fired.
    /// </summary>
    static int s_id = 0;

    /// <summary>
    /// Tracks a list of target locations use has clicked on.
    /// </summary>
    readonly List<UserMissile> userTargetLocations = new();

    /// <summary>
    /// Tracks all the ABMs in flight for the user.
    /// </summary>
    private readonly List<ABMUserControlled> ABMsInFlight = new();

    /// <summary>
    /// Tracks missiles that were inflight that we no longer require. 
    /// </summary>
    private readonly List<ABM> inflightABMsToRemove = new();

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="canvas"></param>
    internal PlayerUser(PictureBox canvas) : base(PlayerMode.human, canvas)
    {
        // the core logic is done in the "base" class
    }

    /// <summary>
    /// Reset the users missiles.
    /// </summary>
    internal void ResetMissiles()
    {
        ABMsInFlight.Clear();
        inflightABMsToRemove.Clear();
        userTargetLocations.Clear();
    }

    /// <summary>
    /// Draws an "X" at each location user has targetted, followed by smoke trails and blobs for inflight ABMs.
    /// </summary>
    /// <param name="g"></param>
    internal override void Draw(Graphics g)
    {
        base.Draw(g);

        // draws an X at each missile target
        using Pen xPen = new(Color.FromArgb(200, RandomNumberGenerator.GetInt32(0, 255), RandomNumberGenerator.GetInt32(0, 255), RandomNumberGenerator.GetInt32(0, 255)), 2);

        foreach (UserMissile missile in userTargetLocations)
        {
            g.DrawLine(xPen, missile.LocationClicked.X - 5, missile.LocationClicked.Y - 5, missile.LocationClicked.X + 5, missile.LocationClicked.Y + 5);
            g.DrawLine(xPen, missile.LocationClicked.X + 5, missile.LocationClicked.Y - 5, missile.LocationClicked.X - 5, missile.LocationClicked.Y + 5);
        }

        // overlay the ABM smoke paths
        foreach (ABMUserControlled missile in ABMsInFlight)
        {
            missile.DrawPaths(g);
        }

        // lastly add a square at the head of the missile
        foreach (ABMUserControlled missile in ABMsInFlight)
        {
            missile.Draw(g);
        }
    }

    /// <summary>
    /// User pressed "1", "2" or "3". We launch an ABM from that "silo" if it has missiles left.
    /// </summary>
    /// <param name="siloIndex"></param>
    internal override void LaunchABM(int siloIndex)
    {
        if (Silos[siloIndex - 1].ABMRemaining == 0) return; // out of missiles

        // If you try to fire more than 8 ABMs at once the game will refuse. 

        if (userTargetLocations.Count == 8) return;

        // track the missile "target" point
        UserMissile userMissile = new()
        {
            LocationClicked = new Point(crossHairLocation.X, crossHairLocation.Y),
            missileLaunched = true
        };

        userTargetLocations.Add(userMissile);

        // launch missile
        Silos[siloIndex - 1].ABMRemaining--; // one less missile at silo

        // create a "user" controlled ABM. These aren't "user" controlled per se, they simply track from silo to target location
        ABMUserControlled uc = new(s_id++, Silos[siloIndex - 1], userMissile.LocationClicked, SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ABMColour, Callback);
        userMissile.abm = uc;
        ABMsInFlight.Add(uc);
    }

    /// <summary>
    /// Check for ABM hitting ICBMs.
    /// </summary>
    /// <param name="abmBeingTested"></param>
    private void Callback(ABM abmBeingTested)
    {
        // whereas we are harsh on the CPU, only allowing it to explode the ICBM it hit, we give the user a chance by exploding any
        // ICBM near the explosion point. Remember ABMs explode near the ICBM, they rarely intend to actually hit it directly.

        // has user hit any ICBMs? check each in turn
        List<Point> hit = new();

        if (GameController.s_currentICBMWaveUSER is null) throw new Exception("wave for user should not be null");

        FlierDefinition? activeFlier = GameController.s_currentICBMWaveUSER.activeFlier;

        foreach (ICBM icbm in GameController.s_currentICBMWaveUSER.ICBMs)
        {
            // not already eliminated, is onscreen and distance is withing a small blast radius.
            if (!icbm.IsEliminated &&
                 icbm.LocationInDeviceCoordinates.Y > 10 &&
                 MathUtils.DistanceBetweenTwoPoints(icbm.LocationInDeviceCoordinates,
                                                    abmBeingTested.LocationInMCCoordinates.MCCoordsToDeviceCoordinates()) < 45)
            {
                icbm.IsEliminated = true;
                explosionManager.Add(icbm.LocationInDeviceCoordinates);
                score += 25 * GameController.Multiplier;
                hit.Add(icbm.LocationInDeviceCoordinates);
            }

            if (activeFlier != null &&
               !activeFlier.isEliminated &&
               MathUtils.DistanceBetweenTwoPoints(activeFlier.location.MCCoordsToDeviceCoordinates(),
                                                  abmBeingTested.LocationInMCCoordinates.MCCoordsToDeviceCoordinates()) < 45)
            {
                activeFlier.isEliminated = true;
                explosionManager.Add(activeFlier.location.MCCoordsToDeviceCoordinatesP());
                score += 100 * GameController.Multiplier;
                hit.Add(activeFlier.location.MCCoordsToDeviceCoordinatesP());
            }
        }

        // chain the explosions. If ABM hit one, see if exploding this one explodes any near it, and near that, and so on.
        while (hit.Count > 0)
        {
            Point oicbm = hit[0];

            foreach (ICBM icbm in GameController.s_currentICBMWaveUSER.ICBMs)
            {
                if (!icbm.IsEliminated &&
                    icbm.LocationInDeviceCoordinates.Y > 10 &&
                    MathUtils.DistanceBetweenTwoPoints(icbm.LocationInDeviceCoordinates,
                                                       oicbm) < 45)
                {
                    icbm.IsEliminated = true;
                    explosionManager.Add(icbm.LocationInDeviceCoordinates);
                    score += 25 * GameController.Multiplier;

                    hit.Add(icbm.LocationInDeviceCoordinates); // causes chaining (ICBM exploding ICBM).
                }

                if (activeFlier != null && !activeFlier.isEliminated &&
                     MathUtils.DistanceBetweenTwoPoints(activeFlier.location.MCCoordsToDeviceCoordinates(),
                                                oicbm) < 45)
                {
                    activeFlier.isEliminated = true;
                    explosionManager.Add(activeFlier.location.MCCoordsToDeviceCoordinatesP());
                    score += 100 * GameController.Multiplier;
                    hit.Add(activeFlier.location.MCCoordsToDeviceCoordinatesP());
                }
            }

            hit.Remove(oicbm);
        }

        // when the ABM explodes we need to remove the ABM from our target locations (the X disappears)
        for (int i = 0; i < userTargetLocations.Count; i++)
        {
            if (userTargetLocations[i].abm == abmBeingTested)
            {
                userTargetLocations.RemoveAt(i);
                break;
            }
        }

        // stop tracking the exploded ABM. These are removed when the next .MoveTrackBallAndFire() executes.
        inflightABMsToRemove.Add(abmBeingTested);
    }

    /// <summary>
    /// Player gets to move the cross hair and fire.
    /// </summary>
    /// <param name="wave"></param>
    internal override void MoveTrackBallAndFire(WaveOfICBMs wave)
    {
        base.MoveTrackBallAndFire(wave);

        foreach (ABMUserControlled abm in ABMsInFlight)
        {
            if (!abm.IsEliminated)
            {
                abm.Move();
                abm.Move();
            }
        }

        foreach (ABMUserControlled abm in inflightABMsToRemove)
        {
            ABMsInFlight.Remove(abm);
        }

        inflightABMsToRemove.Clear();
    }

    /// <summary>
    /// Attaches move/click handlers.
    /// </summary>
    internal override void StartGame()
    {
        base.StartGame();

        // reset everything
        inflightABMsToRemove.Clear();
        userTargetLocations.Clear();
        ABMsInFlight.Clear();

        Canvas.MouseMove += Canvas_MouseMove;
    }

    /// <summary>
    /// Track where the mouse cursor is, and put our "+" cursor at the location.
    /// The place user is pointing at is where an ABM will be launched to, if they 
    /// press one of the fire buttons.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Contains the Mouse position.</param>
    private void Canvas_MouseMove(object? sender, MouseEventArgs e)
    {
        /*
         ; 
         ; Updates the position of the crosshairs, clamping the movement to min/max
         ; values from a table.  We don't want the crosshairs to be up in the score area,
         ; or down with the cities, or wrapping around left and right.

5256: 08           :min_coord      .dd1    8                 ;min X coord
5257: 2d                           .dd1    45                ;min Y coord (above cities)

5258: f7           :max_coord      .dd1    247               ;max X coord
5259: ce                           .dd1    206               ;max Y coord (comfortably below top)
        
          Remember MC has 0 at the BOTTOM, not the top, so we need to invert.
           minY = 45  -> 45 pixels  from bottom = 231-45 (x2 scale) = 372
           maxY = 206 -> 206 pixels from top    = 25 (x2 scale) = 50

        */

        // prevent the user moving the cursor offscreen or outside sensible boundaries (too high/low).
        crossHairLocation.X = e.X.Clamp(16, 494);
        crossHairLocation.Y = e.Y.Clamp(50, 372);
    }
}
