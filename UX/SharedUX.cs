using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.UX;

/// <summary>
/// Handles pre-loading of all the required images, ready for display on demand.
/// </summary>
internal static class SharedUX
{
    // Missile Command's display has some unusual characteristics. 
    // The display width is a friendly 256 pixels wide, but the height is an odd 231 lines (4:3.61 ratio). 

    /// <summary>
    /// The width of the original arcade screen.
    /// </summary>
    internal const int c_MissileCommandVideoWidthPX = 256;

    /// <summary>
    /// The height of the original arcade screen.
    /// </summary>
    internal const int c_MissileCommandVideoHeightPX = 231;

    /// <summary>
    /// Where all the image assets are located.
    /// </summary>
    internal const string c_assetPath = @"UX\Assets\";

    /// <summary>
    /// Bitmap containing the "floor" with "humps" where the bases are. It is sized per MC dimensions.
    /// </summary>
    internal static Bitmap s_floorBaseImage256x26 = new(Path.Combine(c_assetPath, "floor512x52.png"));

    /// <summary>
    /// Bitmap representing the cities (2 colour).
    /// </summary>
    internal static Bitmap s_cityBaseImage = new(Path.Combine(c_assetPath, "city30x14.png"));

    /// <summary>
    /// Bitmap of the ammo that appears in a triangle on each base.
    /// </summary>
    internal static Bitmap s_abmAmmoImage = new(Path.Combine(c_assetPath, "missile6x10.png"));

    /// <summary>
    /// Bitmap for the "down" arrow under "DEFEND CITIES".
    /// </summary>
    internal static Bitmap s_arrow = new(Path.Combine(c_assetPath, "down-arrow-16x18.png"));

    /// <summary>
    /// Bitmap for arrow pointing left.
    /// </summary>
    internal static Bitmap s_leftArrow = new(Path.Combine(c_assetPath, "left-arrow-18x16.png"));

    /// <summary>
    /// Bitmap for arrow pointing right.
    /// </summary>
    internal static Bitmap s_rightArrow = new(Path.Combine(c_assetPath, "right-arrow-18x16.png"));

    /// <summary>
    /// Bitmap representing the bomber left to right.
    /// </summary>
    internal static Bitmap s_bomberL2RBaseImage = new(Path.Combine(c_assetPath, "flier-bomber-l2r-34x22.png"));

    /// <summary>
    /// Bitmap representing the bomber right to left.
    /// </summary>
    internal static Bitmap s_bomberR2LBaseImage = new(Path.Combine(c_assetPath, "flier-bomber-r2l-34x22.png"));

    /// <summary>
    /// Bitmap representing the satellite left to right.
    /// </summary>
    internal static Bitmap s_satelliteL2RBaseImage = new(Path.Combine(c_assetPath, "flier-satellite-l2r-26x26.png"));

    /// <summary>
    /// Bitmap representing the satellite right to left.
    /// </summary>
    internal static Bitmap s_satelliteR2LBaseImage = new(Path.Combine(c_assetPath, "flier-satellite-r2l-26x26.png"));
    
    /// <summary>
    /// Bitmap for the "red" blocky "MISSILE COMMAND" text.
    /// </summary>
    internal static Bitmap s_missileCommandLogo = new(Path.Combine(c_assetPath, "MissileCommand.png"));

    /// <summary>
    /// Array of different coloured logos (we recolour "MISSILE COMMAND" text).
    /// </summary>
    internal static Bitmap[] s_missileCommandLogos = new Bitmap[10];

    /// <summary>
    /// There are 20 distinct levels (that repeat as the user goes past 20), all the settings for them are 
    /// tracked as a "LevelUX".
    /// </summary>
    internal static Dictionary<int, LevelUX> s_uxColorsPerLevel = new();

