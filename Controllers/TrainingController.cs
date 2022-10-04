//#define logMutate
using MissileDefence.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.AI;
using MissileDefence.Controllers.Game;
using MissileDefence.Attackers.Training;
using MissileDefence.Attackers;
using MissileDefence.Defenders.Sensors;
using MissileDefence.Defenders;
using MissileDefence.UX;

namespace MissileDefence.Controllers;

/// <summary>
/// 
/// </summary>
internal class TrainingController
{
    /// <summary>
    /// 
    /// </summary>
    private static int s_missilesLaunched = 0;

    /// <summary>
    /// 
    /// </summary>
    internal static int s_targetsHit = 0;

    /// <summary>
    /// 
    /// </summary>
    internal static float[] s_killRatio = new float[Settings.s_aI.NumberOfABMsToCreate];

    /// <summary>
    /// 
    /// </summary>
    internal static float[] s_missRatio = new float[Settings.s_aI.NumberOfABMsToCreate];

    internal ICBM? trainingICBM;

    /// <summary>
    /// Difference in X (base vs. missile) is the 1st int, second is number of times it hit.
    /// </summary>
    static readonly Dictionary<int, int> s_hit = new();

    /// <summary>
    /// Difference in X (base vs. missile) is the 1st int, second is number of times it missed.
    /// </summary>
    static readonly Dictionary<int, int> s_miss = new();

    /// <summary>
    /// This is the generation of the AI neural network in general. Each time a mutation occurs it increases.
    /// </summary>
    internal static int s_generation = 1;

    /// <summary>
    /// This is the canvas, we are drawing the game / training.
    /// </summary>
    private readonly PictureBox? CanvasGame;

    /// <summary>
    /// This is where we'll write stats and other good stuff.
    /// </summary>
    private readonly PictureBox? CanvasStats;

    /// <summary>
    /// Track the rockets we added.
    /// </summary>
    internal static readonly Dictionary<int, ABM> s_abms = new();

    /// <summary>
    /// Width of the canvas, to save accessing it repeatedly.
    /// </summary>
    private readonly int width;

    /// <summary>
    /// Height of the canvas, to save accessing it repeatedly.
    /// </summary>
    private readonly int height;

    /// <summary>
    /// Number of generations we silently compute.
    /// </summary>
    private static int s_silentGenerationsRemaining;

    /// <summary>
    /// Set to true to ensure it exits quiet mode.
    /// </summary>
    private static bool stopNow = false;

    /// <summary>
    /// 
    /// </summary>
    private static bool lastHit = true;

    /// <summary>
    /// Constructor, attaches the emulation to a canvas.
    /// </summary>
    /// <param name="canvas"></param>
    internal TrainingController(PictureBox canvasPlayer1, PictureBox canvasPlayer2)
    {
        CanvasGame = canvasPlayer1;
        CanvasStats = canvasPlayer2; // we'll use Player 2's screen for stats.

        CanvasGame.Image = new Bitmap(512, 462);
        CanvasStats.Image = new Bitmap(512, 462);

        width = CanvasGame.Width;
        height = CanvasGame.Height;

        s_missilesLaunched = 0;
        s_targetsHit = 0;

        // data that affects when a mutation occurs
        s_silentGenerationsRemaining = Settings.s_aI.GenerationsToTrainBeforeShowingVisuals;

        InitialiseTheNeuralNetworksForABMs();

        LaunchICBM();
    }

    /// <summary>
    /// Forces the rockets AI to mutate.
    /// </summary>
    internal void ForceMutate()
    {
        GameController.Pause();

        MutateABMs();

        LaunchICBM();

        GameController.Unpause();
    }

    /// <summary>
    /// In quiet mode, it doesn't animate rockets.
    /// </summary>
    internal static void ToggleQuietMode()
    {
        if (s_silentGenerationsRemaining > 0) s_silentGenerationsRemaining = 0; else s_silentGenerationsRemaining = 100000;
    }

    /// <summary>
    /// Allows you to determine if in quiet mode or not.
    /// </summary>
    /// <returns></returns>
    internal static bool InQuietMode()
    {
        return s_silentGenerationsRemaining > 0;
    }

