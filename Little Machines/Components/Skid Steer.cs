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
            Dictionary<long, float> dots;

            float Smoothing = 10f;
            string wheelQuery = "Drive Wheels";
            float Speed = 40;

            private SmoothedAxis horizontalInput;
            private SmoothedAxis forwardInput;

            const string AUTO_DIR = "auto";
            private string direction = AUTO_DIR;
            readonly Dictionary<string, Vector3> directions = new Dictionary<string, Vector3>
            {
                {AUTO_DIR, Vector3.Zero },
                {"+x", Vector3.UnitX },
                {"+y", Vector3.UnitY },
                {"+z", Vector3.UnitZ },
                {"-x", -Vector3.UnitX },
                {"-y", -Vector3.UnitY },
                {"-z", -Vector3.UnitZ },
            };

            public override void Start(IMyShipController cockpit)
            {
                // Get the wheels.
                wheels.GetBlocks(lm.GridTerminalSystem, wheelQuery);

                if (wheels.Count < 0)
                    throw new Exception("No wheels found.");

                // Get our direction.
                Vector3 dir;
                if (direction == AUTO_DIR)
                    dir = cockpit.WorldMatrix.Right;
                else
                    dir = Vector3.TransformNormal(directions[direction], wheels.First().CubeGrid.WorldMatrix);

                // Get the middle of all our wheels.
                var origin = Vector3D.Zero;
                foreach (var wheel in wheels)
                    origin += wheel.GetPosition();
                origin /= wheels.Count;

                // Save the dot product of each wheel for later.
                dots = new Dictionary<long, float>(wheels.Count);
                foreach (IMyMotorStator wheel in wheels) {
                    // Get the 
                    Vector3 delta = origin - wheel.GetPosition();
                    var normal = Vector3.ProjectOnVector(ref delta, ref dir);
                    normal = Vector3.ClampToSphere(normal * 2, 1f);

                    dots.Add(wheel.EntityId, normal.Dot(dir));
                }
                // When there's no wheels... let the user know.
                if (dots.Count == 0)
                    Log($"Found: {dots.Count} wheels.");
            }
            public override void Stop()
            {
                // Stop all the wheels from spinning.
                wheels.Set("Velocity", 0f);
            }

            public override void Tick()
            {
                float fwd = forwardInput.Get();
                float turn = -horizontalInput.Get();

                foreach (IMyMotorStator wheel in wheels)
                {
                    float dot = dots[wheel.EntityId];
                    wheel.TargetVelocityRPM = (turn + (fwd * dot)) * Speed;
                }
            }

            public override void ReadCfg(MyIni ini)
            {
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                wheelQuery = ini.Get(section, "wheels").ToString(wheelQuery);
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                var turn = lm.AxisFromString(ini.Get(section, "turn axis").ToString(Axis.Horizontal.ToString()));
                var drive = lm.AxisFromString(ini.Get(section, "drive axis").ToString(Axis.Forward.ToString()));
                horizontalInput = new SmoothedAxis(lm, turn, Smoothing);
                forwardInput = new SmoothedAxis(lm, drive, Smoothing);

                var dirText = ini.Get(section, "direction").ToString(AUTO_DIR).ToLower();
                if (!directions.ContainsKey(dirText))
                    throw new Exception($"Invalid value '{dirText}' for direction");
                direction = dirText;
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "speed", Speed);
                ini.Set(section, "wheels", wheelQuery);
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "turn axis", Axis.Horizontal.ToString());
                ini.Set(section, "drive axis", Axis.Forward.ToString());
                ini.Set(section, "direction", AUTO_DIR);
            }
        }
    }
}
