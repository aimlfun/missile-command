using MissileDefence.UX.Animations;

namespace MissileDefence.Controllers.Game;

internal partial class GameController
{
    /// <summary>
    /// 
    /// </summary>
    private AnimationController? p1AnimationControllerDemoMode;

    /// <summary>
    /// 
    /// </summary>
    private AnimationController? p2AnimationControllerDemoMode;

    /// <summary>
    /// Adds the steps to the animation class.
    /// </summary>
    /// <param name="controller"></param>
    private void InitialiseAnimations(AnimationController controller)
    {
        controller.Add(new AnimationShowTitle(), 1F);    // show red "MISSILE COMMAND" for 2 seconds
        controller.Add(new AnimationShowWhiteLogo(), 0.5F);
        controller.Add(new AnimationTitleExplosion(), 1.2F);

        Animation a = new AnimationDefendCities();  //  "DEFEND     CITIES", words centered on cities
        controller.Add(a, 6);

        Animation b = new AnimationMoveCursor
        {
            StartTime = a.StartTime
        };

        b.EndTime = b.StartTime + 8;
        controller.Add(b);

        Animation c = new AnimationRedArrows(a.StartTime + 0.5F, b.StartTime + 5, 0.3F);
        controller.Add(c);

        AnimationGameOverInsertCoin d = new(a.StartTime, b.EndTime);
        controller.Add(d);

        controller.Completed += StartGame;
    }

    private void DemoAnimations()
    {
        p1AnimationControllerDemoMode?.Animate();
        p2AnimationControllerDemoMode?.Animate();

        // paint

        Bitmap displayPlayer1 = new(s_players[0].Canvas.Image); // do NOT add USING, as we are assigning this to Image (it will crash if you do, for obvious reasons)
        Bitmap displayPlayer2 = new(s_players[1].Canvas.Image); // do NOT add USING, as we are assigning this to Image (it will crash if you do, for obvious reasons)

        using Graphics gPlayer1 = Graphics.FromImage(displayPlayer1);
        using Graphics gPlayer2 = Graphics.FromImage(displayPlayer2);

        if (s_showingBases)
        {
            s_players[0].Draw(gPlayer1);
            s_players[1].Draw(gPlayer2);
        }

        // overlay this
        p1AnimationControllerDemoMode?.Draw(gPlayer1);
        p2AnimationControllerDemoMode?.Draw(gPlayer2);

        gPlayer2.DrawString("USE MOUSE TO AIM, 1, 2 & 3 TO FIRE", new Font("Courier New", 15), Brushes.White, 45, 180);

        gPlayer1.Flush();
        gPlayer2.Flush();

        s_players[0].Canvas.Image?.Dispose();
        s_players[0].Canvas.Image = displayPlayer1;

        s_players[1].Canvas.Image?.Dispose();
        s_players[1].Canvas.Image = displayPlayer2;
    }
}