    /// <summary>
    /// This empties our rocket dictionary, and recreates them.
    /// We use it during mutation to put the rockets back to the start
    /// and ensure no data they contain persists past mutation.
    /// </summary>
    private void LaunchICBM()
    {
        // reset every 100, otherwise for every miss it becomes increasingly more impossible to reach 100% accuracy
        if (s_generation % 100 == 0)
        {
            s_missilesLaunched = 0;
            s_targetsHit = 0;
        }

        // get rid of any explosions
        GameController.s_players[0].explosionManager.Clear();

        TrainingDataPoints.GetPoints(!lastHit, out PointA start, out PointA end);

        lastHit = false;

        s_abms.Clear();
        ABMSilo targetBase = GameController.s_players[0].Silos[1]; // center

        trainingICBM = new ICBM(start,
                                end,
                                BaseHit,
                                SharedUX.s_uxColorsPerLevel[GameController.s_UXWaveTheme].ICBMColour);
        // trainingICBM.MoveDisabled = true;

        // all missiles launch from a base, and attack a random missile
        for (int id = 0; id < Settings.s_aI.NumberOfABMsToCreate; id++)
        {
            ++s_missilesLaunched; // one missile launched by ABM we create

            double launchAngle = 0; ABMSilo.LaunchAngle(targetBase, trainingICBM);

            s_abms.Add(id, new ABMCPUInTraining(id,
                                                targetBase,
                                                trainingICBM,
                                                launchAngle,
                                                Color.FromArgb((10 + id * 971) % 255, id * 113 % 255, id * 211 % 255),
                                                HitCallBack));
            s_abms[id].LaunchAngle = launchAngle;
        }
    }

    private static void HitCallBack(ABM abm)
    {
        ///
    }

    private void BaseHit(BasesBeingDefended? baseHit)
    {

    }

    /// <summary>
    /// Each time a tick occurs, we move the rocket. It also switches to quiet mode, whilst remembering
    /// to "DoEvents()" frequently enough to stop the UI locking up (this is running on the UI thread).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    internal void TimerRocketMove_Tick()
    {
        if (s_silentGenerationsRemaining > 0)
        {
            // if in "silent" training, we avoid a timer to achieve better throughput.
            // i.e. with timer, we move once every 10ms (or whatever it is set to), where as a fixed loop
            // means we move as quick as possible and rattle through mutations.
            ContinuouslyMoveEverythingWithoutUpdatingUIToImprovePerformance();
            return;
        }

        bool atLeastOneRemaining = MoveABMs();

        trainingICBM?.Move();

        DrawEverything(atLeastOneRemaining);

        if (atLeastOneRemaining) return; // GIVE ALL A CHANCE && !badGuyMissileInterceptedOrHitBase

        UpdateHitOrMiss();

        // all are eliminated
        GameController.Pause();
        MutateABMs();

        // pause whilst avoiding being unresponsive
        for (int idx = 0; idx < 20; idx++)
        {
            Application.DoEvents();

            if (stopNow) return;

            Thread.Sleep(200);
        }

        GameController.Unpause();

        LaunchICBM();
    }

    /// <summary>
    /// Tracks accuracy of the CPU training
    /// </summary>
    private static void UpdateHitOrMiss()
    {
        foreach (int id in s_abms.Keys)
        {
            int x = (s_abms[id].LocationTargetX - s_abms[id].LaunchAngleX) / 100;

            if (s_abms[id].EliminatedBecauseOf == "hit")
            {
                lastHit = true;
                if (!s_hit.ContainsKey(x)) s_hit.Add(x, 0);
                s_hit[x]++;
            }
            else
            {
                if (!s_miss.ContainsKey(x)) s_miss.Add(x, 0);
                s_miss[x]++;
            }
        }
    }

