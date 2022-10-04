using MissileDefence.Attackers;

namespace MissileDefence.UX;

internal class LevelUX
{
    /// <summary>
    /// 
    /// </summary>
    private readonly static Bitmap s_floorBaseImage = new(SharedUX.s_floorBaseImage256x26);

    /// <summary>
    /// 
    /// </summary>
    private readonly static Bitmap s_cityBaseImage = new(SharedUX.s_cityBaseImage);

    /// <summary>
    /// 
    /// </summary>
    private readonly static Bitmap s_missileBaseImage = new(SharedUX.s_abmAmmoImage);

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap Floor;

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap City;

    /// <summary>
    /// 
    /// </summary>
    internal Color BackgroundColour;

    /// <summary>
    /// 
    /// </summary>
    internal Color CityBackgroundColour;

    /// <summary>
    /// 
    /// </summary>
    internal Color ABMColour;

    /// <summary>
    /// 
    /// </summary>
    internal Color ICBMColour;

    /// <summary>
    /// 
    /// </summary>
    internal Color City1;

    /// <summary>
    /// 
    /// </summary>
    internal Color City2;

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap Missile;

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap CityDestroyed = new(s_cityBaseImage);

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap LeftArrow;

    /// <summary>
    /// 
    /// </summary>
    internal Bitmap RightArrow;

    internal Dictionary<FlierDefinition.FlierTypes, Dictionary<int, Bitmap>> Flier;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="background"></param>
    /// <param name="ground"></param>
    /// <param name="ABMColour"></param>
    /// <param name="ICBMColour"></param>
    /// <param name="cityForegroundColor"></param>
    /// <param name="cityBackgroundColor"></param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. METHODS CALLED *DO* INITIALISE THE VARIABLES
    internal LevelUX(Color background, Color cityBackground, Color icbmColour, Color cityForegroundColor, Color abmColour, Color cityBackgroundColor)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        BackgroundColour = background;
        CityBackgroundColour = cityBackground;

        ABMColour = abmColour;
        ICBMColour = icbmColour;

        City1 = cityForegroundColor;
        City2 = cityBackgroundColor;

        LeftArrow = new Bitmap(SharedUX.s_leftArrow);
        RightArrow = new Bitmap(SharedUX.s_rightArrow);

        if (abmColour != Color.Blue)
        {
            LeftArrow = ReplaceColourWithTolerance(LeftArrow, 50, Color.Blue, abmColour);
            RightArrow = ReplaceColourWithTolerance(RightArrow, 50, Color.Blue, abmColour);
        }

        LeftArrow.MakeTransparent();
        RightArrow.MakeTransparent();

        // floor image is white for pixels, black for non pixels
        ColourizeFloor(background);
        ColourizeCity(background, cityForegroundColor, cityBackgroundColor);

        Missile = ReplaceColourWithTolerance(new Bitmap(s_missileBaseImage), 50, Color.Blue, abmColour);
        Missile.MakeTransparent(Color.White);

        ColourizeDestroyedCity(background, cityBackground);

        Dictionary<int, Bitmap> bomber = new()
        {
            { -1, ColourizeFlier(SharedUX.s_bomberR2LBaseImage, ICBMColour, abmColour) },
            { 1, ColourizeFlier(SharedUX.s_bomberL2RBaseImage, ICBMColour, abmColour) }
        };

        Dictionary<int, Bitmap> satellite = new()
        {
            { -1, ColourizeFlier(SharedUX.s_satelliteR2LBaseImage, ICBMColour, abmColour) },
            { 1, ColourizeFlier(SharedUX.s_satelliteL2RBaseImage, ICBMColour, abmColour) }
        };

