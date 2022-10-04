using MissileDefence.Controllers;
using System.Security.Cryptography;
/*
Missile Command Wave Guide
~~~~~~~~~~~~~~~~~~~~~~~~~~

In Missile Command you are attacked with a wave of missiles and smart bombs. When you finish a wave, you collect bonus points, 

and your launcher silos are repaired and re-armed. Bonus cities are deployed. As you complete each wave, the attacks become 
faster and more numerous, and the colors change.

Missile movements use 8.8 fixed-point math. The game computes the distance from the start point to the target, and advances the missile
one unit per frame. The distance is approximate, and is capped at 255, so it's possible for a missile to move more than one pixel per frame.

Smart bomb movement changes slightly in wave 9. The evasion code sets flags indicating the directions of nearby explosions. At wave 9+, 
if any nearby collisions are detected, the code always sets a flag indicating an explosion directly above. This prevents the smart bomb 
from moving upward evasively.

Fliers
Bombers and satellites appear as often as possible, so one frequently appears at the same time as a wave of missiles. The height at which 
they fly is chosen at random, but in early waves it's nudged higher to give the player more time to deal with any missiles they fire.

Fliers always move at the same speed: one pixel every 3 frames for bombers, one pixel every 2 frames for satellites. After being destroyed
or allowed to leave the screen, there is a brief "cooldown" period, during which they will not appear. A separate value controls how many 
frames must elapse before a missile can be fired by the flier. The firing cooldown is reset when a missile is fired, and begins counting down 
immediately. All periods are specified in frames (~60Hz).

Wave	Height range	    Cooldown	Fire rate
1	    no fliers allowed	-	        -
2	    +48 (148-195)	    240	        128
3	    +48 (148-195)	    160	        96
4	    +32 (132-163)	    128	        64
5	    +32 (132-163)	    128	        48
6	    +0 (100-131)	    96	        32
7	    +0 (100-131)	    64	        32
8	    +0 (100-131)	    32	        16

Bear in mind that the bottom of the screen is line 0, and the top is line 230. The timing values in the table indicate the minimum delay 
between events. Whether or not a flier fires a missile is affected by the system limits on simultaneous missiles/bombs and the per-wave cap, 
so it's entirely possible for a flier to sail across the screen without firing anything. Fliers are also able to fire multiple missiles at once.

*/
namespace MissileDefence.Attackers;

/// <summary>
/// Generates a "wave" of inbound missiles, more difficult the higher the level.
/// </summary>
internal class MissileWaveGenerator
{
    /*
        https://6502disassembly.com/va-missile-command/wave-guide.html
        
        Missiles
       
        The incoming ordnance changes for each wave up to wave 19. Waves 20 and beyond use the values from wave 19. ICBM speeds increase 
        until they hit their maximum at wave 15.

        Wave	# ICBMs	ICBM frames	# Smart bombs
        1	    12	    4.8125	    0
        2	    15	    2.875	    0
        3	    18	    1.75	    0
        4	    12	    1.03	    0
        5	    16	    0.625	    0
        6	    14	    0.375	    1
        7	    17	    0.25	    1
        8	    10	    0.125	    2
        9	    13	    0.0625	    3
        10	    16	    0.04	    4
        11	    19	    0.02	    4
        12	    12	    0.016	    5
        13	    14	    0.008	    5
        14	    16	    0.004	    6
        15	    18	    0	        6
        16	    14	    0	        7
        17	    17 [16]	0	        7
        18	    19 [18]	0	        7
        19+	    22 [20]	0	        7

        "# ICBMs" and "# Smart bombs" are the number of each that will be fired at the player before the wave ends (unless the wave ends 
        early because three cities have been destroyed). If a missile is a MIRV that splits off 2 additional heads, all 3 count toward the 
        total. Some advanced players will wait as long as possible before shooting down an ICBM in the hope of hitting it right as it splits, 
        conserving ABMs.

        Revision 2 used lower ICBM counts for waves 17-19, shown above in brackets. Adding one or two ICBMs may not seem like a lot, but 
        remember that the player only has 30 ABMs, so the margin for error on a wave with 22 ICBMs and 7 smart bombs is very small. (See $6090.)

        "ICBM frames" is the number of frames that the game waits between moving the ICBMs. A value of zero means it updates the missiles on 
        every frame (~60x/second). Internally this is stored as an 8.8 fixed-point value that is added to a counter. The game can't actually 
        delay for a fraction of a frame, but it can average it out. For example, if the delay were 1.5, the game would delay for 1 frame, then 
        2 frames, then 1, then 2, and so on.


    */
    /// <summary>
    /// Array per level with associated frame rates, number of missiles etc.
    /// </summary>
    internal static float[,] WaveDefinitions = new float[19, 3]
    {
        // missiles, ICBM frames, smart bombs  
        {12, 4.8125F, 0},
        {15, 2.875F, 0},
        {18, 1.75F, 0},
        {12, 1.03F, 0},
        {16, 0.625F, 0},
        {14, 0.375F, 1},
        {17, 0.25F, 1},
        {10, 0.125F, 2},
        {13, 0.0625F, 3},
        {16, 0.04F, 4},
        {19, 0.02F, 4},
        {12, 0.016F, 5},
        {14, 0.008F, 5},
        {16, 0.004F, 6},
        {18, 0, 6},
        {14 ,0, 7},
        {17, 0, 7},
        {19, 0, 7},
        {22, 0, 7}
    };

