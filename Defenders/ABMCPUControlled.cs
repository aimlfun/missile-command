using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissileDefence.AI;
using MissileDefence.Configuration;
using MissileDefence.Controllers.Game;
using MissileDefence.Attackers;
using MissileDefence.Controllers;
using MissileDefence.Defenders.Sensors;

namespace MissileDefence.Defenders
{
    internal class ABMCPUControlled : ABM
    {
        /// <summary>
        /// 
        /// </summary>
        internal ICBM activeTarget;

        /// <summary>
        /// 
        /// </summary>
        internal IRSensorLeftRight IRsensor = new();

        /// <summary>
        /// 
        /// </summary>
        internal double[] OutputFromNeuralNetwork;

        /// <summary>
        /// 
        /// </summary>
        internal List<double> LastHeatSensorOutput;

        /// <summary>
        /// 
        /// </summary>
        double vel = 0;

        /// <summary>
        /// 
        /// </summary>
        double offsetAngle = 0;
        
        /// <summary>
        /// 
        /// </summary>
        double angleABMIsPointing = 0;
        
        /// <summary>
        /// 
        /// </summary>
        double x = 0, y = 0;

        internal double OffsetAngleOfThrustInDegrees
        {
            set { offsetAngle = value; }
            get { return offsetAngle; }
        }

        /// <summary>
        /// 
        /// </summary>
        internal double AnglePointingDegrees
        {
            get
            {
                return angleABMIsPointing;
            }

            set
            {
                angleABMIsPointing = value;
            }
        }

        /// <summary>
        /// Amount of force from igniting the thruster.
        /// </summary>
        private double burnForce = 0;

