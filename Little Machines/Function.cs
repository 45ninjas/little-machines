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
        public class Function<T> : ISerializeCfg where T : class, IMyTerminalBlock
        {
            readonly string name;
            readonly string section;
            public string query;
            public float speed;

            public List<T> Blocks;

            public Function(string name)
            {
                this.name = name;
                section = $"function.{name}";
            }

            public Function(string name, string section)
            {
                this.name = name;
                this.section = section;
            }

            public void ReadCfg(MyIni ini)
            {
                speed = ini.Get(section, "speed").ToSingle();
                query = ini.Get(section, "query").ToString();
            }

            public void WriteCfg(MyIni ini)
            {
                ini.Set(section, "speed", speed);
                ini.Set(section, "query", query);
                ini.SetSectionComment(section, $"Using: {typeof(T).Name}");
            }

            public void Initialize(IMyGridTerminalSystem terminal)
            {
                Blocks = new List<T>();
                var group = terminal.GetBlockGroupWithName(query);
                if (group != null)
                    group.GetBlocksOfType<T>(Blocks);
                else
                {
                    var blk = terminal.GetBlockWithName(query) as T;
                    if (blk != null)
                        Blocks.Add(blk);
                }
            }

            public void Set<K>(string propertyId, K value)
            {
                foreach (T blk in Blocks)
                    blk.SetValue(propertyId, value);
            }
        }
    }
}