    /// <summary>
    /// Draws everything.
    /// </summary>
    /// <param name="oneRemaining"></param>
    /// <exception cref="Exception"></exception>
    private void DrawEverything(bool oneRemaining)
    {
        if (CanvasGame is null) throw new Exception("CanvasGame must be initialised before use.");
        if (CanvasStats is null) throw new Exception("CanvasStats must be initialised before use.");

        Bitmap bitmapDebug = new(CanvasStats.Image);
        using Graphics gDebug = Graphics.FromImage(bitmapDebug);
        gDebug.Clear(Color.Black);

        Bitmap bitmapCanvas = new(CanvasGame.Image);

        using Graphics g = Graphics.FromImage(bitmapCanvas);
        g.Clear(Color.Black);
        GameController.s_players[0].Draw(g);

        // quality graphics
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        // draw each rocket. "best" is used to circle the best.
        foreach (int id in s_abms.Keys)
        {
            s_abms[id].DrawPaths(g); // draws a box with velocity arrows and booster flame.
        }

        string rocketDebug = "\"Q\" Quiet Mode  | \"S\" Slow Mode | \"P\" Pause | \"H\" Heat sensor\n" +
                             "\"C\" Sensor Cone | \"Ctrl-S\" Save AI model \n\n" +
                             "DEBUG: " + (oneRemaining ? "Intercepting" : "Results") + "\n";

        // rockets are drawn after lines
        foreach (int id in s_abms.Keys)
        {
            rocketDebug += $"id: {id:00}" +
                           $" | {ReadableHeatSensor(((ABMCPUControlled)s_abms[id]).LastHeatSensorOutput, s_abms[id].EliminatedBecauseOf == "hit")}" +
                           $" | NN: {((ABMCPUControlled)s_abms[id]).OutputFromNeuralNetwork[0]:0.000}" +
                           $" | gim: {((ABMCPUControlled)s_abms[id]).OffsetAngleOfThrustInDegrees:000.0}" +
                           $" | {s_abms[id].EliminatedBecauseOf}\n";

            s_abms[id].Draw(g); // draws a box with velocity arrows and booster flame.
        }

        using Font f = new("Lucida Console", 6);
        gDebug.DrawString(rocketDebug, f, Brushes.White, 0, 90);

        trainingICBM?.Draw(g);

        UpdateScore(gDebug);

        g.Flush();
        gDebug.Flush();

        // switch last image with this one.
        CanvasGame.Image?.Dispose();
        CanvasGame.Image = bitmapCanvas;

        CanvasStats.Image?.Dispose();
        CanvasStats.Image = bitmapDebug;
    }

    /// <summary>
    /// Provides data on the heat sensor.
    /// </summary>
    /// <param name="sensor"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    private static string ReadableHeatSensor(List<double> sensor, bool hit)
    {
        string result = "";

        int midPoint = sensor.Count / 2;
        int thisPoint = -1;

        foreach (double sensorValue in sensor)
        {
            ++thisPoint;

            if (hit)
            {
                result += @"*";
                continue;
            }

            if (sensorValue == 0)
            {
                result += "^";
                continue;
            }

            if (sensorValue == IRSensorLeftRight.c_nolock)
            {
                if (thisPoint == midPoint) result += ":"; else result += " ";
                continue;
            }

            result += "X";
        }

        return result;
    }

