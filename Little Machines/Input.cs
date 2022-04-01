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

        public class SmoothedAxis
        {
            public readonly Axis Axis;
            public readonly float Smoothing;
            float current;
            private readonly Program lm;

            public SmoothedAxis(Program lm, MyIni ini, string section, string prefix = null)
            {
                if (prefix != null)
                    prefix = prefix + ".";

                Smoothing = ini.Get(section, prefix + "smoothing").ToSingle(20f);
                Axis = lm.AxisFromString(ini.Get(section, prefix + "axis").ToString(Axis.None.ToString()));

                this.lm = lm; ;
            }
            public SmoothedAxis(Program lm, Axis axis, float response)
            {
                Smoothing = response;
                Axis = axis;
                this.lm = lm;
            }
            public float Get()
            {
                float target = MathHelper.Clamp(lm.GetAxis(Axis), -1f, 1f);
                // Linear smoothing
                // current += delta * Response;
                // S-Curve smoothing
                current = current + Math.Sign(target - current) / Smoothing;

                // Snap to zero if we are close to zero.
                if (Math.Abs(current) < 0.01f)
                    current = 0.0f;

                // Return our current value.
                return current;
            }
            public float GetRaw() => lm.GetAxis(Axis);
            public void WriteDefault(MyIni ini, string section, string prefix = null)
            {
                if (prefix != null)
                    prefix = prefix + ".";

                ini.Set(section, prefix + "smoothing", Smoothing);
                ini.Set(section, prefix + "axis", Axis.ToString());
            }
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
