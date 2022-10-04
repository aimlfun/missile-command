using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Controllers;
using MissileDefence.Controllers.Game;
using MissileDefence.UX;

namespace MissileDefence.Attackers
{
    /// <summary>
    /// Represents a wave of ICBMs.
    /// </summary>
    internal class WaveOfICBMs
    {
        /// <summary>
        /// Fliers in this wave with their start position / timing
        /// </summary>
        internal readonly Dictionary<int, FlierDefinition> Fliers = new();

        /// <summary>
        /// The active flier on this wave
        /// </summary>
        internal FlierDefinition? activeFlier = null;

        /// <summary>
        /// The list of ICBMs for the wave
        /// </summary>
        internal readonly List<ICBM> ICBMs = new();

        /// <summary>
        /// Which player the wave belongs to.
        /// </summary>
        private readonly Player player;

        /// <summary>
        /// true - the wave is complete.
        /// </summary>
        internal bool WaveComplete = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="level"></param>
        internal WaveOfICBMs(Player playerWaveIsFor)
        {
            player = playerWaveIsFor;
        }

        /// <summary>
        /// Initiates the wave, creating the missiles.
        /// </summary>
        /// <param name="definition"></param>
        internal void InitiateWave(List<ICBMDefinition> definition, List<FlierDefinition> flierDefinition)
        {
            foreach (ICBMDefinition m in definition)
            {
                ICBMs.Add(new ICBM(m.startLocation,
                                      player.AllInfrastructureBeingDefended[m.baseNumberTargettedByMissile],
                                      m.mirvSplitAtAltitude,
                                      m.MIRVTargets,
                                      player.BaseDestroyed,
                                      SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour));
            }

            // prepare all the fliers 
            Fliers.Clear();

            foreach (FlierDefinition f in flierDefinition)
            {
                f.image = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].Flier[f.bomberOrSatellite][f.deltaX];

                FlierDefinition fnew = f.Clone();
                // assign the image based on the direction
                fnew.location.AltitudeInMissileCommandDisplayPX -= (f.bomberOrSatellite == FlierDefinition.FlierTypes.bomber ? 11 : 13);

                // add the flier, unless by luck (for player) more than one have the same height.
                if (!Fliers.ContainsKey(f.framesRenderedBeforeAppearing)) Fliers.Add(f.framesRenderedBeforeAppearing, fnew);
            }
        }

        /// <summary>
        /// Moves the ICBMs and fliers.
        /// </summary>
        internal void MoveAll()
        {
            List<ICBM> newMissiles = new();

            WaveComplete = true;

            foreach (ICBM missile in ICBMs)
            {
                missile.Move();

                if (!missile.IsEliminated)
                {
                    WaveComplete = false; // at least one active ICBM

                    if (missile.splitAtAltitude == missile.LocationInDeviceCoordinates.Y && missile.LocationInDeviceCoordinates.Y > 0)
                    {
                        missile.splitAtAltitude = -1;

                        foreach (int baseTargetNumber in missile.MIRVTargets)
                        {
                            newMissiles.Add(new ICBM(PointA.DeviceCoordinatesToMC(missile.LocationInDeviceCoordinates),
                                                     player.AllInfrastructureBeingDefended[baseTargetNumber],
                                                     -1,
                                                     new List<int>(),
                                                     player.BaseDestroyed,
                                                     SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour));
                        }
                    }
                }
            }

            // can't modify a collection within a foreach, so we add here.
            ICBMs.AddRange(newMissiles);
        }

        /// <summary>
        /// Moves any flier on screen.
        /// </summary>
        internal void MoveFlier()
        {
            if ((activeFlier is null || activeFlier.isEliminated) && Fliers.ContainsKey(GameController.s_framesRendered))
            {
                activeFlier = Fliers[GameController.s_framesRendered];
            }

            // no flier,no moving one
            if (activeFlier is null || activeFlier.isEliminated) return;

            // move every 3 or 2 frames depending on type
            ++activeFlier.frames;

            int frameToMove = (activeFlier.bomberOrSatellite == FlierDefinition.FlierTypes.bomber) ? 3 : 2;

            if (activeFlier.frames < frameToMove) return;

            activeFlier.location.HorizontalInMissileCommandDisplayPX += activeFlier.deltaX * 2;
            activeFlier.frames = 0;

            if (activeFlier.location.HorizontalInMissileCommandDisplayPX < 0 || activeFlier.location.HorizontalInMissileCommandDisplayPX > 255)
            {
                activeFlier.isEliminated = true;
                activeFlier = null; // create a new one shortly
            }
            else
            {
                int bombReleased = -1;
                foreach (int xpos in activeFlier.icbmDrop.Keys)
                {
                    if (Math.Abs(activeFlier.location.HorizontalInMissileCommandDisplayPX - xpos) < 3)
                    {
                        bombReleased = xpos;
                        ICBMs.Add(new ICBM(activeFlier.location,
                                            player.AllInfrastructureBeingDefended[activeFlier.icbmDrop[xpos]],
                                            -1,
                                            new List<int>(),
                                            player.BaseDestroyed,
                                            SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour));
                    }
                }

                if (bombReleased != -1) activeFlier.icbmDrop.Remove(bombReleased);
            }
        }

        /// <summary>
        /// Draws ICBMs and "Flier".
        /// </summary>
        /// <param name="g"></param>
        internal void DrawAll(Graphics g)
        {
            foreach (ICBM icbm in ICBMs)
            {
                if (!icbm.IsEliminated && icbm.LocationInDeviceCoordinates.Y >= 0) icbm.Draw(g);
            }

            if (activeFlier is not null && !activeFlier.isEliminated)
            {
                if (activeFlier.image is null) throw new Exception("flier without image?");

                Point p = activeFlier.location.MCCoordsToDeviceCoordinatesP();
                p.X -= 15;
                p.Y -= 11;
                g.DrawImageUnscaled(activeFlier.image, p);
            }

            g.Flush();
        }
    }
}