    /// <summary>
    /// Updates training stats.
    /// </summary>
    /// <param name="g"></param>
    private static void UpdateScore(Graphics g)
    {
        string mut = Settings.s_aI.Mutate50pct ? "50%" : Math.Round(100D * (Settings.s_aI.NumberOfABMsToCreate - (double)NumberToPreserve) / Settings.s_aI.NumberOfABMsToCreate).ToString() + "%";
        string label = $"Generation: {s_generation}  Mutation: {mut}  Kills: {s_targetsHit}/{s_missilesLaunched}  Accuracy: {(s_missilesLaunched == 0 ? 0 : s_targetsHit * 100 / s_missilesLaunched)}%  Train #: {TrainingDataPoints.TrainingDataIndex}";

        if (InQuietMode()) label += "\nPress \"Q\" to exit quiet mode.";

        using Font f = new("Segoe UI", 10);
        g.DrawString(label, f, Brushes.White, 0, 0);

        if (InQuietMode()) return;

        using Font f2 = new("Segoe UI", 7);
        string hits = "";

        foreach (float amt in s_killRatio) hits += " " + Math.Round(amt, 1).ToString("###0.0");
        
        SizeF size = g.MeasureString(hits, f2, new SizeF(500, 80));
        StringFormat sf = new()
        {
            FormatFlags = StringFormatFlags.FitBlackBox
        };
        g.DrawString(hits, f2, Brushes.White, new RectangleF(0, 40, size.Width, size.Height), sf);
        
        // we write stats every second if required.
        if (s_generation % 1000 == 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("xdist,count,result");

            foreach (int dist in s_hit.Keys)
            {
                sb.AppendLine($"{dist},{s_hit[dist]},hit");
            }

            foreach (int dist in s_miss.Keys)
            {
                sb.AppendLine($"{dist},{s_miss[dist]},miss");
            }

            //File.WriteAllText(@"c:\temp\hitormiss.csv", sb.ToString());
        }
    }

    /// <summary>
    /// Quiet mode. 
    /// Rockets are moved, and ed when they are all eliminated (rule breaking or run out of fuel).
    /// </summary>
    /// <returns></returns>
    private void ContinuouslyMoveEverythingWithoutUpdatingUIToImprovePerformance()
    {
        GameController.Pause(); // we are running in a high-speed loop, not relying on the timer

        if (trainingICBM is null) throw new Exception(nameof(trainingICBM) + " shouldn't be null, it's the ICBM we a training from");
        int counter = 0;

        // whilst in quiet mode
        while (s_silentGenerationsRemaining > 0)
        {
            if (stopNow) return;

            bool atLeastOneRemaining = MoveABMs();

            if (counter++ % 100 == 0) Application.DoEvents(); // yield frequently enough to avoid UI locking up

            if (!trainingICBM.Move()) s_targetsHit += 0;

            if (atLeastOneRemaining) continue; //&& !badGuyMissileInterceptedOrHitBase

            UpdateHitOrMiss();

            MutateABMs();

            // extend time between mutation
            LaunchICBM();

            --s_silentGenerationsRemaining;

            ShowGeneration(); // in the center of the screen, so user knows we're doing "something"
        }

        GameController.Unpause(); // we are no longer in quiet mode, rely on timer to move
    }

    /// <summary>
    /// Stops quiet mode.
    /// </summary>
    internal static void StopImmediately()
    {
        stopNow = true;
    }

    /// <summary>
    /// Paint the scenery without rockets, and add "Generation xxx" to the middle
    /// </summary>
    /// <exception cref="Exception"></exception>
    private void ShowGeneration()
    {
        if(CanvasStats is null) throw new NullReferenceException(nameof(CanvasStats)+" shouldn't be null");
        Bitmap bitmapCanvas = new(CanvasStats.Image); // start with our pre-drawn image and overlay rockets. we "clone" image, as Dispose will kill original

        using Graphics g = Graphics.FromImage(bitmapCanvas);
        g.Clear(Color.Black);

        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

        string label = $"Generation {s_generation}";
        using Font f = new("Arial", 28);
        SizeF size = g.MeasureString(label, f);
        g.DrawString(label, f, Brushes.White, width / 2 - size.Width / 2, height / 2 - size.Height / 2);

        UpdateScore(g);
        if (CanvasGame is null) return; // should NEVER happen, but just in case

        g.Flush();

        CanvasGame.Image?.Dispose();
        CanvasGame.Image = bitmapCanvas;

        Application.DoEvents(); // allow canvas change to paint
    }

    /// <summary>
    /// Initialises the neural network (one per rocket).
    /// </summary>
    internal static void InitialiseTheNeuralNetworksForABMs()
    {
        NeuralNetwork.s_networks.Clear();

        for (int rocketId = 0; rocketId < Settings.s_aI.NumberOfABMsToCreate; rocketId++)
        {
            _ = new NeuralNetwork(rocketId, NeuralNetwork.Layers);
        }
    }

