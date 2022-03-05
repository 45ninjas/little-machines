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
        public class CruiseControl : Component
        {
            Dictionary<string, float> targets;
            public float? Target = 0;
            public ILocomotion locomotion;
            public override void Stop()
            {
                if (locomotion != null)
                    locomotion.Move(0f, 0f, 1f);
            }
            public override void Start(IMyShipController cockpit)
            {
                if (locomotion == null)
                {
                    locomotion = lm.components.Values.OfType<ILocomotion>().First();
                    if (locomotion != null)
                        Log($"Using '{((Component)locomotion).section}' for locomotion.");
                    else
                    {
                        Log($"No locomotion found. Disabling");
                        Enabled = false;
                    }
                }
            }

            public override void ReadCfg(MyIni ini)
            {
                Component loco;
                string locoName = ini.Get(section, "locomotion component").ToString();
                if(lm.components.TryGetValue(locoName, out loco))
                {
                    locomotion = loco as ILocomotion;
                    if(locomotion == null)
                        Log($"'{locoName}' does not implement 'ILocomotion'");
                }

                targets = new Dictionary<string, float>();

                List<MyIniKey> keys = new List<MyIniKey>();
                ini.GetKeys(section, keys);

                foreach (var key in keys)
                {
                    float target;
                    if (ini.Get(key).TryGetSingle(out target))
                    {
                        targets.Add(key.Name, target);
                        Log($"Found '{key.Name}' target.");
                    }
                }
                Log($"Found {targets.Count} speed targets.");
            }
            public override void WriteCfg(MyIni ini)
            {
                ini.AddSection(section);
            }

            public override void Execute(List<string> args)
            {
                if(args.Count > 0)
                {
                    float target;
                    if (args[0].ToLower() == "stop")
                    {
                        Enabled = false;
                    }
                    else if (targets.TryGetValue(args[0], out target))
                    {
                        Enabled = true;
                        Log($"Target Speed {Target}");
                    }
                }
            }
        }
    }
}
