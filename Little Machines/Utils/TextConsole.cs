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

            string status;

            StringBuilder sb;
            public TextConsole(IMyTextSurface surface)
            {
                SetSurface(surface);
            }
            public void PrintLn(string msg)
            {
                lines.Enqueue(msg);
                Redraw();
            }

            public void SetStatus(string status)
            {
                this.status = status;
                Redraw();
            }

            private void Redraw()
            {
                sb.Clear();

                sb.AppendLine(status);
                sb.AppendLine("--------------------------------------------------------------------------");

                while (lines.Count > maxLines)
                    lines.Dequeue();

                foreach (var line in lines)
                    sb.AppendLine(line);

                surface.WriteText(sb, false);
            }

            // Configure our surface.
            public void SetSurface(IMyTextSurface surface)
            {
                sb = new StringBuilder();
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                this.surface = surface;
                float aspect = surface.SurfaceSize.Y / surface.SurfaceSize.X;
                maxLines = (int)((18 / surface.FontSize) * aspect);
                // Subtract for our status lines.
                maxLines -= 3;
                lines = new Queue<string>(surface.GetText().Split('\n'));
            }
        }
    }
}