    private static void MutateLog(string text)
    {
#if logMutate
        Debug.WriteLine(text);
#endif
    }

    internal static int NumberToPreserve = 0;

    /// <summary>
    /// Replaces rockets with better ones (with slight mutation)
    /// </summary>
    /// <param name="best">List of id's of top rockets.</param>
    private static void MutateABMs()
    {
        ++s_generation;

        const float punishmentForNotHitting = 0.01F;

        int cnt = 0;
        bool hasHitICBMWithOneOrMore = false;

        // update networks fitness for each rocket
        foreach (int id in s_abms.Keys)
        {
            if (s_abms[id].EliminatedBecauseOf == "hit") hasHitICBMWithOneOrMore = true;

            NeuralNetwork.s_networks[id].Fitness = AIScoring.Fitness(s_generation, s_abms[id], s_killRatio[id]);

            if (NeuralNetwork.s_networks[id].Fitness > 0) ++cnt;
        }

        foreach (int id in s_abms.Keys)
        {
            if (s_abms[id].EliminatedBecauseOf != "hit" && s_killRatio[id] > 10)
            {
                s_killRatio[id] = (s_killRatio[id] - punishmentForNotHitting * s_missRatio[id] / s_killRatio[id]).Clamp(0, float.MaxValue); // punish for not hitting ensures it doesn't always take this as the best
            }
            else
            {
                s_missRatio[id] = 0; // reset if hit
            }
        }

        if (cnt == 0)
        {
            // won't hit this currently (except when none hit at the start)
            MutateLog("No hits, no sort, nothing");

            InitialiseTheNeuralNetworksForABMs(); // jumble 'em up

            return;
        }

        OutputNN(NeuralNetwork.s_networks, "Before Sort");

        NeuralNetwork.SortNetworkByFitness(); // largest "fitness" (best performing) goes to the bottom

        OutputNN(NeuralNetwork.s_networks, "After Sort");

        OutputKillRatio();

        List<NeuralNetwork> n = new();
        foreach (int n2 in NeuralNetwork.s_networks.Keys) n.Add(NeuralNetwork.s_networks[n2]);

        NeuralNetwork[] array = n.ToArray();

        if (!hasHitICBMWithOneOrMore || Settings.s_aI.Mutate50pct)
        {
            MutateLog("Mutate top 50%");

            // replace the 50% worse offenders with the best, then mutate them.
            // we do this by copying top half (lowest fitness) with top half.
            for (int worstNeuralNetworkIndex = 0; worstNeuralNetworkIndex < Settings.s_aI.NumberOfABMsToCreate / 2; worstNeuralNetworkIndex++)
            {
                // 50..100 (in 100 neural networks) are in the top performing
                int neuralNetworkToCloneFromIndex = worstNeuralNetworkIndex + Settings.s_aI.NumberOfABMsToCreate / 2; // +50% -> top 50% 
                MutateLog($"Copy from id: {neuralNetworkToCloneFromIndex} to id: {worstNeuralNetworkIndex}");

                NeuralNetwork.CopyFromTo(array[neuralNetworkToCloneFromIndex], array[worstNeuralNetworkIndex]); // copy
                MutateLog($"Killing {array[worstNeuralNetworkIndex].Id}");

                if (hasHitICBMWithOneOrMore)
                {
                    s_killRatio[array[worstNeuralNetworkIndex].Id] = s_killRatio[array[neuralNetworkToCloneFromIndex].Id] * 0.9F; // mutation resets it
                    s_missRatio[array[worstNeuralNetworkIndex].Id] = 0; // mutation resets it
                    array[worstNeuralNetworkIndex].Mutate(12, 0.25F); // mutate
                }
                else
                {
                    s_killRatio[array[worstNeuralNetworkIndex].Id] = s_killRatio[array[neuralNetworkToCloneFromIndex].Id] * 0.8F; // mutation resets it
                    s_missRatio[array[worstNeuralNetworkIndex].Id] = 0; // mutation resets it
                    array[worstNeuralNetworkIndex].Mutate(25, 0.5F); // mutate
                }
            }
        }
        else
        {
            NumberToPreserve = Settings.s_aI.NumberOfABMsToCreate / 2 + s_generation / 1000;
            NumberToPreserve = MathUtils.Clamp(NumberToPreserve, 2, Settings.s_aI.NumberOfABMsToCreate - 2);

            MutateLog($"Mutate all but {NumberToPreserve}");

            int offset = 0;
            // replace all but top rocket with a mutated version of the best rocket
            // mutation chance and strength is halved.
            for (int worstNeuralNetworkIndex = 0; worstNeuralNetworkIndex < Settings.s_aI.NumberOfABMsToCreate - NumberToPreserve; worstNeuralNetworkIndex++)
            {
                // 50..100 (in 100 neural networks) are in the top performing
                int neuralNetworkToCloneFromIndex = Settings.s_aI.NumberOfABMsToCreate - 1 - offset; // top rocket

                MutateLog($"Copy from id: {neuralNetworkToCloneFromIndex} to id: {worstNeuralNetworkIndex}");
                NeuralNetwork.CopyFromTo(array[neuralNetworkToCloneFromIndex], array[worstNeuralNetworkIndex]); // copy

                MutateLog($"Killing {array[worstNeuralNetworkIndex].Id}");

                offset++;
                if (offset >= NumberToPreserve) offset = 0;

                s_killRatio[array[worstNeuralNetworkIndex].Id] = s_killRatio[array[neuralNetworkToCloneFromIndex].Id] * 0.8F; // mutation resets it
                s_missRatio[array[worstNeuralNetworkIndex].Id] = 0; // mutation resets it

                array[worstNeuralNetworkIndex].Mutate(25, 0.5F); // mutate
            }

        }

        // unsort, restoring the order of rocket to neural network i.e [x]=id of "x".
        Dictionary<int, NeuralNetwork> unsortedNetworksDictionary = new();

        for (int rocketIndex = 0; rocketIndex < Settings.s_aI.NumberOfABMsToCreate; rocketIndex++)
        {
            var neuralNetwork = NeuralNetwork.s_networks[rocketIndex];

            MutateLog($"unsortedNetworksDictionary[{neuralNetwork.Id}] = neuralNetwork.id ({neuralNetwork.Id})");
            unsortedNetworksDictionary[neuralNetwork.Id] = neuralNetwork;
        }

        OutputNN(unsortedNetworksDictionary, "After reverse sort");

        NeuralNetwork.s_networks = unsortedNetworksDictionary;
        OutputNN(NeuralNetwork.s_networks, "After mutate");

        OutputKillRatio();
    }

