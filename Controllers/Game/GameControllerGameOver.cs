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
    /// 
    /// </summary>
    private static void GameOver()
    {        
        State = PossibleStatesForTheStateMachine.HighScoreCheck;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    private static void GameOverAnimation(Player player)
    {
        if (!player.InGameOverAnimation)
        {
            return; // no more animation
        }

        player.radius += player.ImplodeExplodeDirection;

        if (player.radius < 0)
        {
            player.InGameOverAnimation = false;
            GameOverWords(player);
            return;
        }

        if (player.radius > 108)
        {
            player.ImplodeExplodeDirection = -player.ImplodeExplodeDirection;
        }

        bool drawTHEEND = player.radius > 97 || player.ImplodeExplodeDirection == -1;

        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        player.Draw(gPlayer);
        ClipDrawOctagon(gPlayer, player.radius * 2);

        if (drawTHEEND)
        {
            using Font f = new("Courier New", 40, FontStyle.Bold);
            SizeF size = gPlayer.MeasureString("THE END", f);
            gPlayer.DrawString("THE END", f, Brushes.Red, 256 - size.Width / 2, 231 - size.Height / 2);
        }

        // replace with new rendered frame
        gPlayer.Flush();

        player.Canvas.Image = displayPlayer;
    }

    /// <summary>
    /// Displays "GAME OVER".
    /// </summary>
    /// <param name="gPlayer"></param>
    private static void GameOverWords(Player player)
    {
        Bitmap displayPlayer = (Bitmap)player.Canvas.Image;

        using Graphics gPlayer = Graphics.FromImage(displayPlayer);

        gPlayer.SmoothingMode = SmoothingMode.HighSpeed;
        gPlayer.CompositingQuality = CompositingQuality.HighSpeed;

        using Font f = new("Courier New", 40, FontStyle.Bold);
        SizeF size = gPlayer.MeasureString("GAME OVER", f);
        gPlayer.DrawString("GAME OVER", f, Brushes.Red, 256 - size.Width / 2, 231 - size.Height / 2 + 40);

        // replace with new rendered frame
        gPlayer.Flush();

        player.Canvas.Image = displayPlayer;
    }

    /// <summary>
    /// Draw an "octagon" explosion.
    /// </summary>
    /// <param name="gPlayer"></param>
    /// <param name="radius"></param>
    private static void ClipDrawOctagon(Graphics gPlayer, int radius)
    {
        int x = 256;
        int y = 231;

        double a = radius;
        double b = radius;

        var points = new List<Point>();

        for (int pn = 0; pn < 8; pn++)
        {
            double angle = 360.0 / 8 * pn * Math.PI / 180;
            
            points.Add(new Point((int)(a * Math.Cos(angle) + x), (int)(b * Math.Sin(angle) + y)));
        }

        using Brush br = new SolidBrush(Color.FromArgb(200,
                                                       RandomNumberGenerator.GetInt32(100, 255),
                                                       RandomNumberGenerator.GetInt32(100, 255),
                                                       RandomNumberGenerator.GetInt32(100, 255)));

        using GraphicsPath path = new();

        path.AddPolygon(points.ToArray());
        gPlayer.SetClip(path, CombineMode.Exclude); // when draw occurs, path is painted circle and thus only that appears
        gPlayer.FillRectangle(br, 0, 30, 512, 462);
    }
}