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
            public string PropertyId = "Velocity";
            public SmoothedAxis Input;

            public override void Start(IMyShipController cockpit)
            {
                blocks.GetBlocks(lm.GridTerminalSystem, Query);
                foreach (var blk in blocks)
                {
                    if (blk.GetProperty(PropertyId) == null)
                        Log($"'{PropertyId}' property absent on '{blk.CustomName}' block.");
                }

                if (blocks.Count == 0)
                    Log($"Found {blocks.Count} blocks.");
            }

            public override void Stop()
            {
                blocks.Set(PropertyId, 0f);
            }

            public override void Tick()
            {
                float target = Input.Get() * Speed;
                blocks.Set(PropertyId, target);
            }

            public override void ReadCfg(MyIni ini)
            {
                Speed = ini.Get(section, "speed").ToSingle(Speed);
                Query = ini.Get(section, "query").ToString(Query);
                PropertyId = ini.Get(section, "property id").ToString(PropertyId);
                Input = new SmoothedAxis(lm, ini, section);
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.Set(section, "speed", Speed);
                ini.Set(section, "query", Query);
                ini.Set(section, "property id", PropertyId);
                Input.WriteDefault(ini, section);
            }
        }
    }
}