    private static void OutputKillRatio()
    {
#if logMutate
        int x = 0;
        foreach(int kill in killRatio)
        {
            MutateLog($"{x++}. kills {kill}");
        }
#endif
    }

    private static void OutputNN(Dictionary<int, NeuralNetwork> nn, string label)
    {
#if logMutate
        MutateLog("\n"+label);
        foreach (int id in nn.Keys) MutateLog($"{id}. {nn[id].Fitness} hash {nn[id].Hash()}");
#endif
    }

    /// <summary>
    /// Moves the rockets either using "parallel" or serial. 
    /// </summary>
    private static bool MoveABMs()
    {
        if (Settings.s_world.UseParallelMove)
        {
            // this should run much faster (multi-threading). Particularly good if AI is lots of neurons
            Parallel.ForEach(s_abms.Keys, id =>
            {
                ABM r = s_abms[id];

                if (!r.IsEliminated) r.Move();
            });
        }
        else
        {
            foreach (int id in s_abms.Keys)
            {
                ABM missile = s_abms[id];

                if (!missile.IsEliminated) missile.Move();
            }
        }

        // we return true if there are rockets not eliminated that can be moved
        foreach (var id in s_abms.Keys)
        {
            ABM r = s_abms[id];

            if (!r.IsEliminated) return true;
        }

        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="interceptMissile"></param>
    internal static void Launch(int id, ABM interceptMissile)
    {
        s_abms[id] = interceptMissile;
    }
}
