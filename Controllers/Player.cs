using MissileDefence.Attackers;
using MissileDefence.Controllers.Game;
using MissileDefence.Defenders;
using MissileDefence.UX;
using MissileDefence.UX.Explosions;
using System.Security.Cryptography;

namespace MissileDefence.Controllers;

/// <summary>
/// A player can be either cpu or human.
/// </summary>
internal enum PlayerMode { cpu, human }

/// <summary>
/// Base class representing a player be it "CPU" or "USER" 
/// </summary>
internal class Player
{
    /// <summary>
    /// Manages any explosions for this player.
    /// </summary>
    internal ExplosionManager explosionManager = new();

    /// <summary>
    /// Tracks a list of good guy bases.
    /// </summary>
    internal List<BasesBeingDefended> AllInfrastructureBeingDefended = new();

    /// <summary>
    /// Maintains a list of silo's for this player
    /// </summary>
    internal List<ABMSilo> Silos = new();

    /// <summary>
    /// Represents the display the game is rendered to.
    /// </summary>
    internal PictureBox Canvas;

    /// <summary>
    /// Set to true when game is over.
    /// </summary>
    private bool gameOver = false;

    /// <summary>
    /// true - this play is in the "Game over" animation.
    /// </summary>
    internal bool InGameOverAnimation;

    /// <summary>
    /// Game over animates implode/explode, this determines which.
    /// </summary>
    internal int ImplodeExplodeDirection = 1;

    /// <summary>
    /// Game of implode/explode radius.
    /// </summary>
    internal int radius = 1;

    /// <summary>
    /// Detect if game over.
    /// </summary>
    internal bool GameOver
    {
        get
        {
            return gameOver;
        }
        set
        {
            if (value != gameOver && value)
            {
                InGameOverAnimation = true;
                KillAllABMs();
            }
            gameOver = value;
        }
    }

    /// <summary>
    /// The score this player has achieved in this game.
    /// </summary>
    internal int score = 0;

    /// <summary>
    /// Used during high-score to track initials. n
    /// </summary>
    internal string initials = "¦¦¦";

    /// <summary>
    /// Velocity allows spin of track-ball to step thru letters without user having to keep moving it.
    /// </summary>
    internal float scrolltoPos = 0;

    /// <summary>
    /// A..Z
    /// </summary>
    internal float selectedLetter = 0;

    /// <summary>
    /// This is the total amount of bonus from the last wave.
    /// </summary>
    internal int BonusAmmoAward = 0;

    /// <summary>
    /// This is the number of cities awarded in last wave.
    /// </summary>
    internal int BonusCityAward = 0;

    /// <summary>
    /// This is the number of cities awarded previously.
    /// </summary>
    internal int BonusCitiesAwardedInThePast = 0;

    /// <summary>
    /// These are kept until the user loses a city, in which case it rebuilds and adds this.
    /// </summary>
    internal int BonusCitiesAvailableForRebuilding = 0;

    /// <summary>
    /// Can be used to work out if the player is cpu or user.
    /// </summary>
    internal readonly PlayerMode playerMode;

    int x0 = 0;
    int y0 = 0;
    int x1 = 0;
    int y1 = 0;

    /// <summary>
    /// Where the cross-hair is currently located.
    /// </summary>
    protected Point crossHairLocation = new(0, 0);

    /// <summary>
    /// Setter/Getter for the cross-hair location.
    /// </summary>
    internal Point CrossHairLocation
    {
        get { return crossHairLocation; }
        set { crossHairLocation = value; }
    }

    /// <summary>
    /// true - the cross-hair is visible (and drawn).
    /// </summary>
    protected bool crossHairVisible = false;

    /// <summary>
    /// Setter/Getter for whether the cross-hair is visible or not.
    /// </summary>
    internal bool CrossHairVisible
    {
        get { return crossHairVisible; }
        set { crossHairVisible = value; }
    }

    /// <summary>
    /// Setter/Getter for the score for this player.
    /// </summary>
    int Score
    {
        get { return score; }
        set { score = value; }
    }

    /// <summary>
    /// Where we want the cross hair to go to during anmiations
    /// </summary>
    private Point targetHairLocation;

