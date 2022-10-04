using MissileDefence.Controllers.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Attackers.Training;

/// <summary>
/// A class that provides specific data points.
/// </summary>
internal static class TrainingDataPoints
{
    /// <summary>
    /// List of training points (start/end)
    /// </summary>
    private static List<ICBMEndPoints>? trainingPoints = null;

    /// <summary>
    /// Array of training points (start/end)
    /// </summary>
    private static ICBMEndPoints[]? targets;

    /// <summary>
    /// Index into list of training points
    /// </summary>
    internal static int TrainingDataIndex = -1;

    /// <summary>
    /// Points chosen not at random, but to encourage the training to promote networks that are versatile (capable
    /// of hitting things near and far including wide).
    /// </summary>
    /// <returns></returns>
    internal static ICBMEndPoints[] GenerateTrainingPoints()
    {
        trainingPoints = new List<ICBMEndPoints>();

        // training points that teach the AI to aim in all directions, alternating direction both horizontally and vertically.
        // feel free to experiment.

        for (int y = 231; y > 201; y -= 10)
        {
            for (int a = 3; a < 63; a += 10)
            {
                for (int x = 50; x > 0; x -= 7)
                {
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 + x, y), new PointA(128 + x + a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 - x, y), new PointA(128 - x - a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 + x, y), new PointA(128 + x - a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 - x, y), new PointA(128 - x + a, 10)));

                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 + x, y - 50), new PointA(128 + x + a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 - x, y - 50), new PointA(128 - x - a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 + x, y - 50), new PointA(128 + x - a, 10)));
                    trainingPoints.Add(new ICBMEndPoints(new PointA(128 - x, y - 50), new PointA(128 - x + a, 10)));
                }
            }
        }

        // we need an array so we can step thru them sequentially
        ICBMEndPoints[] arrayOfPoints = trainingPoints.ToArray();

        return arrayOfPoints;
    }

    /// <summary>
    /// Return the next training point, unless "repeat" is set in which case we return the same as last time
    /// </summary>
    /// <param name="repeat"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <exception cref="Exception"></exception>
    internal static void GetPoints(bool repeat, out PointA start, out PointA end)
    {
        if (trainingPoints is null) targets = GenerateTrainingPoints();

        if (targets is null) throw new Exception("targets incorrectly initialised");

        if (TrainingDataIndex >= targets.Length - 1)
        {
            int rnd = RandomNumberGenerator.GetInt32(0, 30);
           
            int pos;

            if (RandomNumberGenerator.GetInt32(0, 30) > 15)
                pos = 128 - rnd * 13 % 127;
            else
                pos = 128 + rnd * 13 % 127;


            int altitude = (231 - RandomNumberGenerator.GetInt32(0, 22)).Clamp(85, 231 + 50);
            start = new PointA(pos, altitude);

            end = GameController.s_players[0].AllInfrastructureBeingDefended[RandomNumberGenerator.GetInt32(0, GameController.s_players[0].AllInfrastructureBeingDefended.Count)].LocationInMCCoordinates;

            return;
        }
    
        if (!repeat) ++TrainingDataIndex;

        if (targets is null || targets.Length < TrainingDataIndex) throw new Exception("target issue");

        start = targets[TrainingDataIndex].StartPoint;
        end = targets[TrainingDataIndex].EndPoint;
    }
}
