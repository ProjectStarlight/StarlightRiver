using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	public class ZoomHandler : ModSystem
    {
        private static int zoomTimer;
        public static float zoomOverride = 1;
        private static float oldZoom = 1;
        private static int maxTimer = 0;

        private static int flatZoomTimer = 0;
        private static float oldFlatZoom = 0;
        private static float oldFlatZoomTarget = 0;
        private static float flatZoomTarget = 0;
        private static float flatZoom = 0;

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

        public static float ClampedExtraZoomTarget => System.Math.Min(1, ExtraZoomTarget);

        public override void PostUpdateEverything()
        {
            ZoomHandler.TickZoom();
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            Transform.Zoom = ZoomHandler.ScaleVector;
            ZoomHandler.UpdateZoom();
        }

        public static void AddFlatZoom(float value)
		{
            flatZoomTarget += value;
        }

        public static void SetZoomAnimation(float zoomValue, int maxTimer = 60)
        {
            ExtraZoomTarget = zoomValue;
            ZoomHandler.maxTimer = maxTimer;
        }

        public static Vector2 ScaleVector => new Vector2(zoomOverride + flatZoom, zoomOverride + flatZoom);

        public static void TickZoom()
		{
            if (zoomTimer < maxTimer) zoomTimer++;

            if (flatZoomTimer < 30) flatZoomTimer++;

            if (flatZoomTarget != oldFlatZoomTarget)
            {
                flatZoomTimer = 0;
                oldFlatZoom = flatZoom;
            }

            flatZoom = Vector2.SmoothStep(new Vector2(oldFlatZoom, 0), new Vector2(flatZoomTarget, 0), flatZoomTimer / 30f).X;

            oldFlatZoomTarget = flatZoomTarget;
            flatZoomTarget = 0;
        }

        public static void UpdateZoom()
        {
            zoomOverride = Vector2.SmoothStep(new Vector2(oldZoom, 0), new Vector2(extraZoomTarget, 0), zoomTimer / (float)maxTimer).X;

            if (zoomOverride == Main.GameZoomTarget)
                oldZoom = Main.GameZoomTarget;

            if (zoomTimer == maxTimer && extraZoomTarget == Main.GameZoomTarget)
                maxTimer = 0;

            if (maxTimer == 0)
                zoomOverride = Main.GameZoomTarget;
        }
    }
}