        /// <summary>
        /// Constructor for a CPU controlled ABM.
        /// </summary>
        /// <param name="missileId"></param>
        /// <param name="sentFromBase"></param>
        /// <param name="icbm"></param>
        /// <param name="angle"></param>
        /// <param name="smokeColour"></param>
        /// <param name="callback"></param>    
        internal ABMCPUControlled(int missileId, BasesBeingDefended sentFromBase, ICBM icbm, double angle, Color smokeColour, OnTargetHit callback) : base(missileId, smokeColour)
        {
            TargetHit += ABMCPUControlled_HitTarget;
            Elimination += ABMCPUControlled_Elimination;
            RegisterHit += callback;

            LocationInMCCoordinates = new PointA(sentFromBase.LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX, 21);
            AnglePointingDegrees = MathUtils.Clamp360(angle);
            LaunchAngle = AnglePointingDegrees;

            locationLastInMCCoordinates = LocationInMCCoordinates;

            PointA icbmLocation = PointA.DeviceCoordinatesToMC(icbm.LocationInDeviceCoordinates);

            LocationTargetX = (int)icbmLocation.HorizontalInMissileCommandDisplayPX;
            LaunchAngleX = (int)LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX;
            activeTarget = icbm;

            LastHeatSensorOutput = new List<double>();
            OutputFromNeuralNetwork = Array.Empty<double>();

            x = sentFromBase.LocationInMCCoordinates.HorizontalInMissileCommandDisplayPX;
            y = LocationInMCCoordinates.AltitudeInMissileCommandDisplayPX;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abm"></param>
        /// <param name="reason"></param>
        private void ABMCPUControlled_Elimination(ABM abm, string reason)
        {
            IRsensor.Clear(); // so we don't paint triangle when the missile is dead.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="abm"></param>
        private void ABMCPUControlled_HitTarget(ABM abm)
        {
            if (TrainingController.s_killRatio is not null) TrainingController.s_killRatio[NeuralNetwork.s_networks[Id].Id]++;

            if (activeTarget != null)
            {
                activeTarget.Killed();

                if (!TrainingController.InQuietMode()) GameController.s_players[0].explosionManager.Add(activeTarget.LocationInDeviceCoordinates);
            }
        }

        /// <summary>
        /// Determines how close to target
        /// </summary>
        /// <returns></returns>
        internal override float DistanceFromActiveTarget()
        {
            if (activeTarget is null) return int.MaxValue;

            float dx = activeTarget.LocationInDeviceCoordinates.X - LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP().X;  // <===
            float dy = activeTarget.LocationInDeviceCoordinates.Y - LocationInMCCoordinates.MCCoordsToDeviceCoordinatesP().Y;  // <===

            double dist = Math.Sqrt(dx * dx + dy * dy);

            return (float)dist;
        }

        /// <summary>
        /// Moves the rocket (applying velocity, and acceleration via AI).
        /// </summary>
        internal override void GuideMissile()
        {
            burnForce = 5;

            // OUTPUT: angle
            double[] neuralNetworkInput = IRsensor.Read(AnglePointingDegrees, LocationInMCCoordinates, PointA.DeviceCoordinatesToMC(activeTarget.LocationInDeviceCoordinates), out double[] lastHeatSensorArray); // heat LIDAR input
            LastHeatSensorOutput = new(lastHeatSensorArray);

            // ask the neural to use the input and decide what to do with the rocket
            OutputFromNeuralNetwork = NeuralNetwork.s_networks[Id].FeedForward(neuralNetworkInput); // process inputs

            // adjustment of angle
            OffsetAngleOfThrustInDegrees = OutputFromNeuralNetwork[0] * Settings.s_aI.AIthrusterNozzleAmplifier; //degrees

            // stop it going too quick
            if (Speed > 14) burnForce *= 0.9;
            if (Speed > 16) burnForce *= 0.8;

            Speed += burnForce * 0.003F;

            angleABMIsPointing += OffsetAngleOfThrustInDegrees * 0.05;

            angleABMIsPointing = MathUtils.Clamp360(angleABMIsPointing);

            double angleInRadians = MathUtils.DegreesInRadians(angleABMIsPointing);

            x += Math.Sin(angleInRadians) * Speed;
            y += Math.Cos(angleInRadians) * Speed;

            LocationInMCCoordinates = new PointA((int)x, (int)y);

            if (EliminatedBecauseOf is null && !IRsensor.IsInSensorSweepTriangle)
            {
                EliminatedBecauseOf = "nolock"; // lost sight of target
            }
        }

        /// <summary>
        /// Track velocity/speed.
        /// </summary>
        internal double Speed
        {
            set { vel = value; }
            get { return vel; }
        }

        /// <summary>
        /// Draw the ABM with optional debug can be turned on via the UI / settings to visually see what's happening.
        /// </summary>
        /// <param name="g"></param>
        internal override void Draw(Graphics g)
        {
            base.Draw(g);

            // rarely used, shows the speed of each ABM
            if (Settings.s_debug.ABMWriteVelocity)
            {
                g.DrawString($"{Speed:##.#}", new Font("Arial", 8), Brushes.White, LocationInMCCoordinates.MCCoordsToDeviceCoordinates());
            }

            // draws all the heat sensor segments
            if (Settings.s_sensor.DrawHeatSensor)
            {
                // for training we want each sweep to match the IBM color, so we can identify which missile.
                // for combat, we don't need to; in fact mostly there is one ABM enroute at a time! (yes, it's that efficient / accurate)
                Color c = this is ABMCPUInTraining ? Color.FromArgb(1, (20 + Id * 971) % 255, Id * 113 % 255, Id * 211 % 255) : Color.FromArgb(30, 255, 255, 255);
                IRsensor.DrawFullSweepOfHeatSensor(g, c);
            }

            // draws the part of the heat sensor the target is within
            if (Settings.s_sensor.DrawTargetPartOfHeatSensor)
            {
                using SolidBrush colorOfTargetLocationSegment = new(Color.FromArgb(this is ABMCPUInTraining ? 3 : 30, 255, 0, 0));

                IRsensor.DrawWhereTargetIsInRespectToSweepOfHeatSensor(g, colorOfTargetLocationSegment);
            }
        }
    }
}