using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using MissileDefence.UX;
using MissileDefence.Attackers;
using MissileDefence.Attackers.Training;
using MissileDefence.UX.Animations;
using MissileDefence.Defenders;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// We track hi-scores which requires load/save, neatly handled by this object.
    /// </summary>
    internal static HighScoreManager s_highScoreManager = new();

    /// <summary>
    /// Displays the high scores on both displays.
    /// </summary>
    private static void ShowHighScores()
    {
        // 10 seconds, and it comes out of showing "high scores" back to title
        if (++s_framesRendered > c_gameTimerInterval * 10) State = PossibleStatesForTheStateMachine.Reset;
        
        DisplayHighScore(s_players[0]);
        DisplayHighScore(s_players[1]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    private static void DisplayHighScore(Player player)
    {
        Color color1 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ABMColour;
        Color color2 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ICBMColour;

        using SolidBrush mainNumbers = new(color2);
        using SolidBrush mainWriting = new(color1);

        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        player.Draw(gPlayer);

        using Font fontRegular = new("Courier New", 20, FontStyle.Bold);
        DrawStringCenteredHorizontally(gPlayer, "HIGH SCORES", fontRegular, mainWriting, 60);

        int index = 80;
        foreach (HighScore highScore in s_highScoreManager.hiscores)
        {
            DrawStringCenteredHorizontally(gPlayer, $"{highScore.Initials} {highScore.Score,10}", fontRegular, mainNumbers, index);
            index += 10;
        }

        DrawStringCenteredHorizontally(gPlayer, "BONUS CITY EVERY 10000 POINTS", fontRegular, mainWriting, 180);

        // replace with new rendered frame
        gPlayer.Flush();

        player.Canvas.Image = displayPlayer;
    }

    /// <summary>
    /// Invokes the enter initials for both players.
    /// </summary>
    private static void EnterHighScoreInitialsForBothPlayers()
    {
        ++s_framesRendered;

        if (s_highScoreManager.IsHighScore(s_players[0])) EnterInitialsByPlayer(s_players[0]); else s_players[0].selectedLetter = -1; // enter the hiscore
        if (s_highScoreManager.IsHighScore(s_players[1])) EnterInitialsByPlayer(s_players[1]); else s_players[1].selectedLetter = -1; // enter the hiscore

        if (s_players[0].selectedLetter == -1 && s_players[1].selectedLetter == -1)
        {
            s_framesRendered = 0;
            State = PossibleStatesForTheStateMachine.ShowHighScores;
        }
    }
    
    /// <summary>
    ///          A A A                          ] SCORE COLOUR
    ///  
    ///         PLAYER X
    ///        GREAT SCORE                      ]- BIGGER
    ///     ENTER YOUR INITIALS                 ] LEFT SCORE ARROW COLOUR (ABM?)
    /// SPIN BALL TO CHANGE LETTERS
    /// PRESS ANY FIRE SWITCH TO SELECT
    /// </summary>
    private static void EnterInitialsByPlayer(Player player)
    {      
        if (player.initials == "¦¦¦")
        {
            player.initials = "A";
            if(player.playerMode == PlayerMode.human) Cursor.Position = player.Canvas.PointToScreen(new Point(256, 231));

            player.scrolltoPos = 0;
            player.selectedLetter = 0;
        }
 
        // -1 = finished selecting initials.
        if (s_framesRendered % 4 != 0|| player.selectedLetter == -1) return;

        if (player.playerMode == PlayerMode.cpu)
        {
            if (CPUhasFinishedSigningHiScore(player))
            {
                player.selectedLetter = -1;
                return;
            }
        }
        else
        {
            // scroll letters based on how much the user has swiped the mouse pad
            
            // if cursor isn't within the area of the player's display, we cannot use the swipe action
            Control? c = FindControlAtCursor(player.Canvas.FindForm());
            
            if (c == null || c != player.Canvas) return;

            // we locate the cursor at the center of the canvas, but it is in "real" coordinates
            // so we have to subtract "real" coordinates to compute swipe.
            Point center = player.Canvas.PointToScreen(new Point(256, 231));

            // work out delta
            int deltaX = Cursor.Position.X - center.X;
            int deltaY = Cursor.Position.Y - center.Y;
            float delta = ((deltaX + deltaY) * 4).Clamp(-20, 20);// / c_sensitivity;

            player.scrolltoPos += delta / 12;
        }

        player.selectedLetter += Math.Sign((int)player.scrolltoPos);

        // A..Z..A..Z... | Z..A..Z..A wrap around
        if (player.selectedLetter < 0) player.selectedLetter = 26 + player.selectedLetter; 
        if (player.selectedLetter > 25) player.selectedLetter -= 26;

        player.scrolltoPos -= Math.Sign((int)player.scrolltoPos);

        // delete last character and show the one being editer.
        player.initials = player.initials[..^1] + Convert.ToChar((int) player.selectedLetter + 65); // 65="A".

        Color color1 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ABMColour;
        Color color2 = SharedUX.s_uxColorsPerLevel[s_UXWaveTheme].ICBMColour;

        using SolidBrush mainNumbers = new(color2);
        using SolidBrush mainWriting = new(color1);

        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        player.Draw(gPlayer);

        using Font fontBig = new("Courier New", 40, FontStyle.Bold);
        DrawStringCenteredHorizontally(gPlayer, "GREAT SCORE", fontBig, mainWriting, 86);

        // we aren't using a trackball, so this is for authenticity rather than practicality...

        using Font fontRegular = new("Courier New", 20, FontStyle.Bold);
        gPlayer.DrawString(SpaceBetweenLetters(player.initials), fontRegular, mainNumbers, new Point( 256 - 91 / 2, 30 * 2 - 33 / 2));
        DrawStringCenteredHorizontally(gPlayer, "PLAYER 1", fontRegular, mainWriting, 60);
        DrawStringCenteredHorizontally(gPlayer, "ENTER YOUR INITIALS", fontRegular, mainWriting, 110);
        DrawStringCenteredHorizontally(gPlayer, "SPIN BALL TO CHANGE LETTERS", fontRegular, mainWriting, 130);
        DrawStringCenteredHorizontally(gPlayer, "PRESS ANY FIRE SWITCH TO SELECT", fontRegular, mainWriting, 150);

        // replace with new rendered frame
        gPlayer.Flush();

        player.Canvas.Image = displayPlayer;
        if(player.playerMode == PlayerMode.human && player.Canvas.FindForm().Focused) Cursor.Position = player.Canvas.PointToScreen(new Point(256, 231));
    }

    /// <summary>
    /// Using the same UI as the user, enter the initials "CPU".
    /// </summary>
    /// <param name="player">The CPU player.</param>
    /// <returns></returns>
    private static bool CPUhasFinishedSigningHiScore(Player player)
    {
        // we're complete entering C P U?
        if (player.selectedLetter == -1) return true;

        // rotate up the alphabet to find "C" (from A)
        if (player.initials[0] != 'C')
        {
            player.scrolltoPos = 1; // scroll to "C"
            return false;
        }

        // we've entered "C"? so add "A" to make "C A  ", we'll search next time for P
        if (player.initials.Length == 1)
        {
            s_players[0].initials += "A";
            return false;
        }

        // until we get from "C A  " to "C P  " we scroll up the alphabet
        if (player.initials[1] != 'P')
        {
            player.scrolltoPos = 1;
            return false;
        }

        // we've now got "C P  ", let's make "C P A"
        if (player.initials.Length == 2)
        {
            s_players[0].initials += "A";
            return false;
        }

        // until we get from "C P A" to "C P U" we scroll up the alphabet.
        if (player.initials[2] != 'U')
        {
            player.scrolltoPos = 1;
            return false;
        }

        // we now have "C P U", we can add the score, and exit
        if (player.initials.Length == 3)
        {
            s_highScoreManager.Add(s_players[0].score, s_players[0].initials);

            return true;
        }

        // unlikely to ever be reached because of the logic below.
       
        return false;
    }

    /// <summary>
    /// User pressed 1/2/3, if editing high-score for that user, then fix the current "letter".
    /// </summary>
    internal static void FixInitial()
    {
        if (s_players[1].initials == "¦¦¦") return;
        
        if (s_players[1].initials.Length == 3 )
        {
            s_highScoreManager.Add(s_players[1].score, s_players[1].initials);

            s_players[1].selectedLetter = -1;
            return;
        }

        s_players[1].initials += "A";
    }

    /// <summary>
    /// Space in between each letter.
    /// Used because we display "C P U" not "CPU".
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string SpaceBetweenLetters(string text)
    {
        text = (text + "  ")[..3];
        char[] s = text.ToCharArray();
        return string.Join(" ", s) + " ";
    }

    /// <summary>
    /// Writes text at a particular location centered horizontally.
    /// </summary>
    /// <param name="g"></param>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="brush"></param>
    /// <param name="Y"></param>
    private static void DrawStringCenteredHorizontally(Graphics g, string text, Font font, Brush brush, int Y)
    {
        SizeF size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, 256 - size.Width / 2, Y*2 - size.Height / 2);
    }
    
    /// <summary>
    /// Works out from the control hierarchy where the point is situated.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static Control? FindControlAtPoint(Control container, Point pos)
    {
        Control? child;
        
        foreach (Control c in container.Controls)
        {
            if (c.Visible && c.Bounds.Contains(pos))
            {
                child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                
                if (child == null) 
                    return c;
                else 
                    return child;
            }
        }
     
        return null;
    }

    /// <summary>
    /// Returns a control at the current cursor position of "null" if not within form.
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    public static Control? FindControlAtCursor(Form form)
    {
        Point pos = Cursor.Position;
        
        if (form.Bounds.Contains(pos)) return FindControlAtPoint(form, form.PointToClient(pos));
        
        return null;
    }
}