        Flier = new Dictionary<FlierDefinition.FlierTypes, Dictionary<int, Bitmap>>
        {
            { FlierDefinition.FlierTypes.bomber, bomber },
            { FlierDefinition.FlierTypes.satellite, satellite }
        };
    }

    /// <summary>
    /// Colours the "fliers" (bomber, satellite).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mainColour"></param>
    /// <param name="abmColour"></param>
    /// <returns></returns>
    private static Bitmap ColourizeFlier(Bitmap source, Color mainColour, Color abmColour)
    {
        Dictionary<Color, Color> flierMappings = new();
        flierMappings.Add(Color.White, abmColour);

        Color transparent = Color.Black;

        if (mainColour == transparent)
        {
            flierMappings.Add(transparent, Color.Green);
            flierMappings.Add(Color.Red, mainColour);
            transparent = Color.Green;
        }
        else
        {
            flierMappings.Add(Color.Red, mainColour);
        }
        Bitmap flier = ReplaceColourWithTolerance(source, 3, flierMappings);

        flier.MakeTransparent(transparent);
        return flier;        
    }

    /// <summary>
    /// Colours the "destroyed" city in background colour.
    /// </summary>
    /// <param name="background"></param>
    /// <param name="cityBackground"></param>
    private void ColourizeDestroyedCity(Color background, Color cityBackground)
    {
        // destroyed city becomes background colour

        Dictionary<Color, Color> cityDestroyedMappings = new();
        cityDestroyedMappings.Add(Color.White, cityBackground);
        cityDestroyedMappings.Add(Color.Red, cityBackground);
        cityDestroyedMappings.Add(Color.Black, background);

        CityDestroyed = ReplaceColourWithTolerance(CityDestroyed, 50, cityDestroyedMappings);

        CityDestroyed.MakeTransparent(background);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="background"></param>
    /// <param name="cityForegroundColor"></param>
    /// <param name="cityBackgroundColor"></param>
    private void ColourizeCity(Color background, Color cityForegroundColor, Color cityBackgroundColor)
    {
        City = new Bitmap(s_cityBaseImage);
        Dictionary<Color, Color> cityMappings = new();
        cityMappings.Add(Color.White, cityForegroundColor);
        cityMappings.Add(Color.Red, cityBackgroundColor);
        cityMappings.Add(Color.Black, background);

        City = ReplaceColourWithTolerance(City, 50, cityMappings);
        City.MakeTransparent(background);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="background"></param>
    private void ColourizeFloor(Color background)
    {
        Floor = new Bitmap(s_floorBaseImage);
        Dictionary<Color, Color> floorMappings = new();
        floorMappings.Add(Color.White, CityBackgroundColour);
        floorMappings.Add(Color.Black, background);
        Floor = ReplaceColourWithTolerance(Floor, 50, floorMappings);
    }

    /// <summary>
    /// Replaces a colour on an image, with a degree of tolerance.
    /// </summary>
    /// <param name="inputBitmap"></param>
    /// <param name="tolerance"></param>
    /// <param name="oldColour"></param>
    /// <param name="newColour"></param>
    /// <returns></returns>
    internal static Bitmap ReplaceColourWithTolerance(Bitmap inputBitmap, int tolerance, Color oldColour, Color newColour)
    {
        Bitmap outputImage = new(inputBitmap.Width, inputBitmap.Height);
        using Graphics graphicsForReplacementBitmap = Graphics.FromImage(outputImage);
        graphicsForReplacementBitmap.DrawImage(inputBitmap, 0, 0);

        for (int y = 0; y < outputImage.Height; y++)
        {
            for (int x = 0; x < outputImage.Width; x++)
            {
                Color PixelColor = outputImage.GetPixel(x, y);

                // pixel is not within tolerance?
                if (PixelColor.R <= oldColour.R - tolerance || PixelColor.R >= oldColour.R + tolerance ||
                    PixelColor.G <= oldColour.G - tolerance || PixelColor.G >= oldColour.G + tolerance ||
                    PixelColor.B <= oldColour.B - tolerance || PixelColor.B >= oldColour.B + tolerance)
                {
                    continue;
                }

                int RColorDiff = ReplaceColour(oldColour.R, newColour.R, PixelColor.R);
                int GColorDiff = ReplaceColour(oldColour.G, newColour.G, PixelColor.G);
                int BColorDiff = ReplaceColour(oldColour.B, newColour.B, PixelColor.B);

                outputImage.SetPixel(x, y, Color.FromArgb(RColorDiff, GColorDiff, BColorDiff));
            }
        }

        return outputImage;
    }

    /// <summary>
    /// Replace multiple colours simultaneously.
    /// </summary>
    /// <param name="inputBitmap"></param>
    /// <param name="tolerance"></param>
    /// <param name="colorMappings"></param>
    /// <returns></returns>
    internal static Bitmap ReplaceColourWithTolerance(Bitmap inputBitmap, int tolerance, Dictionary<Color, Color> colorMappings)
    {
        Bitmap outputImage = new(inputBitmap.Width, inputBitmap.Height);
        using Graphics graphicsForReplacementBitmap = Graphics.FromImage(outputImage);
        graphicsForReplacementBitmap.DrawImage(inputBitmap, 0, 0);

        for (int y = 0; y < outputImage.Height; y++)
        {
            for (int x = 0; x < outputImage.Width; x++)
            {
                Color PixelColor = outputImage.GetPixel(x, y);

                foreach (Color Colour in colorMappings.Keys)
                {

                    // pixel is not within tolerance?
                    if (PixelColor.R <= Colour.R - tolerance || PixelColor.R >= Colour.R + tolerance ||
                        PixelColor.G <= Colour.G - tolerance || PixelColor.G >= Colour.G + tolerance ||
                        PixelColor.B <= Colour.B - tolerance || PixelColor.B >= Colour.B + tolerance)
                    {
                        continue;
                    }

                    int RColorDiff = ReplaceColour(Colour.R, colorMappings[Colour].R, PixelColor.R);
                    int GColorDiff = ReplaceColour(Colour.G, colorMappings[Colour].G, PixelColor.G);
                    int BColorDiff = ReplaceColour(Colour.B, colorMappings[Colour].B, PixelColor.B);

                    outputImage.SetPixel(x, y, Color.FromArgb(RColorDiff, GColorDiff, BColorDiff));
                    break; // we've replaced the colour
                }
            }
        }

        return outputImage;
    }

    /// <summary>
    /// Replaces a colourr.
    /// </summary>
    /// <param name="oldColor"></param>
    /// <param name="newColour"></param>
    /// <param name="pixelColor"></param>
    /// <returns></returns>
    private static int ReplaceColour(byte oldColor, byte newColour, byte pixelColor)
    {
        int colourDiff = oldColor - pixelColor;

        if (pixelColor > oldColor) colourDiff = newColour + colourDiff; else colourDiff = newColour - colourDiff;

        return colourDiff.Clamp(0, 255);
    }
}