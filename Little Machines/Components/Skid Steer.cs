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
        public class SkidSteer : Component, ILocomotion
        {
            public Function<IMyMotorStator> wheels = new Function<IMyMotorStator>("wheels")
            {
                query = "Wheels",
                speed = 40f
            };
            Dictionary<long, float> wheelDots;

            public float Smoothing = 0.1f;

            public float BreakingTorque = 40000;

            float forwardInput;
            float turnInput;

            public float MaxSpeed
            {
                get { return wheels.speed; }
            }

            public override void Start(IMyShipController cockpit)
            {
                // Get the wheels.
                wheels.Initialize(Terminal);
                Log($"Found {wheels.Blocks.Count} Wheels");

                if (wheels.Blocks.Count < 0)
                    throw new Exception("No wheels found.");

                //Log($"Using '{cockpit.CustomName}' as direction of grid.");
                var right = cockpit.WorldMatrix.Right;
                var com = cockpit.CenterOfMass;

                wheelDots = new Dictionary<long, float>(wheels.Blocks.Count);
                foreach (var wheel in wheels.Blocks)
                {
                    var posDot = (com - wheel.GetPosition()).Dot(right);
                    var dirDot = Math.Abs(wheel.WorldMatrix.Up.Dot(right));
                    wheelDots.Add(wheel.EntityId, (float)posDot);
                }
                Log($"Found: {wheelDots.Count} wheels.");
            }

            public override void Stop()
            {
                Move(0f, 0f, 1f);
            }

            public override void Tick(CockpitInputs inputs)
            {
                forwardInput = MathHelper.Lerp(forwardInput, inputs.move.Z, Smoothing);
                turnInput = MathHelper.Lerp(turnInput, inputs.move.X, Smoothing);
                Move(forwardInput * MaxSpeed, turnInput, 0);
            }

            public override void ReadCfg(MyIni ini)
            {
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                BreakingTorque = ini.Get(section, "breaking torque").ToSingle(BreakingTorque);
                wheels.ReadCfg(ini, section);
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "breaking torque", BreakingTorque);
                wheels.WriteCfg(ini, section);
            }

            public void Move(float speed, float turning, float brakes)
            {
                // Other components can make the machine move (auto pilot/crusie control). so we
                // need to get the percentage of our max speed for skid steering.
                float fwdInput = speed / MaxSpeed;

                foreach (var wheel in wheels.Blocks)
                {
                    float dot = wheelDots[wheel.EntityId];
                    wheel.TargetVelocityRPM = (turning * -dot + fwdInput) * dot * MaxSpeed;
                    wheel.BrakingTorque = brakes * BreakingTorque;
                }
                return;
            }
        }
    }
}
