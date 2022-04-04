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
    partial class Program : MyGridProgram
    {
        Dictionary<string, Stack<Component>> avalibleComponents;

        List<IMyShipController> controllers;
        IMyShipController activeController;
        MyIniKey logSurfaceKey = new MyIniKey("lm.core", "log-surface");
        MyIniKey hotloadKey = new MyIniKey("lm.core", "hotload");
        bool hotload = false;
        TextConsole logger;

        int lastHash;

        const string VERSION = "0.4.2 A";
        const string HEADER = "Little Machines\nVersion " + VERSION;

        public enum MachineState
        {
            Uninitialized,
            Standby,
            Running,
            Error
        }

        public MachineState State { get; protected set; }

        public Program()
        {
            init();
        }

        void init()
        {
            avalibleComponents = new Dictionary<string, Stack<Component>>()
            {
                {"skidsteer", new Stack<Component>(new[] { new SkidSteer() }) },
                {"generic", new Stack<Component>(new[] { new Generic(), new Generic(), new Generic(), new Generic(),new Generic(),new Generic(),new Generic() }) },
                {"pivot", new Stack<Component>(new[] { new Pivot(), new Pivot() }) },
            };
            // Parse the Custom Data.
            var ini = new MyIni();
            if (!ini.TryParse(Me.CustomData))
                throw new Exception("Unable to Parse config.");

            // Get a surface from the cfg or use the one on the programmable block.
            var surface = SurfaceFromCfg(ini, logSurfaceKey);
            if (surface == null)
                surface = Me.GetSurface(0);

            // Clear the console with a little header message. Then create our logger.
            surface.WriteText(HEADER);
            logger = new TextConsole(surface);

            logger.PrintLn($"Avalible Components: {string.Join(", ", avalibleComponents.Keys)}");

            // Get all controllers on this ship..
            controllers = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controllers);

            if (controllers.Count == 0)
                throw new Exception("No Cockpit or Remote found.");

            // Do this mess...
            ComponentsFromCfg(ini);
            Me.CustomData = ini.ToString();
            logger.PrintLn($"Loaded {components.Count} components.");
            SetState(MachineState.Standby);
            logger.PrintLn("Finihsed Startup");

            hotload = ini.Get(hotloadKey).ToBoolean(false);
            if (hotload)
            {
                lastHash = Me.CustomData.GetHashCode();
                logger.PrintLn("WARN: Hotload ON!");
            }
        }

        void SetState(MachineState state)
        {
            if (state == State)
                return;
            string stateText = "INI";
            switch (state)
            {
                case MachineState.Error:
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    stateText = "ERR";
                    logger.PrintLn("Stopped by Error!");
                    break;
                case MachineState.Standby:
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                    stateText = "SBY";
                    break;
                case MachineState.Running:
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    stateText = "RUN";
                    break;
                default:
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    break;
            }
            State = state;
            logger.PrintLn($"Changed state to {state}");
            logger.SetStatus($"LM {VERSION}          [ {components.Count} ]        [ {stateText} ]");
        }

        void Start()
        {
            foreach (var component in EnabledComponents())
            {
                try
                {
                    component.Start(activeController);
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.section}: Failed to Start!");
                    logger.PrintLn(e.ToString());
                    SetState(MachineState.Error);
                }
            }
        }
        void Stop()
        {
            foreach (var component in components.Values)
            {
                try
                {
                    component.Stop();
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.section}: Failed to Stop!");
                    logger.PrintLn(e.ToString());
                    SetState(MachineState.Error);
                }
            }
        }
        void Tick()
        {
            foreach (var component in EnabledComponents())
            {
                try
                {
                    component.Tick();
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.GetType().Name}: Failed to Tick!");
                    logger.PrintLn(e.ToString());
                    SetState(MachineState.Error);
                }
            }
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource.HasFlag(UpdateType.Update100))
            {
                if (hotload && Me.CustomData.GetHashCode() != lastHash)
                    init();

                if (State == MachineState.Standby)
                {
                    foreach (var controller in controllers)
                    {
                        if (controller.IsUnderControl)
                        {
                            SetState(MachineState.Running);
                            activeController = controller;
                            Start();
                            break;
                        }
                    }
                }
            }
            if (updateSource.HasFlag(UpdateType.Update1) && State == MachineState.Running)
            {
                if (!activeController.IsUnderControl)
                {
                    SetState(MachineState.Standby);
                    activeController = null;
                    Stop();
                }
                else
                    Tick();
            }

            if (!string.IsNullOrWhiteSpace(argument))
            {
                var args = new List<string>(argument.Split(' '));
                if (args.Count > 0)
                {
                    var cmd = args[0];
                    args.RemoveAt(0);

                    Component component;
                    if (!components.TryGetValue(cmd, out component))
                    {
                        logger.PrintLn($"Component {cmd} not found.");
                    }
                    component.Execute(args);
                }
            }
        }

        public interface ISerializeCfg
        {
            void WriteCfg(MyIni ini);
            void ReadCfg(MyIni ini);
        }
    }
}