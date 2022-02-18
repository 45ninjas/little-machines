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
        public interface ILogger
        {
            void PrintLn(string msg);
        }
        public class TextConsole : ILogger
        {
            private IMyTextSurface surface;

            Queue<string> lines;
            int maxLines;

            StringBuilder sb;
            public TextConsole(IMyTextSurface surface)
            {
                SetSurface(surface);
            }
            public void PrintLn(string msg)
            {
                lines.Enqueue(msg);
                while (lines.Count > maxLines)
                    lines.Dequeue();

                sb.Clear();
                foreach (var line in lines)
                    sb.AppendLine(line);

                surface.WriteText(sb, false);
            }

            // Configure our surface.
            public void SetSurface(IMyTextSurface surface)
            {
                sb = new StringBuilder();
                surface.Font = "Monospace";
                surface.TextPadding = 0;
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                this.surface = surface;
                float aspect = surface.SurfaceSize.Y / surface.SurfaceSize.X;
                maxLines = (int)((18 / surface.FontSize) * aspect);
                lines = new Queue<string>(surface.GetText().Split('\n'));
            }
        }
    }
}