    /// <summary>
    /// Setter/Getter for the target cross hair location.
    /// </summary>
    public Point TargetHairLocation
    {
        get
        {
            return targetHairLocation;
        }
        set
        {
            targetHairLocation = value;

            x0 = crossHairLocation.X;
            y0 = crossHairLocation.Y;
            x1 = targetHairLocation.X;
            y1 = targetHairLocation.Y;

            // Bresenham's line algorithm to target
            dx = Math.Abs(x1 - x0);
            dy = Math.Abs(y1 - y0);
            sx = x0 < x1 ? 1 : -1;
            sy = y0 < y1 ? 1 : -1;
            err = dx - dy;
        }
    }

    int dx, dy, sx, sy, err;

    /// <summary>
    /// Font used to render score and high-scores.
    /// </summary>
    private readonly Font fontScores = new("Lucida Console", 13);

    /// <summary>
    /// This is the arrow next to the score, that flashes.
    /// </summary>
    private bool showScoreArrow = true;

    /// <summary>
    /// Count of how many cities have been saved.
    /// </summary>
    internal int CitiesSaved;

    /// <summary>
    /// Starts the game (by resetting).
    /// </summary>
    internal virtual void StartGame()
    {
        Reset();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseIndex">1-3</param>
    internal virtual void LaunchABM(int baseIndex)
    {
        // override this for the "user" player. Makes no sense for CPU.
    }

    /// <summary>
    /// Resets the game.
    /// </summary>
    internal void Reset()
    {
        gameOver = false;
        InGameOverAnimation = false;

        // we track the infra (city + silo) plus just silo's in separate list.
        AllInfrastructureBeingDefended.Clear();
        Silos.Clear();

        Score = 0;
        radius = 1;
        initials = "¦¦¦";
        scrolltoPos = 0;
        selectedLetter = 0;
        ImplodeExplodeDirection = 1;
        selectedLetter = 0;

        BonusAmmoAward = 0;
        BonusCityAward = 0;

        BonusCitiesAvailableForRebuilding = 0;
        BonusCitiesAwardedInThePast = 0;
        
        
        CreateBases();

        x0 = 0; x1 = 0; y1 = 0; y0 = 0;

        CrossHairLocation = new Point(256, 231);
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="canvas"></param>
    internal Player(PlayerMode mode, PictureBox canvas)
    {
        playerMode = mode;
        Canvas = canvas;

        Reset();

        Bitmap bitmap = new(512, 462);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Color.Black);
        Canvas.Image = bitmap;
    }

    /// <summary>
    /// Adds all the bases (cities and silos).
    /// </summary>
    private void CreateBases()
    {
        /*
        ; https://6502disassembly.com/va-missile-command/MissileCommand.html#Symcity_x_tbl
        ;
        ; ORIGINAL
        ;
        ; Horizontal positions for the 6 cities.
        ; 
        ; NOTE: cities are not stored left to right here.  Visually the indices are:
        ;   ^ 3 4 0 ^ 2 1 5 ^
        ; This is noticeable when the surviving cities are tabulated between rounds.
        ; 
        ; Some of the code for attacking ground targets references this with an index of
        ; 0-8 instead of 0-5.  This table must be followed by the silo positions.
        ; 
        60e2: 5f b4 94 2c+ city_x_tbl      .bulk   $5f,$b4,$94,$2c,$47,$d0
        ; 
        ; Horizontal positions for the 3 ABM launcher sites.  This table must follow the
        ; city table.
        ; 
        60e8: 14 7b f0     abm_silo_x_tbl   .bulk   $14,$7b,$f0
        ; 
        ; Vertical positions for the 6 cities.  They're not quite on the same line.
        ; 
        60eb: 10 15 12 12+ city_y_tbl       .bulk   $10,$15,$12,$12,$11,$11
        ; 
        ; Vertical positions for the 3 ABM launcher silos.  This table must follow the
        ; city table.
        ; 
        60f1: 16 16 16     abm_silo_y_tbl   .bulk   $16,$16,$16
        ; 
        ; Vertical position of the top of the 3 ABM launcher silos.  This is the Y
        ; coordinate from which the missile trails will start.
        ; 
        
        60f4: 18 18 18     abm_silo_top_tbl .bulk   $18,$18,$18
         
        */

        // Using the above, we plug in the coordinates.

        // THESE ARE CITY CENTERS

        // 60e2: 5f b4 94 2c+ city_x_tbl      .bulk   $5f,$b4,$94,$2c,$47,$d0
        // 60eb: 10 15 12 12 + city_y_tbl     .bulk   $10,$15,$12,$12,$11,$11

        AllInfrastructureBeingDefended.Add(new City(95, 16));
        AllInfrastructureBeingDefended.Add(new City(180, 21));
        AllInfrastructureBeingDefended.Add(new City(148, 18));

        AllInfrastructureBeingDefended.Add(new City(44, 18));
        AllInfrastructureBeingDefended.Add(new City(71, 17));
        AllInfrastructureBeingDefended.Add(new City(208, 17));

        // THESE ARE SILO CENTERS

        // 60e8: 14 7b f0     abm_silo_x_tbl  .bulk   $14,$7b,$f0
        // 60f4: 18 18 18                     .bulk   $18,$18,$18

        ABMSilo missileBase1 = new(20, 24);
        AllInfrastructureBeingDefended.Add(missileBase1);
        Silos.Add(missileBase1);

        ABMSilo missileBase2 = new(124, 24);
        AllInfrastructureBeingDefended.Add(missileBase2);
        Silos.Add(missileBase2);

        ABMSilo missileBase3 = new(241, 24);
        AllInfrastructureBeingDefended.Add(missileBase3);
        Silos.Add(missileBase3);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetHit"></param>
    internal void BaseDestroyed(BasesBeingDefended? targetHit)
    {
        if(targetHit is null) throw new ArgumentNullException(nameof(targetHit),"target should not be null");

        if (!targetHit.IsDestroyed) explosionManager.Add(targetHit.LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP());
        targetHit.BaseDestroyed();
    }

    /// <summary>
    /// Called during the game to give CPU and USER a chance to move their target, and
    /// unleash ABMs.
    /// </summary>
    /// <param name="wave"></param>
    internal virtual void MoveTrackBallAndFire(WaveOfICBMs wave)
    {
        // override this to give user/cpu specific missile launch/guidance.
    }

    /// <summary>
    /// Returns the number of ABMs left by summing up all the silos. If gameover, it's automatically 0.
    /// </summary>
    internal int CountOfABMsRemainingInSilos
    {
        get
        {
            return GameOver ? 0 : Silos[0].ABMRemaining + Silos[1].ABMRemaining + Silos[2].ABMRemaining;
        }
    }
  
    /// <summary>
    /// Computes how many remaining cities are visible.
    /// </summary>
    /// <returns></returns>
    internal int CountOfRemainingVisibleCitiesExcludingBonus
    {
        get
        {
            int count = 0;

            // can we find at least ONE city not destroyed?
            foreach (BasesBeingDefended infra in AllInfrastructureBeingDefended)
            {
                if (infra is City city && !infra.IsDestroyed && city.Visible) ++count;
            }

            return count;
        }
    }

    /// <summary>
    /// Determines if all the cities have been destroyed (end of level).
    /// </summary>
    /// <returns></returns>
    internal bool AllCitiesDestroyed()
    {
        // can we find at least ONE city not destroyed?
        foreach (BasesBeingDefended infra in AllInfrastructureBeingDefended)
        {
            if (infra is City && !infra.IsDestroyed) return false;
        }

        return true;
    }

    /// <summary>
    /// User has lost, ensure no ABMs.
    /// </summary>
    internal void KillAllABMs()
    {
        foreach (ABMSilo silo in Silos)
        {
            silo.ABMRemaining = 0;
        }
    }

    /// <summary>
    /// Determines if all the cities have been destroyed including bonus (end of level).
    /// </summary>
    /// <returns></returns>
    internal bool AllCitiesIncludingBonusDestroyed()
    {
        if (BonusCitiesAvailableForRebuilding > 0) return false; // they have some bonus cities

        // can we find at least ONE city not destroyed?
        foreach (BasesBeingDefended infra in AllInfrastructureBeingDefended)
        {
            if (infra is City && !infra.IsDestroyed) return false;
        }

        return true;
    }

    /// <summary>
    /// Missile silo's recover each wave.
    /// Cities are rebuilt from bonus ones.
    /// </summary>
    internal void EndOfWaveReset()
    {
        BonusAmmoAward = 0;
        BonusCityAward = 0;
        CitiesSaved = 0;

        // at the start of a wave, we re-instate the ammo and silos.
        foreach (ABMSilo abm in Silos)
        {
            abm.IsDestroyed = false;
            abm.ABMRemaining = 10;
        }

        // we rebuild cities at random.
        List<City> destroyed = new();

        foreach (BasesBeingDefended infra in AllInfrastructureBeingDefended)
        {
            if (infra is City city)
            {
                city.Visible = true; // these are hidden during counting, we don't want them hidden after

                // create a list of destroyed ones, to randomly add back
                if (city.IsDestroyed) destroyed.Add(city);
            }
        }

        // replace missing cities with BONUS cities, if they have any
        while (destroyed.Count > 0 && BonusCitiesAvailableForRebuilding > 0)
        {
            // pick one at random out of the ones "destroyed"
            int randomCity = RandomNumberGenerator.GetInt32(0, destroyed.Count);

            // un-destroy it
            destroyed[randomCity].IsDestroyed = false;

            // reduce the bonus count
            --BonusCitiesAvailableForRebuilding;

            // remove from list so we don't try rebuilding twice.
            destroyed.RemoveAt(randomCity);
        }
    }

    /// <summary>
    /// Draws the game for the player.
    /// </summary>
    internal virtual void Draw(Graphics g)
    {
        MoveToTargetCrossHair();
        g.Clear(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].BackgroundColour);

        Bitmap ground = SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].Floor;

        g.DrawImageUnscaled(ground, new Point(0, Canvas.Height - 51));

        foreach (BasesBeingDefended baseDefended in AllInfrastructureBeingDefended) baseDefended.Draw(g);

        // draw coloured filled circle when ABMs hit ICBMs or ICBMs hit bases
        explosionManager.Draw(g);

        WriteHighScore(g);

        // p1 score appears left of high-score, p2 on right of it

        // scores appear on BOTH screens, opposite sides

        WritePlayerScore(g);

        if (GameController.s_players[1] is not null)
        {
            DrawCrossHairIfVisible(g);
        }
    }

