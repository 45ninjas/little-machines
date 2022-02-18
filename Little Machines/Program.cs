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
        List<Component> components;

        List<IMyShipController> controllers;
        IMyShipController activeController;

        CockpitInputs input;

        ILogger logger;

        const string VERSION = "0.1.0-alpha";

        public enum MachineState
        {
            Uninitialized,
            Standby,
            Running,
            Error
        }

        public MachineState State { get; protected set;}

        public Program()
        {
            var surface = Me.GetSurface(0);
            surface.WriteText($"Little Machines\n{VERSION}\n");
            logger = new TextConsole(surface);
            controllers = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controllers);

            if (controllers.Count == 0)
                throw new Exception("No Cockpit or Remote found.");

            components = new List<Component>()
            {
                new SkidSteer(),
            };

            MyIni ini = new MyIni();
            if (string.IsNullOrWhiteSpace(Me.CustomData))
            {
                logger.PrintLn("Custom Data is empty.");
                foreach (var component in components)
                {
                    component.WriteCfg(ini);
                    logger.PrintLn($"{component.GetType().Name}: Writing default Cfg");
                }
                logger.PrintLn("Wrote Cfg to custom data");
                Me.CustomData = ini.ToString();
            }

            if(!ini.TryParse(Me.CustomData))
                throw new Exception("Failed to parse CustomData");

            foreach ( var component in components)
            {
                logger.PrintLn($"Loading: {component.GetType().Name}");
                component.Terminal = GridTerminalSystem;
                component.logger = logger;
                component.ReadCfg(ini);
            }
            logger.PrintLn($"Loaded {components.Count} compoents.");
            SetState(MachineState.Standby);
        }

        void SetState(MachineState state)
        {
            if (state == State)
                return;

            switch (state)
            {
                case MachineState.Uninitialized:
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    break;
                case MachineState.Error:
                    Runtime.UpdateFrequency = UpdateFrequency.None;
                    logger.PrintLn("Stopped by Error!");
                    break;
                case MachineState.Standby:
                    Runtime.UpdateFrequency = UpdateFrequency.Update100;
                    break;
                case MachineState.Running:
                    Runtime.UpdateFrequency = UpdateFrequency.Update1;
                    break;
            }
            State = state;
            logger.PrintLn($"Changed state to {state}");
        }

        void Start()
        {
            foreach (var component in components)
            {
                try
                {
                    component.Start();
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.GetType().Name}: Failed to Start!");
                    logger.PrintLn(e.ToString());
                    SetState(MachineState.Error);
                }
            }
        }
        void Stop()
        {
            foreach (var component in components)
            {
                try
                {
                    component.Stop();
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.GetType().Name}: Failed to Stop!");
                    logger.PrintLn(e.ToString());
                    SetState(MachineState.Error);
                }
            }
        }
        void Tick()
        {
            input = new CockpitInputs(activeController);
            foreach (var component in components)
            {
                try
                {
                    component.Tick(input);
                }
                catch (Exception e)
                {
                    logger.PrintLn($"{component.GetType().Name}: Failed to Stop!");
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
            if(updateSource.HasFlag(UpdateType.Update100) && State == MachineState.Standby)
            {
                foreach (var controller in controllers)
                {
                    if(controller.IsUnderControl)
                    {
                        SetState(MachineState.Running);
                        activeController = controller;
                        Start();
                        break;
                    }
                }
            }
            if(updateSource.HasFlag(UpdateType.Update1) && State == MachineState.Running)
            {
                if(!activeController.IsUnderControl)
                {
                    SetState(MachineState.Standby);
                    activeController = null;
                    Stop();
                }
                else
                    Tick();
            }
        }

        public interface ISerializeCfg
        {
            void WriteCfg(MyIni ini);
            void ReadCfg(MyIni ini);
        }

        public struct CockpitInputs
        {
            public Vector3 move;
            public Vector3 rotation;
            public CockpitInputs(IMyShipController controller)
            {
                move = controller.MoveIndicator;
                rotation = new Vector3(controller.RotationIndicator, controller.RollIndicator);
            }
        }
    }
}
