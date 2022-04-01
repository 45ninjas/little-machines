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
            public SmoothedAxis Input;

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

                if(motorDots.Count == 0)
                    Log($"Found {motorDots.Count} rotors.");
            }

            public override void Stop()
            {
                blocks.Set("Velocity", 0f);
            }

            public override void Tick()
            {
                float speed = Input.Get() * Speed;
                foreach (var motor in blocks)
                    motor.TargetVelocityRPM = speed * motorDots[motor.EntityId];
            }

            public override void ReadCfg(MyIni ini)
            {
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                Query = ini.Get(section, "query").ToString(Query);
                Input = new SmoothedAxis(lm, ini, section);
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "speed", Speed);
                ini.Set(section, "query", Query);
                Input.WriteDefault(ini, section);
            }
        }
    }
}