    /// <summary>
    /// Initialises everything, colouring the logos etc
    /// </summary>
    internal static void Initialise()
    {
        s_arrow.MakeTransparent();


        List<Bitmap> missileCommandLogoColouredlist = new();

        // "MISSILE COMMAND" logos in each colour. 
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.White));
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.Yellow));
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.Magenta));
        missileCommandLogoColouredlist.Add(new(s_missileCommandLogo)); // #4 = Red.
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.Cyan));
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.LimeGreen));
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.Blue));
        missileCommandLogoColouredlist.Add(LevelUX.ReplaceColourWithTolerance(new(s_missileCommandLogo), 100, Color.Red, Color.Black));

        /*
         * Missile Command Video Memory
         * 
         * It uses a typical indexed color scheme, with an 8-entry palette. Most of the screen has 2 bits per 
         * pixel and so can only show 4 at a time, but the last 32 lines of the screen support 3 bits per pixel, 
         * so all colors can be shown. The colors in the palette can be changed on the fly, providing low-cost 
         * color-cycling animation for highlights and explosions.
         */

        s_missileCommandLogos = missileCommandLogoColouredlist.ToArray();

        /*  https://6502disassembly.com/va-missile-command/wave-guide.html

            Wave	Bkgnd	CityBk	ICBM	City1	ABM	    City2
            1 / 2	Black	Yellow	Red	    Cyan	Blue	Blue
            3 / 4	Black	Yellow	Green	Cyan	Blue	Blue
            5 / 6	Black	Blue	Red	    Yellow	Green	Green
            7 / 8	Black	Red	    Yellow	Yellow	Blue	Blue
            9 / 10	Blue	Yellow	Red	    Magenta	Black	Black
            11 / 12	Cyan	Yellow	Red	    Black	Blue	Blue
            13 / 14	Magenta	Green	Black	Black	Yellow	Yellow
            15 / 16	Yellow	Green	Black	White	Red	    Red
            17 / 18	White	Red	    Magenta	Yellow	Green	Green
            19 / 20	Red	    Yellow	Black	Green	Blue	Blue
        */

        s_uxColorsPerLevel.Add(1, new LevelUX(background: Color.Black, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Blue, cityForegroundColor: Color.Cyan, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(2, new LevelUX(background: Color.Black, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Blue, cityForegroundColor: Color.Cyan, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(3, new LevelUX(background: Color.Black, cityBackground: Color.Yellow, icbmColour: Color.LimeGreen, abmColour: Color.Blue, cityForegroundColor: Color.Cyan, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(4, new LevelUX(background: Color.Black, cityBackground: Color.Yellow, icbmColour: Color.LimeGreen, abmColour: Color.Blue, cityForegroundColor: Color.Cyan, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(5, new LevelUX(background: Color.Black, cityBackground: Color.Blue, icbmColour: Color.Red, abmColour: Color.LimeGreen, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.LimeGreen));
        s_uxColorsPerLevel.Add(6, new LevelUX(background: Color.Black, cityBackground: Color.Blue, icbmColour: Color.Red, abmColour: Color.LimeGreen, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.LimeGreen));
        s_uxColorsPerLevel.Add(7, new LevelUX(background: Color.Black, cityBackground: Color.Red, icbmColour: Color.Yellow, abmColour: Color.Blue, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(8, new LevelUX(background: Color.Black, cityBackground: Color.Red, icbmColour: Color.Yellow, abmColour: Color.Blue, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(9, new LevelUX(background: Color.Blue, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Black, cityForegroundColor: Color.Magenta, cityBackgroundColor: Color.Black));
        s_uxColorsPerLevel.Add(10, new LevelUX(background: Color.Blue, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Black, cityForegroundColor: Color.Magenta, cityBackgroundColor: Color.Black));
        s_uxColorsPerLevel.Add(11, new LevelUX(background: Color.Cyan, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Blue, cityForegroundColor: Color.Black, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(12, new LevelUX(background: Color.Cyan, cityBackground: Color.Yellow, icbmColour: Color.Red, abmColour: Color.Blue, cityForegroundColor: Color.Black, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(13, new LevelUX(background: Color.Magenta, cityBackground: Color.LimeGreen, icbmColour: Color.Black, abmColour: Color.Yellow, cityForegroundColor: Color.Black, cityBackgroundColor: Color.Yellow));
        s_uxColorsPerLevel.Add(14, new LevelUX(background: Color.Magenta, cityBackground: Color.LimeGreen, icbmColour: Color.Black, abmColour: Color.Yellow, cityForegroundColor: Color.Black, cityBackgroundColor: Color.Yellow));
        s_uxColorsPerLevel.Add(15, new LevelUX(background: Color.Yellow, cityBackground: Color.LimeGreen, icbmColour: Color.Black, abmColour: Color.Red, cityForegroundColor: Color.White, cityBackgroundColor: Color.Red));
        s_uxColorsPerLevel.Add(16, new LevelUX(background: Color.Yellow, cityBackground: Color.LimeGreen, icbmColour: Color.Black, abmColour: Color.Red, cityForegroundColor: Color.White, cityBackgroundColor: Color.Red));
        s_uxColorsPerLevel.Add(17, new LevelUX(background: Color.White, cityBackground: Color.Red, icbmColour: Color.Magenta, abmColour: Color.LimeGreen, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.LimeGreen));
        s_uxColorsPerLevel.Add(18, new LevelUX(background: Color.White, cityBackground: Color.Red, icbmColour: Color.Magenta, abmColour: Color.LimeGreen, cityForegroundColor: Color.Yellow, cityBackgroundColor: Color.LimeGreen));
        s_uxColorsPerLevel.Add(19, new LevelUX(background: Color.Red, cityBackground: Color.Yellow, icbmColour: Color.Black, abmColour: Color.Blue, cityForegroundColor: Color.LimeGreen, cityBackgroundColor: Color.Blue));
        s_uxColorsPerLevel.Add(20, new LevelUX(background: Color.Red, cityBackground: Color.Yellow, icbmColour: Color.Black, abmColour: Color.Blue, cityForegroundColor: Color.LimeGreen, cityBackgroundColor: Color.Blue));
    }
}