using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence;

/// <summary>
/// Maths related utility functions.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static bool PtInTriangle2(PointF p, PointF p0, PointF p1, PointF p2)
    {
        float dX = p.X - p2.X;
        float dY = p.Y - p2.Y;
        float dX21 = p2.X - p1.X;
        float dY12 = p1.Y - p2.Y;
        float D = dY12 * (p0.X - p2.X) + dX21 * (p0.Y - p2.Y);
        float s = dY12 * dX + dX21 * dY;
        float t = (p2.Y - p0.Y) * dX + (p0.X - p2.X) * dY;

        if (D < 0) return s <= 0 && t <= 0 && s + t >= D;

        return s >= 0 && t >= 0 && s + t <= D;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointToTestIsInTriangle"></param>
    /// <param name="triangleVertex1"></param>
    /// <param name="triangleVertex2"></param>
    /// <param name="triangleVertex3"></param>
    /// <returns></returns>
    public static bool PtInTriangle(PointA pointToTestIsInTriangle, PointA triangleVertex1, PointA triangleVertex2, PointA triangleVertex3)
    {
        double det = (triangleVertex2.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX) * (triangleVertex3.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) - (triangleVertex2.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) * (triangleVertex3.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX);

        return det * ((triangleVertex2.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) - (triangleVertex2.AltitudeInMissileCommandDisplayPX - triangleVertex1.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex1.HorizontalInMissileCommandDisplayPX)) >= 0 &&
               det * ((triangleVertex3.HorizontalInMissileCommandDisplayPX - triangleVertex2.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex2.AltitudeInMissileCommandDisplayPX) - (triangleVertex3.AltitudeInMissileCommandDisplayPX - triangleVertex2.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex2.HorizontalInMissileCommandDisplayPX)) >= 0 &&
               det * ((triangleVertex1.HorizontalInMissileCommandDisplayPX - triangleVertex3.HorizontalInMissileCommandDisplayPX) * (pointToTestIsInTriangle.AltitudeInMissileCommandDisplayPX - triangleVertex3.AltitudeInMissileCommandDisplayPX) - (triangleVertex1.AltitudeInMissileCommandDisplayPX - triangleVertex3.AltitudeInMissileCommandDisplayPX) * (pointToTestIsInTriangle.HorizontalInMissileCommandDisplayPX - triangleVertex3.HorizontalInMissileCommandDisplayPX)) >= 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    public static double Clamp360(double angleInDegrees)
    {
        if (angleInDegrees < 0) angleInDegrees += 360;
        if (angleInDegrees >= 360) angleInDegrees -= 360;
        
        return angleInDegrees;
    }

    /// <summary>
    /// Determine a point rotated by an angle around an origin.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="origin"></param>
    /// <param name="angleInDegrees"></param>
    /// <returns></returns>
    internal static PointF RotatePointAboutOrigin(PointF point, PointF origin, double angleInDegrees)
    {
        return RotatePointAboutOriginInRadians(point, origin, DegreesInRadians(angleInDegrees));
    }

    /// <summary>
    /// Determine a point rotated by an angle around an origin.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="origin"></param>
    /// <param name="angleInRadians"></param>
    /// <returns></returns>
    public static PointF RotatePointAboutOriginInRadians(PointF point, PointF origin, double angleInRadians)
    {
        double cos = Math.Cos(angleInRadians);
        double sin = Math.Sin(angleInRadians);
        float dx = point.X - origin.X;
        float dy = point.Y - origin.Y;

        // standard maths for rotation.
        return new PointF((float)(cos * dx - sin * dy + origin.X),
                          (float)(sin * dx + cos * dy + origin.Y)
        );
    }

    /// <summary>
    /// Logic requires radians but we track angles in degrees, this converts.
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public static double DegreesInRadians(double angle)
    {
       // if (angle < 0 || angle > 360) Debugger.Break();

        return (double)Math.PI * angle / 180;
    }

    /// <summary>
    /// Converts radians into degrees. 
    /// One could argue, WHY not just use degrees? Preference. Degrees are more intuitive than 2*PI offset values.
    /// </summary>
    /// <param name="radians"></param>
    /// <returns></returns>
    public static double RadiansInDegrees(double radians)
    {
        // radians = PI * angle / 180
        // radians * 180 / PI = angle
        return radians * 180F / Math.PI;
    }

    /// <summary>
    /// Computes the distance between 2 points using Pythagoras's theorem a^2 = b^2 + c^2.
    /// </summary>
    /// <param name="pt1">First point.</param>
    /// <param name="pt2">Second point.</param>
    /// <returns></returns>
    public static float DistanceBetweenTwoPoints(PointF pt1, PointF pt2)
    {
        float dx = pt2.X - pt1.X;
        float dy = pt2.Y - pt1.Y;

        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Computes the distance between 2 points using Pythagoras's theorem a^2 = b^2 + c^2.
    /// </summary>
    /// <param name="pt1">First point.</param>
    /// <param name="pt2">Second point.</param>
    /// <returns></returns>
    public static float DistanceBetweenTwoPoints(PointA pt1, PointA pt2)
    {
        float dx = pt2.HorizontalInMissileCommandDisplayPX - pt1.HorizontalInMissileCommandDisplayPX;
        float dy = pt2.AltitudeInMissileCommandDisplayPX - pt1.AltitudeInMissileCommandDisplayPX;

        return (float)Math.Sqrt(dx * dx + dy * dy);
    }
    
    /// <summary>
    /// Returns a value between min and max (never outside of).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }

        if (val.CompareTo(max) > 0)
        {
            return max;
        }

        return val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int Clamp(int val, int min, int max)
    {
        if (val.CompareTo(min) < 0)
        {
            return min;
        }

        if (val.CompareTo(max) > 0)
        {
            return max;
        }

        return val;
    }
}
