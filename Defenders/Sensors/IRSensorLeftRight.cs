using MissileDefence.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Defenders.Sensors;

/// <summary>
/// An infra-red left..right sensor
/// </summary>
internal class IRSensorLeftRight
{
    internal static float c_nolock = -2;

    /// <summary>
    /// true indicates ICBM is within the sensor.
    /// </summary>
    internal bool WithinCone = false;

    /// <summary>
    /// Points that make up the sensor sweep triangle. So we can plot it.
    /// </summary>
    private readonly List<PointF[]> heatSensorSweepTrianglePolygonsInDeviceCoordinates = new();

    /// <summary>
    /// Points that make up the sensor target triangle. So we can plot it.
    /// </summary>
    private readonly List<PointF[]> heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates = new();

    /// <summary>
    /// Returns TRUE if the target is within the sweep.
    /// </summary>
    internal bool IsInSensorSweepTriangle
    {
        get
        {
            return heatSensorSweepTrianglePolygonsInDeviceCoordinates.Count > 0;
        }
    }
  
    /// <summary>
    /// Resets the heatmap debug images.
    /// </summary>
    internal void Clear()
    {
        heatSensorSweepTrianglePolygonsInDeviceCoordinates.Clear();
        heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates.Clear();
    }

    /// <summary>
    /// Read the infrared sensor to detect a SINGLE "ICBM".
    /// Whilst "we" know the location, the sensor plays dumb. I cannot simulate a real heat sensor despite
    /// warming my GPU up running this code. What's important is that the neural network cannot cheat.
    /// </summary>
    /// <param name="angleCentreToSweep">Where the ABM is pointing</param>
    /// <param name="objectMCLocation">Where the ABM is on screen.</param>
    /// <param name="targetMCLocation">Where the ICBM is on screen.</param>
    /// <param name="heatSensorRegionsOutput">Sensor output.</param>
    /// <returns></returns>
    internal double[] Read(double angleCentreToSweep, PointA objectMCLocation, PointA targetMCLocation, out double[] heatSensorRegionsOutput)
    {
        heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates.Clear();
        heatSensorSweepTrianglePolygonsInDeviceCoordinates.Clear();
        WithinCone = false;

        heatSensorRegionsOutput = new double[(int)Settings.s_sensor.SamplePoints];

        // e.g 
        // input to the neural network
        //   _ \ | / _   
        //   0 1 2 3 4 
        //        
        double fieldOfVisionStartInDegrees = Settings.s_sensor.FieldOfVisionStartInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   [-] this
        double sensorVisionAngleInDegrees = Settings.s_sensor.VisionAngleInDegrees;

        //   _ \ | / _   
        //   0 1 2 3 4
        //   ^ this
        double sensorAngleToCheckInDegrees = fieldOfVisionStartInDegrees - sensorVisionAngleInDegrees / 2 + angleCentreToSweep;

        // how far ahead we look. Given this could be diagonal across the screen, it needs to be sufficient
        double DepthOfVisionInPixels = 700; // needs to cover whole screen during training

        for (int LIDARangleIndex = 0; LIDARangleIndex < Settings.s_sensor.SamplePoints; LIDARangleIndex++)
        {
            //     -45  0  45
            //  -90 _ \ | / _ 90   <-- relative to direction of missile, hence + angle missile is pointing
            double LIDARangleToCheckInRadiansMin = MathUtils.DegreesInRadians(sensorAngleToCheckInDegrees);
            double LIDARangleToCheckInRadiansMax = LIDARangleToCheckInRadiansMin + MathUtils.DegreesInRadians(sensorVisionAngleInDegrees);

            /*  p1        p2
             *   +--------+
             *    \      /
             *     \    /     this is our imaginary "heat sensor" triangle
             *      \  /
             *       \/
             *    abmLocation
             */
            PointA p1 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + objectMCLocation.HorizontalInMissileCommandDisplayPX),
                            (float)(Math.Cos(LIDARangleToCheckInRadiansMin) * DepthOfVisionInPixels + objectMCLocation.AltitudeInMissileCommandDisplayPX));

