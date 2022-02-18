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
        public class Component : ISerializeCfg
        {
            public IMyGridTerminalSystem Terminal;
            public ILogger logger;

            public virtual void Tick(CockpitInputs inputs)
            {

            }
            public virtual void Stop()
            {

            }
            public virtual void Start()
            {

            }

            public virtual void WriteCfg(MyIni ini)
            {
                
            }

            public virtual void ReadCfg(MyIni ini)
            {
                
            }

            protected void Log(string msg) => logger.PrintLn($"{GetType().Name}:{msg}");
        }
    }
}
