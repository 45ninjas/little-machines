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
        public class Function<T> where T : class, IMyTerminalBlock
        {
            readonly string name;
            public string query;
            public float speed;

            public readonly List<T> Blocks = new List<T>();

            public Function(string name)
            {
                this.name = name;
            }

            public void ReadCfg(MyIni ini, string section)
            {
                speed = ini.Get(section, $"{name}.speed").ToSingle();
                query = ini.Get(section, $"{name}.query").ToString();
            }

            public void WriteCfg(MyIni ini, string section)
            {
                ini.Set(section, $"{name}.speed", speed);
                ini.Set(section, $"{name}.query", query);
                //ini.SetSectionComment(section, $"Using: {typeof(T).Name}");
            }

            public void Initialize(IMyGridTerminalSystem terminal)
            {
                Blocks.Clear();
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