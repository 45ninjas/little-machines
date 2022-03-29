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
        public enum Axis
        {
            None,
            Horizontal,
            Vertical,
            Forward,
            Pitch,
            Yaw,
            Roll
        }

        public Axis AxisFromString(string val)
        {
            Axis axis;
            if (!Enum.TryParse(val, true, out axis)) {
                throw new Exception($"'{val}' is not a valid axis");
            }
            return axis;
        }

        public float GetAxis(Axis axis)
        {
            if (activeController == null || !activeController.CanControlShip)
                return 0f;
            switch (axis)
            {
                case Axis.Horizontal:
                    return activeController.MoveIndicator.X;
                case Axis.Vertical:
                    return activeController.MoveIndicator.Y;
                case Axis.Forward:
                    return activeController.MoveIndicator.Z;
                case Axis.Pitch:
                    return activeController.RotationIndicator.X;
                case Axis.Yaw:
                    return activeController.RotationIndicator.Y;
                case Axis.Roll:
                    return activeController.RollIndicator;
                default:
                    return 0f;
            }
        }
    }
}