            PointA p2 = new((float)(Math.Sin(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + objectMCLocation.HorizontalInMissileCommandDisplayPX),
                            (float)(Math.Cos(LIDARangleToCheckInRadiansMax) * DepthOfVisionInPixels + objectMCLocation.AltitudeInMissileCommandDisplayPX));

            heatSensorSweepTrianglePolygonsInDeviceCoordinates.Add(new PointF[] { objectMCLocation.MCCoordsToDeviceCoordinates(),
                                                                                  p1.MCCoordsToDeviceCoordinates(),
                                                                                  p2.MCCoordsToDeviceCoordinates() });

            if (MathUtils.PtInTriangle(targetMCLocation, objectMCLocation, p1, p2))
            {
                WithinCone = true;
                double mult;

                if ((int)(Settings.s_sensor.SamplePoints - 1) / 2 == LIDARangleIndex)
                {
                    // favour center
                    mult = 0;
                }
                else
                {
                    double dist = MathUtils.DistanceBetweenTwoPoints(objectMCLocation, targetMCLocation);
                 
                    //     mult
                    // 0 = +2
                    // 1 = +1  }
                    // 2 => 0  } Sample Points = 5.
                    // 3 = +1  }
                    // 4 = +2

                    mult = Math.Abs(LIDARangleIndex - (Settings.s_sensor.SamplePoints - 1) / 2F);

                    //      / sample points     
                    //  2/5 = 0.4              2/2 = 1                         3/6 = 
                    //  1/5 = 0.2              1/2 = 0.5                       2/6
                    //  0/5 =                                                  1/6
                    //  1/5 =
                    //  2/5 =
                    mult /= (Settings.s_sensor.SamplePoints - 1) / 2F;
                    mult *= 220 / dist;
                }

                heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates.Add(new PointF[] { objectMCLocation.MCCoordsToDeviceCoordinates(),
                                                                                           p1.MCCoordsToDeviceCoordinates(),
                                                                                           p2.MCCoordsToDeviceCoordinates()
                                                                                          });

                heatSensorRegionsOutput[LIDARangleIndex] = mult;
            }
            else
            {
                heatSensorRegionsOutput[LIDARangleIndex] = c_nolock; // no target in this direction
            }

            //   _ \ | / _         _ \ | / _   
            //   0 1 2 3 4         0 1 2 3 4
            //  [-] from this       [-] to this
            sensorAngleToCheckInDegrees += sensorVisionAngleInDegrees;
        }

        // return where we sensed the ICBM if present
        return heatSensorRegionsOutput;
    }

    /// <summary>
    /// Draws the full triangle sweep range.
    /// +--------+
    ///  \      /
    ///   \    /     this is our imaginary "heat sensor" triangle
    ///    \  /
    ///     \/
    ///     ABM
    /// </summary>
    /// <param name="g"></param>
    /// <param name="triangleSweepColour"></param>
    internal void DrawFullSweepOfHeatSensor(Graphics g, Color triangleSweepColour)
    {
        using SolidBrush brushOrange = new(triangleSweepColour);

        foreach (PointF[] point in heatSensorSweepTrianglePolygonsInDeviceCoordinates) g.FillPolygon(brushOrange, point);
    }

    /// <summary>
    /// Draws the region of the sweep that the target is in.
    /// +---++---+
    ///  \  ||  /
    ///   \ || /     hopefully the center strip
    ///    \||/
    ///     \/
    ///     ABM
    /// </summary>
    /// <param name="g"></param>
    internal void DrawWhereTargetIsInRespectToSweepOfHeatSensor(Graphics g, SolidBrush sbColor)
    {
        // draw the heat sensor
        foreach (PointF[] point in heatSensorTriangleTargetIsInPolygonsInDeviceCoordinates) g.FillPolygon(sbColor, point);
    }
}
