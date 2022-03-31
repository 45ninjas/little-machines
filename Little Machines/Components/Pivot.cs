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
        public class Pivot : Component
        {
            public Blocks<IMyMotorStator> blocks = new Blocks<IMyMotorStator>();
            Dictionary<long, float> motorDots;
            public string Query = "Blocks";
            public float Speed = 1f;
            public float Smoothing = 0.1f;
            public Axis InputAxis = Axis.Roll;

            float speed;

            public override void Start(IMyShipController cockpit)
            {
                blocks.GetBlocks(lm.GridTerminalSystem, Query);

                
                var right = cockpit.WorldMatrix.Right;
                // Get the center of mass.
                var com = cockpit.CenterOfMass;

                // Fill a dict. of the dot product of each motor from the center of mass.
                motorDots = new Dictionary<long, float>(blocks.Count);
                foreach (var motor in blocks)
                {
                    var posDot = (com - motor.GetPosition()).Dot(right);
                    // TODO: Make this relative to the direction the rotor is pointing NOT if it's LEFT or RIGHT of the cockpit.
                    //var dirDot = Math.Abs(motor.WorldMatrix.Up.Dot(right));
                    motorDots.Add(motor.EntityId, (float)posDot);
                }
                Log($"Found {motorDots.Count} rotors.");
            }

            public override void Stop()
            {
                blocks.Set("Velocity", 0f);
            }

            public override void Tick()
            {
                speed = MathHelper.Lerp(speed, lm.GetAxis(InputAxis) * Speed, Smoothing);
                speed = MathHelper.Clamp(speed, -Speed, Speed);

                foreach (var motor in blocks)
                {
                    motor.TargetVelocityRPM = speed * motorDots[motor.EntityId];
                }
            }

            public override void ReadCfg(MyIni ini)
            {
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                Query = ini.Get(section, "query").ToString(Query);
                InputAxis = lm.AxisFromString(ini.Get(section, "input axis").ToString(InputAxis.ToString()));
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "speed", Speed);
                ini.Set(section, "query", Query);
                ini.Set(section, "input axis", InputAxis.ToString());
            }
        }
    }
}
