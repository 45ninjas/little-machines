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
        public class Piston : Component
        {
            public Function<IMyPistonBase> pistons = new Function<IMyPistonBase>("piston")
            {
                query = "Lift Pistons",
                speed = 0.25f,
            };

            public float Smoothing = 0.01f;

            public float MaxSpeed = 0.5f;

            float speed;

            public override void Start(IMyShipController cockpit)
            {
                pistons.Initialize(Terminal);
                Log($"Has {pistons.Blocks.Count} Pistons");
            }

            public override void Stop()
            {
                pistons.Set<float>("Velocity", 0);
            }

            public override void Tick(CockpitInputs inputs)
            {
                speed = MathHelper.Lerp(speed, inputs.rotation.X * pistons.speed, Smoothing);
                speed = MathHelper.Clamp(speed, -MaxSpeed, MaxSpeed);
                pistons.Set("Velocity", speed);
            }

            public override void ReadCfg(MyIni ini)
            {
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                MaxSpeed = ini.Get(section, "max-speed").ToSingle(MaxSpeed);
                pistons.ReadCfg(ini, section);
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "max-speed", MaxSpeed);
                pistons.WriteCfg(ini, section);
            }
        }
    }
}
