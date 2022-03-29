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
        public class Blocks<T>: IEnumerable<T> where T : class, IMyTerminalBlock
        {
            readonly List<T> blocks = new List<T>();

            public void GetBlocks(IMyGridTerminalSystem terminal, string query)
            {
                blocks.Clear();
                var group = terminal.GetBlockGroupWithName(query);
                if (group != null)
                    group.GetBlocksOfType<T>(blocks);
                else
                {
                    var blk = terminal.GetBlockWithName(query) as T;
                    if (blk != null)
                        blocks.Add(blk);
                }
            }

            public void Set<K>(string propertyId, K value)
            {
                foreach (T blk in blocks)
                {
                    blk.SetValue(propertyId, value);
                }
            }

            public IEnumerator<T> GetEnumerator() => blocks.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => blocks.GetEnumerator();

            public int Count => blocks.Count;
        }
    }
}