    /// <summary>
    /// Defines the fliers with increasing difficulty (lower, more frequent, higher firing rate).
    /// </summary>
    internal static int[,] flierDefinitions = new int[8, 5]
    {
        // Wave, Height range (min,max), Cooldown, Fire rate
        { 1,   0,   0,  0,    0 }, //no fliers allowed   -           -
        { 2, 148, 195, 240, 128 },
        { 3, 148, 195, 160,  96 },
        { 4, 132, 163, 128,  64 },
        { 5, 132, 163, 128,  48 },
        { 6, 100, 131,  96,  32 },
        { 7, 100, 131,  64,  32 },
        { 8, 100, 131,  32,  17 }
    };

    /// <summary>
    /// Returns a wave definition (with missiles and fliers).
    /// </summary>
    /// <param name="levelToGenerateWaveOff">The level to generate the wave off </param>
    /// <param name="fliers">List of flier definitions</param>
    /// <returns>ICBM definitions for this wave.</returns>
    internal static List<ICBMDefinition> GenerateWave(int levelToGenerateWaveOff, out List<FlierDefinition> fliers)
    {
        List<ICBMDefinition> icbmMissileDefinitions = new();
        fliers = new();
        float numberOfMissilesInWave = WaveDefinitions[(levelToGenerateWaveOff - 1).Clamp(0, 18), 0 /* number of missiles */];

        // spreads them out more vertically the lower the level.        
        int pos = 231;

        if (levelToGenerateWaveOff > 1)
        {
            int flierLevel = (levelToGenerateWaveOff - 1).Clamp(0, 7);
            int cooldown = 0;

            int last = RandomNumberGenerator.GetInt32(8 * 16, 14 * 16);

            // generate up to 5 fliers (any more will potentially result in running out of ABMs)
            for (int flier = 0; flier < 5; flier++)
            {
                FlierDefinition fd = new();
                fd.bomberOrSatellite = RandomNumberGenerator.GetInt32(0, 100) < 50 ? FlierDefinition.FlierTypes.bomber : FlierDefinition.FlierTypes.satellite;

                int min =  flierDefinitions[flierLevel, 1];
                int max = flierDefinitions[flierLevel, 2];
                fd.deltaX = RandomNumberGenerator.GetInt32(0, 100) < 50 ? -1 : 1;

                // create horizontal positions for the bombs it drops
                int bomb = 0;

                while (bomb < 256)
                {
                    float minTime = flierDefinitions[flierLevel, 3]*1.5F ;

                    bomb += RandomNumberGenerator.GetInt32((int)minTime - 5, (int)minTime + 15);
                
                    if (bomb > 255) break; // off screen 

                    fd.icbmDrop.Add((int)(bomb/2)*2, RandomNumberGenerator.GetInt32(0, 9));
                }

                fd.location.AltitudeInMissileCommandDisplayPX = RandomNumberGenerator.GetInt32(min,max);
                if (fd.deltaX == 1) fd.location.HorizontalInMissileCommandDisplayPX = 0; else fd.location.HorizontalInMissileCommandDisplayPX = 256 - 16;

                fd.framesRenderedBeforeAppearing = last+ cooldown;

                // cool down is the next time one can appear
                cooldown = fd.framesRenderedBeforeAppearing + flierDefinitions[flierLevel, 3];
                last = fd.framesRenderedBeforeAppearing;

                fliers.Add(fd);
            }
        }

        // create ICBM wave
        for (int missileIndex = 0; missileIndex < numberOfMissilesInWave; missileIndex++)
        {
            int pixelsOffscreenAbove = (30 - RandomNumberGenerator.GetInt32(0, levelToGenerateWaveOff * 2)).Clamp(5, 25);
            int verticalOffset = pos + pixelsOffscreenAbove;
            pos = verticalOffset;

            PointA location = new(RandomNumberGenerator.GetInt32(0, 255), verticalOffset);

            int targetLocationMissileIsTryingToHit = RandomNumberGenerator.GetInt32(0, 9);

            /*
                An ICBM can split (MIRV) on any wave if certain conditions are met. 
                The code walks through the 8 ICBM slots, updating a maximum altitude value as it goes. 
            
                If the maximum altitude seen so far is between 128 and 159,  meaning we have at least one ICBM in the upper-middle 
                of the screen and nothing higher, then the ICBM currently being examined becomes a MIRV candidate. 
                
                The candidate's altitude is irrelevant, except that it must be below 160. So the conditions required for an ICBM to be 
                eligible to split are (see $5379/$56d1):

                The current missile, or a previously-examined missile, is at an altitude between 128 and 159.
                
                No previously-examined missile is above 159.
                
                There must be available slots in the ICBM table, and unspent ICBMs for the wave.
                A single missile can spawn up to 3 additional missiles. Each missile will have a different target.
            */

            // the comment is what the original does. We cannot do it quite the same way as it is pre-computed at the 
            // start of the wave (to be same for both players).
            int divisor = 20 - (levelToGenerateWaveOff - 3) / 2 + RandomNumberGenerator.GetInt32(0, 40) - 20;

            if (divisor == 0) divisor = 10;

            int splitAt = levelToGenerateWaveOff < 4 ? 0 : 150 + 462 / divisor; // random so point changes

            if (splitAt != 0 && splitAt < 134) splitAt = 134 + RandomNumberGenerator.GetInt32(0, 4) - 2;

            // stop MIRVs splitting below minimum cross hair height. (45 is min altitude of x-hair)
            if (splitAt != 0 && splitAt < 50) splitAt = 50;

            if (RandomNumberGenerator.GetInt32(0, levelToGenerateWaveOff) < 3) splitAt = -1; // it's random as to whether it splits, and more likely as the levels increase

            // A single missile can spawn up to 3 additional missiles. Each missile will have a different target.
            int splitCount = splitAt > 0 ? RandomNumberGenerator.GetInt32(1, 3) : 0;

            List<int> splitTargets = new();

            for (int i = 0; i < splitCount; i++)
            {
                splitTargets.Add(RandomNumberGenerator.GetInt32(0, 9));
            }

            // create a definiton
            icbmMissileDefinitions.Add(new(location, targetLocationMissileIsTryingToHit, splitTargets, splitAt));
        }

        return icbmMissileDefinitions;
    }
}
