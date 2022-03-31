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
        public class Generic : Component
        {
            public Blocks<IMyTerminalBlock> blocks = new Blocks<IMyTerminalBlock>();
            public string Query = "Blocks";
            public float Speed = 1f;
            public float Smoothing = 0.01f;
            public string PropertyId = "Velocity";
            public Axis InputAxis = Axis.Roll;

            float actualSpeed;

            public override void Start(IMyShipController cockpit)
            {
                blocks.GetBlocks(lm.GridTerminalSystem, Query);
                foreach (var blk in blocks)
                {
                    if (blk.GetProperty(PropertyId) == null)
                        Log($"'{PropertyId}' property absent on '{blk.CustomName}' block.");
                }

                Log($"Found {blocks.Count} blocks.");
            }

            public override void Stop()
            {
                blocks.Set(PropertyId, 0f);
            }

            public override void Tick()
            {
                actualSpeed = MathHelper.Lerp(actualSpeed, lm.GetAxis(InputAxis) * Speed, Smoothing);
                actualSpeed = MathHelper.Clamp(actualSpeed, -Speed, Speed);
                blocks.Set(PropertyId, actualSpeed);
            }

            public override void ReadCfg(MyIni ini)
            {
                Smoothing = ini.Get(section, "smoothing").ToSingle(Smoothing);
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                Query = ini.Get(section, "query").ToString(Query);
                PropertyId = ini.Get(section, "property id").ToString(PropertyId);
                InputAxis = lm.AxisFromString(ini.Get(section, "input axis").ToString(InputAxis.ToString()));
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "smoothing", Smoothing);
                ini.Set(section, "speed", Speed);
                ini.Set(section, "query", Query);
                ini.Set(section, "property id", PropertyId);
                ini.Set(section, "input axis", InputAxis.ToString());
            }
        }
    }
}
