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
            const string section = "skidsteer";
            public Function<IMyMotorStator> leftWheels = new Function<IMyMotorStator>("left", section + ".left")
            {
                query = "Left Rotors",
                speed = 40f,
            };
            public Function<IMyMotorStator> rightWheels = new Function<IMyMotorStator>("right", section + ".right")
            {
                query = "Right Rotors",
                speed = 40f,
            };

            public override void Start()
            {
                leftWheels.Initialize(Terminal);
                rightWheels.Initialize(Terminal);

                Log($"Has {leftWheels.Blocks.Count} Left wheels");
                Log($"Has {rightWheels.Blocks.Count} Right wheels");
            }

            public override void Stop()
            {
                leftWheels.Set<float>("Velocity", 0);
                rightWheels.Set<float>("Velocity", 0);
            }

            public override void Tick(CockpitInputs inputs)
            {
                leftWheels.Set("Velocity", (inputs.move.Z - inputs.move.X) * leftWheels.speed);
                rightWheels.Set("Velocity", (inputs.move.Z + inputs.move.X) * rightWheels.speed);
            }

            public override void ReadCfg(MyIni ini)
            {
                leftWheels.ReadCfg(ini);
                rightWheels.ReadCfg(ini);
            }
            public override void WriteCfg(MyIni ini)
            {
                leftWheels.WriteCfg(ini);
                rightWheels.WriteCfg(ini);
            }
        }
    }
}
