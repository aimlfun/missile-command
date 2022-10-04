//#define test
using MissileDefence;
using MissileDefence.Configuration;
using MissileDefence.AI;
using MissileDefence.Controllers.Game;
using MissileDefence.Controllers;
using MissileDefence.UX;

namespace MissileDefence;

/// <summary>
/// Main form comprising of two "displays" (one per player)
/// </summary>
public partial class MainForm : Form
{
    /// <summary>
    /// Orchestrates the game and demo (UI), plus player-controllers.
    /// </summary>
    GameController gameController;

    /// <summary>
    /// If the first .ai file is present, all 30 should be.
    /// </summary>
    readonly bool requiresTraining = !File.Exists(Path.Combine(Program.aiFolder, $"missile0.ai"));

    /// <summary>
    /// Constructor.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable. GameController declared after display
    public MainForm()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        InitializeComponent();

        // loads images and re-colours them etc.
        SharedUX.Initialise();
    }

    /// <summary>
    /// On load of form.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Form1_Load(object sender, EventArgs e)
    {
        InitialiseTheNeuralNetworksForTheABMs();

        /*
        if (!requiresTraining)
        {
            float maxFitness = -int.MaxValue;
            int maxNN = -1;

            // load the trained missiles
            foreach (int id in NeuralNetwork.s_networks.Keys)
            {
                NeuralNetwork nn = NeuralNetwork.s_networks[id];

                nn.Load(Path.Combine(Program.aiFolder, $"missile{nn.Id}.ai"));

                if (nn.Fitness > maxFitness)
                {
                    maxFitness = nn.Fitness;
                    maxNN = id;
                }
            }
        }
        */
        
        Show(); // maximises screen so width/height are correct
    }

    /// <summary>
    /// Form has been shown at it's real size, now we resize edges etc to ensure our "displays" are
    /// the correct size.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainForm_Shown(object sender, EventArgs e)
    {
        MakePlayerScreensCorrectAspectRatio();

        // create a game controller and notify it of the "displays" (canvas in the form of PictureBox)
        gameController = new(CanvasPlayer1, CanvasPlayer2, requiresTraining);

        // either starts training, or the demo mode
        GameController.Start();
    }

    /// <summary>
    /// Everything is implemented as split screen; CPU on left, human on right. Both 512x462 in size (2 x original display width/height)
    /// </summary>
    private void MakePlayerScreensCorrectAspectRatio()
    {
        // determine the best width/height based on scaling
        int width = (int)Math.Round(SharedUX.c_MissileCommandVideoWidthPX * 2F);
        int height = (int)Math.Round(SharedUX.c_MissileCommandVideoHeightPX * 2F);

        SuspendLayout();

        CanvasPlayer1.Width = width;
        CanvasPlayer2.Width = width;
        CanvasPlayer1.Height = height;
        CanvasPlayer2.Height = height;

        panelMain.Height = height;

        if (CanvasPlayer1.Height != height)
        {
            panelBottom.Height += panelBottom.Height - height;
        }

        if (CanvasPlayer2.Width > width)
        {
            int diff = (CanvasPlayer2.Width - width) / 2;
            panelLeft.Width += diff;
            panelRight.Width += diff;

            panelLeftBottom.Width = panelLeft.Width;
            panelLeftTop.Width = panelLeft.Width;
            panelRightBottom.Width = panelRight.Width;
            panelRightTop.Width = panelRight.Width;

            panelTopLeft.Width = width;
            panelTopRight.Width = width;

            panelLeftBaseControls.Width = CanvasPlayer1.Width;
            //controlsCPU.Width = CanvasPlayer1.Width;
            controlsHuman.Width = CanvasPlayer1.Width;
        }

        if (CanvasPlayer2.Width != width)
        {
            panelDivider.Width += CanvasPlayer2.Width - width;
            panelDividerControls.Width = panelDivider.Width;
            panelDividerControls.Width = panelDivider.Width;
            panelTopDividerControls.Width = panelDivider.Width;
        }

        ResumeLayout();

        // adjust the bottom to fill any space.
        if (CanvasPlayer1.Height != height)
        {
            panelBottom.Height += CanvasPlayer1.Height - height;
        }
    }

    /// <summary>
    /// Unfortunately, winForm apps don't provide key-down to other controls, so we handle it here.
    /// User pressed a key, handle ABM launch, slow-mode toggle or start game etc.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        // in game press "1", "2" or "3" for launching missile from Alpha/Delta/Omega base
        switch (GameController.State)
        {
            case GameController.PossibleStatesForTheStateMachine.PlayGame:

                // key 1 launches missile from base 1
                if (e.KeyCode == Keys.End || e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1) GameController.s_players[1].LaunchABM(1);

                // key 2 launches missile from base 2
                if (e.KeyCode == Keys.Down || e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2) GameController.s_players[1].LaunchABM(2);

                // key 3 launches missile from base 3
                if (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3) GameController.s_players[1].LaunchABM(3);

                break;

            case GameController.PossibleStatesForTheStateMachine.EnterInitials:

                // key 1/2/3 saves initial
                if ((e.KeyCode == Keys.End || e.KeyCode == Keys.NumPad1 || e.KeyCode == Keys.D1) ||
                   (e.KeyCode == Keys.Down || e.KeyCode == Keys.NumPad2 || e.KeyCode == Keys.D2) ||
                   (e.KeyCode == Keys.PageDown || e.KeyCode == Keys.NumPad3 || e.KeyCode == Keys.D3))
                {
                    GameController.FixInitial();
                }
                
                break;

            case GameController.PossibleStatesForTheStateMachine.ShowTitle:
                // "2" starts game 
                if (e.KeyValue == 50 || e.KeyCode == Keys.Down || e.KeyCode == Keys.NumPad2)
                {
                    gameController.StartGame();
                }

                break;
        }

        switch (e.KeyCode)
        {
            case Keys.S:
                // "S" toggles slow mode (so we can see what is happening esp. during learning),
                // ctrl-s saves the AI trained model
                if (!e.Control)
                {
                    gameController.SlowMode = !gameController.SlowMode;
                }
                else
                {
                    if (requiresTraining) SaveTrainedModel(); // only applies when in training mode
                }
                break;

            case Keys.P:
                // "P" pause the game / animation
                GameController.PauseUnpause();
                break;

            case Keys.Q:
                // "Q" goes into quiet mode (faster learn)
                if (requiresTraining) TrainingController.ToggleQuietMode();
                break;

            case Keys.H:
                // "H" heat sensor
                Settings.s_sensor.DrawTargetPartOfHeatSensor = !Settings.s_sensor.DrawTargetPartOfHeatSensor;
                break;

            case Keys.C:
                // "C" cone part of heat sensor
                Settings.s_sensor.DrawHeatSensor = !Settings.s_sensor.DrawHeatSensor;
                break;

            case Keys.V:
                // "C" cone part of heat sensor
                Settings.s_debug.ABMWriteVelocity = !Settings.s_debug.ABMWriteVelocity;
                break;
        }
    }

    /// <summary>
    /// Saves the AI training data to %APPDATA%/MissileDefenceAI, then notifies user via message box.
    /// </summary>
    private static void SaveTrainedModel()
    {
        // each file is saved as a .AI file.
        foreach (int id in NeuralNetwork.s_networks.Keys)
        {
            NeuralNetwork nn = NeuralNetwork.s_networks[id];
            nn.Save(Path.Combine(Program.aiFolder, $"missile{nn.Id}.ai"));
        }

        MessageBox.Show("AI model saved");
    }

    /// <summary>
    /// Initialises the neural network (one per ABM).
    /// </summary>
    internal static void InitialiseTheNeuralNetworksForTheABMs()
    {
        NeuralNetwork.s_networks.Clear();

        for (int abmId = 0; abmId < Settings.s_aI.NumberOfABMsToCreate; abmId++)
        {
            _ = new NeuralNetwork(abmId, NeuralNetwork.Layers);
        }
    }

    /// <summary>
    /// When training it is in "non-responsive" mode (i.e. not processing Windows messages frequently).
    /// If we catch the closing, and set a flag it will exit that non responsive loop.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        TrainingController.StopImmediately();
    }

    /// <summary>
    /// We hide the cursor for the player 2 display as we draw an MC like cross instead.
    /// But we don't hide the cursor except if they are within the region - otherwise
    /// it gets really annoying to the user!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CanvasPlayer2_MouseEnter(object sender, EventArgs e)
    {
        Cursor.Hide();
    }

    /// <summary>
    /// Show the cursor as they stop mouse over of the display. The cross MC style cursor
    /// will remain wherever it exited.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CanvasPlayer2_MouseLeave(object sender, EventArgs e)
    {
        Cursor.Show();
    }
}
