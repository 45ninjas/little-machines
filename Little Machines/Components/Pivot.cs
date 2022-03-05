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
            public Function<IMyMotorStator> rotors = new Function<IMyMotorStator>("rotor")
            {
                query = "Tilt",
                speed = 1f,
            };

            public override void Start(IMyShipController cockpit)
            {
                rotors.Initialize(Terminal);
                Log($"Has {rotors.Blocks.Count} Rotors");
            }

            public override void Stop()
            {
                rotors.Set<float>("Velocity", 0);
            }

            public override void Tick(CockpitInputs inputs)
            {
                rotors.Set("Velocity", inputs.rotation.X * rotors.speed);
            }

            public override void ReadCfg(MyIni ini)
            {
                rotors.ReadCfg(ini, section);
            }
            public override void WriteCfg(MyIni ini)
            {
                rotors.WriteCfg(ini, section);
            }
        }
    }
}
