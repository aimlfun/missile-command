using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence;

/// <summary>
/// Like PointF, but horizontal/altitude rather than x,y
/// </summary>
public class PointA
{
    /// <summary>
    /// The "y" position in Missile Command display units.
    /// </summary>
    public float AltitudeInMissileCommandDisplayPX;

    /// <summary>
    /// The "x" position in Missile Command display units.
    /// </summary>
    public float HorizontalInMissileCommandDisplayPX;

    /// <summary>
    /// New PointA() from X/Y in MC coordinates.
    /// </summary>
    /// <param name="altitudeInMissileCommandDisplayPX"></param>
    /// <param name="horizontalInMissileCommandDisplayPX"></param>
    public PointA(int horizontalInMissileCommandDisplayPX, int altitudeInMissileCommandDisplayPX)
    {
        AltitudeInMissileCommandDisplayPX = altitudeInMissileCommandDisplayPX;
        HorizontalInMissileCommandDisplayPX = horizontalInMissileCommandDisplayPX;
    }

    /// <summary>
    /// Create point from x,y flaot. 
    /// </summary>
    /// <param name="altitudeInMissileCommandDisplayPX"></param>
    /// <param name="horizontalInMissileCommandDisplayPX"></param>
    public PointA(float horizontalInMissileCommandDisplayPX, float altitudeInMissileCommandDisplayPX)
    {
        AltitudeInMissileCommandDisplayPX = altitudeInMissileCommandDisplayPX;
        HorizontalInMissileCommandDisplayPX = horizontalInMissileCommandDisplayPX;
    }

    /// <summary>
    /// Clones a point
    /// </summary>
    /// <param name="point"></param>
    public PointA(PointA point)
    {
        AltitudeInMissileCommandDisplayPX = point.AltitudeInMissileCommandDisplayPX;
        HorizontalInMissileCommandDisplayPX = point.HorizontalInMissileCommandDisplayPX;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pr1"></param>
    /// <param name="pr2"></param>
    /// <returns></returns>
    public static RectangleF MCToDeviceCoordinateRectangle(PointA pr1, PointA pr2)
    {
        PointF p1 = pr1.MCCoordsToDeviceCoordinates();
        PointF p2 = pr2.MCCoordsToDeviceCoordinates();
        return new RectangleF(Math.Min(p1.X,p2.X), Math.Min(p1.Y,p2.Y), Math.Abs(p2.X - p1.X), Math.Abs(p2.Y - p1.Y));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="horizontal1"></param>
    /// <param name="altitude1"></param>
    /// <param name="horizontal2"></param>
    /// <param name="altitude2"></param>
    /// <returns></returns>
    public static RectangleF MCToDeviceCoordinateRectangle(float horizontal1, float altitude1, float horizontal2, float altitude2)
    {
        PointF p1 = (new PointA(horizontal1, altitude1)).MCCoordsToDeviceCoordinates();
        PointF p2 = (new PointA(horizontal2, altitude2)).MCCoordsToDeviceCoordinates();

        return new RectangleF(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="horizontalMC1"></param>
    /// <param name="altitude1"></param>
    /// <param name="horizontalMC2"></param>
    /// <param name="altitudeMC2"></param>
    /// <returns></returns>
    public static RectangleF MCToDeviceCoordinateRectangle(int horizontalMC1, int altitudeMC1, int horizontalMC2, int altitudeMC2)
    {
        PointF p1 = (new PointA(horizontalMC1, altitudeMC1)).MCCoordsToDeviceCoordinates();
        PointF p2 = (new PointA(horizontalMC2, altitudeMC2)).MCCoordsToDeviceCoordinates();

        return new RectangleF(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public PointF MCCoordsToDeviceCoordinates()
    {
//            Debug.WriteLine($"MC: {HorizontalInMissileCommandDisplayPX},{AltitudeInMissileCommandDisplayPX} = DC: {new Point((int)HorizontalInMissileCommandDisplayPX * 2, 2 * ((int)(231 - AltitudeInMissileCommandDisplayPX)))}");

        return new PointF(HorizontalInMissileCommandDisplayPX * 2,
                           2 * (231 - AltitudeInMissileCommandDisplayPX));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Point MCCoordsToDeviceCoordinatesP()
    {
//            Debug.WriteLine($"MC: {HorizontalInMissileCommandDisplayPX},{AltitudeInMissileCommandDisplayPX} = DC: {new Point((int)HorizontalInMissileCommandDisplayPX * 2, 2 * ((int)(231 - AltitudeInMissileCommandDisplayPX)))}");

        return new Point((int)HorizontalInMissileCommandDisplayPX * 2,
                         2 * ((int)(231 - AltitudeInMissileCommandDisplayPX)));
    }

    /// <summary>
    /// Xd is 0..512 (2x pixels)
    /// Yd is 0..462 (2x pixels), but Yd is inverted. 0=top, 462=bottom
    /// Xmc is 0..256
    /// Ymc is 0..231, with 0 at the bottom
    /// 
    /// Xmc = Xd/2
    /// Ymc = 231-Yd/2
    /// 
    /// Xd = Xmc*2
    /// Yd = (231-Ymc)*2
    /// </summary>
    /// <returns></returns>
    public static PointA DeviceCoordinatesToMC(Point p)
    {
//          Debug.WriteLine($"DC: {p} = mc: {new PointA(p.X / 2, 231 - p.Y / 2)}");
        return new PointA(p.X / 2, 231 - p.Y / 2);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pr1"></param>
    /// <param name="pr2"></param>
    /// <returns></returns>
    public static Rectangle ToRectangleMCInDeviceCoordinates(PointA pr1, PointA pr2)
    {
        PointF p1 = pr1.MCCoordsToDeviceCoordinates();
        PointF p2 = pr2.MCCoordsToDeviceCoordinates();

        return new Rectangle((int)Math.Round(p1.X),
                             (int)Math.Round(p1.Y),
                             (int)Math.Round(p2.X - p1.X),
                             (int)Math.Round(p2.Y - p1.Y));
    }

    public override string? ToString()
    {
        return $"({this.HorizontalInMissileCommandDisplayPX},{this.AltitudeInMissileCommandDisplayPX})";
    }
}