    /// <summary>
    /// Enables the GameController to remove any explosions.
    /// </summary>
    internal void KillExplosions()
    {
        explosionManager.Clear();
    }

    /// <summary>
    /// Draw a cross hair.
    ///     |
    ///  ---+---
    ///     |
    /// </summary>
    /// <param name="g"></param>
    private void DrawCrossHairIfVisible(Graphics g)
    {
        if (!crossHairVisible) return;

        using Pen pCrossHair = new(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ABMColour, 2);

        // ---+---
        g.DrawLine(pCrossHair, crossHairLocation.X - 8, crossHairLocation.Y, crossHairLocation.X + 8, crossHairLocation.Y);

        //  |
        //  +
        //  |
        g.DrawLine(pCrossHair, crossHairLocation.X, crossHairLocation.Y - 8, crossHairLocation.X, crossHairLocation.Y + 8);
    }

    /// <summary>
    /// Draws the player score.
    /// </summary>
    /// <param name="g"></param>
    private void WritePlayerScore(Graphics g)
    {
        SolidBrush sb = new(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour);

        string scoreText = Score.ToString();
        SizeF size = g.MeasureString(scoreText, fontScores);

        if (GameController.s_framesRendered % 32 == 0) showScoreArrow = !showScoreArrow; // toggle arrow on > off > on > off ... every 32 frames

        if (playerMode == PlayerMode.cpu)
        {
            // LEFT
            g.DrawString(scoreText, fontScores, sb, 60 - (int)size.Width / 2, 1);
            if (showScoreArrow) g.DrawImageUnscaled(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].LeftArrow, 63 + (int)size.Width / 2, 1);
        }
        else
        {
            // RIGHT
            g.DrawString(scoreText, fontScores, sb, 512 - 60 - (int)size.Width / 2, 1);
            if (showScoreArrow) g.DrawImageUnscaled(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].RightArrow, 512 - 60 - (int)size.Width / 2 - 20, 1);
        }
    }

    /// <summary>
    /// hiscore middle top appears centre of display.
    /// </summary>
    /// <param name="g"></param>
    private void WriteHighScore(Graphics g)
    {
        if (Score > GameController.s_highScoreManager.TopScore) GameController.s_highScoreManager.TopScore = Score;

        string hiScoreText = GameController.s_highScoreManager.TopScore.ToString();
        using SolidBrush sb = new(SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour);

        SizeF size = g.MeasureString(hiScoreText, fontScores);
        g.DrawString(hiScoreText, fontScores, sb, 256 - size.Width / 2, 1);
    }

    /// <summary>
    /// Using Bresenham's this tracks from current position to target.
    /// </summary>
    private void MoveToTargetCrossHair()
    {
        if (x0 == 0 || y0 == 0 || x1 == 0 || y1 == 0) return;
        if (x0 == x1 && y0 == y1) return; // no move

        for (int i = 0; i < 3; i++)
        {
            var e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x0 += sx; }
            if (e2 < dx) { err += dx; y0 += sy; }

            crossHairLocation.X = x0;
            crossHairLocation.Y = y0;
        }
    }
}
