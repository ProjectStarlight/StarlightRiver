using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace StarlightRiver.Core
{
    public static class ZoomHandler
    {
        private static int zoomTimer;
        public static float zoomOverride = 1;
        private static float oldZoom = 1;
        private static int maxTimer = 0;

        private static float extraZoomTarget = 1;

        public static float ExtraZoomTarget
        {
            get => extraZoomTarget;

            private set
            {
                oldZoom = zoomOverride;
                zoomTimer = 0;
                extraZoomTarget = value;
            }
        }

        public static void SetZoomAnimation(float zoomValue, int maxTimer = 60)
        {
            ExtraZoomTarget = zoomValue;
            ZoomHandler.maxTimer = maxTimer;
        }

        public static Vector2 ScaleVector => new Vector2(zoomOverride, zoomOverride);

        public static void UpdateZoom()
        {
            if (zoomTimer < maxTimer) zoomTimer++;
            zoomOverride = Vector2.Lerp(new Vector2(oldZoom), new Vector2(extraZoomTarget), zoomTimer / (float)maxTimer).X;

            if (zoomOverride == Main.GameZoomTarget)
                oldZoom = Main.GameZoomTarget;

            if (zoomTimer == maxTimer && extraZoomTarget == Main.GameZoomTarget)
                maxTimer = 0;

            if (maxTimer == 0)
                zoomOverride = Main.GameZoomTarget;
        }
    }
}
