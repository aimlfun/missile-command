using MissileDefence.Controllers.Game;
using MissileDefence.Controllers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.UX;

namespace MissileDefence.Defenders
{
    internal class ABMUserControlled : ABM
    {
        /// <summary>
        /// Computes the next point along the path.
        /// Users point to the target, and the missile streaks from base to target using Bresenham's line algorithm.
        /// </summary>
        readonly BresenhamLine lineData;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="missileId"></param>
        /// <param name="launchedFromSilo">Which silo the ABM was launched from.</param>
        /// <param name="cursorClickedLocation">The location the ABM is targetting.</param>
        /// <param name="smokeColour">The colour of the ABM smoke.</param>
        /// <param name="callbackUponReachingEndPoint"></param>
        internal ABMUserControlled(int missileId, ABMSilo launchedFromSilo, Point cursorClickedLocation, Color smokeColour, OnTargetHit callbackUponReachingEndPoint) : base(missileId, smokeColour)
        {
            TargetHit += callbackUponReachingEndPoint;
         
            // we need to know where they clicked, so we convert cursor pos to screen location.
            LocationInMCCoordinates = new PointA(launchedFromSilo.LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX, 20);
            locationLastInMCCoordinates = LocationInMCCoordinates;

            Point p = LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP();

            lineData = new BresenhamLine(p, cursorClickedLocation);
        }

        /// <summary>
        /// Determines how close to target
        /// </summary>
        /// <returns></returns>
        internal override float DistanceFromActiveTarget()
        {
            // dummy value, it will be invoked for user, but we aren't tracking any specific
            // target. We are simply streaking across the sky to the point they marked.
            return int.MaxValue;
        }

        /// <summary>
        /// User guidance is simply use Bresenham's line algorithm to move to the next
        /// point along the line until we reach the end-point
        /// </summary>
        internal override void GuideMissile()
        {
            // to speed up the flight of missiles, we compute it multiple times
            for (int i = 0; i < 6; i++)
            {
                if (lineData.NextPoint(out Point p))
                {
                    // we explode at the "end point"
                    GameController.s_players[1].explosionManager.Add(p);

                    SeeIfMissileHitAnyTargets();
                    return;
                }
                else
                {
                    LocationInMCCoordinates = PointA.DeviceCoordinatesToMC(p);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SeeIfMissileHitAnyTargets()
        {
            NotifyHit();
        }
    }
}
