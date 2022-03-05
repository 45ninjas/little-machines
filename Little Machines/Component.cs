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
        //List<Component> components;
        Dictionary<string, Component> components;
        public class Component : ISerializeCfg
        {
            public IMyGridTerminalSystem Terminal;
            public string section;
            public Program lm;

            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    if(value != _enabled)
                    {
                        _enabled = value;
                        if (Enabled == false)
                            Stop();
                        else if (lm != null)
                            Start(lm.activeController);
                    }
                }
            }
            private bool _enabled = false;

            public virtual void Tick(CockpitInputs inputs) { }
            public virtual void Stop() { }
            public virtual void Start(IMyShipController cockpit) { }
            public virtual void WriteCfg(MyIni ini) { }
            public virtual void ReadCfg(MyIni ini) { }
            protected void Log(string msg) => lm.logger.PrintLn($"{section}:{msg}");

            public virtual void Execute(List<string> args) {
                Log("Has 0 commands.");
            }
        }

        public IEnumerable<Component> EnabledComponents() => components.Values.Where(x => x.Enabled);
        public void ComponentsFromCfg(MyIni ini)
        {
            components = new Dictionary<string, Component>();

            // Get all the keys in lm.components and iterate over them.
            var keys = new List<MyIniKey>();
            ini.GetKeys("lm.components", keys);

            foreach (var key in keys)
            {
                try
                {
                    string section = "lm." + key.Name;
                    var kind = ini.Get(key).ToString();
                    var component = ComponentFromCfg(ini, section, kind);
                    components.Add(key.Name, component);
                    logger.PrintLn($"Loaded {kind} named {section}");
                }
                catch (Exception e)
                {
                    logger.PrintLn($"Failed to load '{key.Name}'");
                    logger.PrintLn(e.ToString());
                }
            }
            // Now that ALL components have been loaded, delete our avalibe components.
            avalibleComponents.Clear();
        }
        public Component ComponentFromCfg(MyIni ini, string section, string kind)
        {
            if (!avalibleComponents.ContainsKey(kind))
                throw new Exception($"Component type {kind} does not exist.");

            if (avalibleComponents[kind].Count == 0)
                throw new Exception($"Ran out of '{kind}' components.");

            var component = avalibleComponents[kind].Pop();
            component.section = section;
            component.Terminal = GridTerminalSystem;
            component.lm = this;

            if (ini.ContainsSection(section))
                component.ReadCfg(ini);
            else
                component.WriteCfg(ini);

            return component;
        }

        public interface ILocomotion
        {
            void Move(float forward, float turning, float brakes);
            float MaxSpeed {
                get;
            }
        }
    }
}
