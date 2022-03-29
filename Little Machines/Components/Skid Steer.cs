using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class SkidSteer : Component
        {
            public Blocks<IMyMotorStator> wheels = new Blocks<IMyMotorStator>();
            Dictionary<long, float> wheelDots;

            public float Smoothing = 0.1f;
            string wheelQuery = "Drive Wheels";
            public float Speed = 40;

            float forwardInput;
            float turnInput;

            public override void Start(IMyShipController cockpit)
            {
                // Get the wheels.
                wheels.GetBlocks(lm.GridTerminalSystem, wheelQuery);
                Log($"Found {wheels.Count} Wheels");

                if (wheels.Count < 0)
                    throw new Exception("No wheels found.");

                // Get a vector that points to the right of the cockpit.
                var right = cockpit.WorldMatrix.Right;
                // Get the center of mass.
                var com = cockpit.CenterOfMass;

                // Fill a dict. of the dot product of each wheel from the center of mass.
                wheelDots = new Dictionary<long, float>(wheels.Count);
                foreach (IMyMotorStator wheel in wheels)
                {
                    var posDot = (com - wheel.GetPosition()).Dot(right);
                    var dirDot = Math.Abs(wheel.WorldMatrix.Up.Dot(right));
                    wheelDots.Add(wheel.EntityId, (float)posDot);
                }
                Log($"Found: {wheelDots.Count} wheels.");
            }

            public override void Stop()
            {
                // Stop all the wheels from spinning.
                wheels.Set("Velocity", 0f);
            }

            public override void Tick()
            {
                forwardInput = MathHelper.Lerp(forwardInput, lm.GetAxis(Axis.Forward), Smoothing);
                turnInput = MathHelper.Lerp(turnInput, lm.GetAxis(Axis.Horizontal), Smoothing);

                foreach (IMyMotorStator wheel in wheels)
                {
                    float dot = wheelDots[wheel.EntityId];
                    wheel.TargetVelocityRPM = (turnInput * -dot + forwardInput) * dot * Speed;
                }
            }

            public override void ReadCfg(MyIni ini)
            {
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                wheelQuery = ini.Get(section, "wheels").ToString(wheelQuery);
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "speed", Speed);
                ini.Set(section, "wheels", wheelQuery);
            }
        }
    }
}
