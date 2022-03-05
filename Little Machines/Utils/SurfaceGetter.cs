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
        public IMyTextSurface SurfaceFromCfg(MyIni ini, MyIniKey key)
        {
            if(!ini.ContainsKey(key))
                return null;

            var input = ini.Get(key).ToString();

            if (string.IsNullOrWhiteSpace(input))
                return null;

            var peices = input.Split(':');
            if (peices.Length < 1 || string.IsNullOrWhiteSpace(peices[0]))
                return null;

            var provider = GridTerminalSystem.GetBlockWithName(peices[0]) as IMyTextSurfaceProvider;

            if (provider == null)
                return null;

            int index;
            if(peices.Length < 2 || !int.TryParse(peices[1], out index))
                return null;

            if(index >= 0 && index < provider.SurfaceCount)
                return provider.GetSurface(index);

            return null;
        }
    }
}
