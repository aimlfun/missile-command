using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.Configuration;
using MissileDefence.Controllers.Game;
using MissileDefence.Attackers;
using MissileDefence.UX;

namespace MissileDefence.Defenders
{
    /// <summary>
    /// Implementation of a missile base, with triangle of missiles.
    /// "ABMs" are Anti-Ballistic Missile.
    /// </summary>
    internal class ABMSilo : BasesBeingDefended
    {

        /*
            ; There are 10 ABMs at each launcher site.  As they are fired, the visual
            ; representation is updated in this order:
            ;        9
            ;      8   7
            ;    6   5   4
            ;  1   3   2   0
            ; 
                681a: 00 fd 03 fa+ abm_xoffset_tbl .bulk   $00,$fd,$03,$fa,$00,$06,$fd,$03,$f7,$09
                6810: 02 ff ff fc+ abm_yoffset_tbl .bulk   $02,$ff,$ff,$fc,$fc,$fc,$f9,$f9,$f9,$f9
                                                            9   8   7   6   5   4   3   2   1   0

                mapping: f7 = 247 = -9 | f9 = 249 = -7 | fa = 250 = -6 | fc = 252 = -4 | fd = 253 = -3 | ff = 255 = -1

                          0,  2 

                     -4, -1     3, -1

                -6, -4     0, -4     6, -4
        
            -3, -7     3, -7     -9, -7      9, -7
        
              ^3        ^2         ^1         ^0

         */

        /// <summary>
        /// Missiles are positioned in a triangle. We remove them from the bottom first.
        /// </summary>
        private readonly static Point[] abmMissileOffsetsInMCCoordinates = new Point[10] {
                                             new Point(0,2),
                                    new Point(-3,-1), new Point(3,-1),
                              new Point(-6,-4), new Point(0,-4), new Point(6,-4),
                      new Point(-3,-7), new Point(3,-7), new Point(-9,-7), new Point(9,-7) };
        
        /// <summary>
        /// How many ABMs are left in this silo.
        /// </summary>
        internal int ABMRemaining;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="horizInMCCoordinates"></param>
        /// <param name="altitudeInMCCoordinates"></param>
        /// <param name="isMissileBase"></param>
        internal ABMSilo(int horizInMCCoordinates, int altitudeInMCCoordinates) : base(horizInMCCoordinates, altitudeInMCCoordinates)
        {
            Reset();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abmBase"></param>
        /// <param name="icbm"></param>
        /// <returns></returns>
        internal static double LaunchAngle(ABMSilo abmBase, ICBM icbm)
        {
            PointA icbmLocation = PointA.DeviceCoordinatesToMC(icbm.LocationInDeviceCoordinates);

            float dx = icbmLocation.HorizontalInMissileCommandDisplayPX - abmBase.LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX;
            float dy = icbmLocation.AltitudeInMissileCommandDisplayPX - abmBase.LocationInMCCoordinates.AltitudeInMissileCommandDisplayPX;

            double dist = Math.Sqrt(dx * dx + dy * dy);

            double angleInDegrees = MathUtils.RadiansInDegrees(Math.Asin(dx / dist));

            // adjust to point it more upward as gravity will drop the path

            if (angleInDegrees > 45) angleInDegrees -= Settings.s_sensor.FieldOfVisionStopInDegrees;
            if (angleInDegrees < -45) angleInDegrees += Settings.s_sensor.FieldOfVisionStopInDegrees;

            angleInDegrees = MathUtils.Clamp360(angleInDegrees);

            return angleInDegrees;
        }

        /// <summary>
        /// Draw either "OUT" under the missile base, or a stacked triangle of remaining missiles.
        /// </summary>
        /// <param name="g"></param>
        internal override void Draw(Graphics g)
        {
            base.Draw(g);

            Point pMissile = locationInMCcoordinates.MCCoordsToDeviceCoordinatesP();

            if (IsDestroyed || ABMRemaining == 0)
            {
                /*
                                   bne     :SkipText2        ;if not zero, don't print "OUT"
530c: a5 93                        lda     play_mode_flag    ;game in progress?
530e: f0 04                        beq     :SkipText2        ;no, don't show "OUT"
5310: 8a                           txa                       ;put silo number in A-reg
5311: 20 36 68                     jsr     PrintLauncherOut  ;print "OUT"
5314: 8a           :SkipText2      txa                       ;put silo number in A-reg */

                WriteUnderneathBase(g, pMissile, "OUT");
                return;
            }

            if (!IsDestroyed)
            {
                DrawTriangleOfMissilesBasedOnRemainingCount(g, pMissile);

                /*
                                   lda     abms_left,x       ;get number of ABMs remaining in silo
52f6: c9 04                        cmp     #$04              ;down to 4?
52f8: d0 09                        bne     :Not4             ;no, branch
52fa: 8a                           txa                       ;put silo number in A-reg
52fb: 20 24 68                     jsr     PrintLauncherLow  ;print "LOW"
                 */

                if (ABMRemaining <= 4) WriteUnderneathBase(g, pMissile, "LOW");
            }
        }

        /// <summary>
        /// Write "OUT" underneath the base.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pMissile"></param>
        /// <param name="text"></param>
        private static void WriteUnderneathBase(Graphics g, Point pMissile, string text)
        {
            if (GameController.State != GameController.PossibleStatesForTheStateMachine.PlayGame) return; // don't show

            using Font f2 = new("Lucida Console", 13, FontStyle.Bold);

            using SolidBrush textColor = new(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ABMColour);

            SizeF size = g.MeasureString(text, f2);
            g.DrawString(text, f2, textColor, pMissile.X - size.Width / 2, 462 - 17);
        }

        /// <summary>
        /// Draw stacked missile triangle.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pMissile"></param>
        private void DrawTriangleOfMissilesBasedOnRemainingCount(Graphics g, Point pMissile)
        {
            for (int abmAmmoIconIndex = 0; abmAmmoIconIndex < ABMRemaining; abmAmmoIconIndex++)
            {
                Bitmap abmAmmoIcon = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].Missile;

                int x = pMissile.X + 2 * abmMissileOffsetsInMCCoordinates[abmAmmoIconIndex].X - 3;
                int y = pMissile.Y - 2 * (abmMissileOffsetsInMCCoordinates[abmAmmoIconIndex].Y - 3);

                /*
                   ; 
                   ; Draws an ABM ammo icon.
                   ; 
                   ; 
                   ;    #
                   ;    #
                   ;    #
                   ;   ###
                   ;   # #
                   ; 
                 */
                g.DrawImageUnscaled(abmAmmoIcon, x, y);
            }
        }

        /// <summary>
        /// This missile silo has been destroyed.
        /// </summary>
        internal override void BaseDestroyed()
        {
            base.BaseDestroyed();
            ABMRemaining = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Reset()
        {
            ABMRemaining = 10; // missiles
        }

    }
}
