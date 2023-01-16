using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.CameraModifiers;
using Terraria.ModLoader;

namespace StarlightRiver.Core.Systems
{
	internal class PanModifier : ICameraModifier
	{
        public Func<Vector2, Vector2, float, Vector2> EaseInFunction = Vector2.SmoothStep;
        public Func<Vector2, Vector2, float, Vector2> EaseOutFunction = Vector2.SmoothStep;
        public Func<Vector2, Vector2, float, Vector2> PanFunction = Vector2.Lerp;

        public int TotalDuration = 0;
        public int Timer = 0;
        public Vector2 PrimaryTarget = new Vector2(0, 0);
        public Vector2 SecondaryTarget = new Vector2(0, 0);

        public string UniqueIdentity => "Starlight River Pan";

		public bool Finished => false;

        public void PassiveUpdate()
		{
            if (TotalDuration > 0 && PrimaryTarget != Vector2.Zero)
            {
                //cutscene timers
                if (Timer >= TotalDuration)
                {
                    TotalDuration = 0;
                    Timer = 0;
                    PrimaryTarget = Vector2.Zero;
                    SecondaryTarget = Vector2.Zero;
                }

                if (Timer < TotalDuration)
                    Timer++;
            }
        }

		public void Update(ref CameraInfo cameraPosition)
		{
            var maxTime = TotalDuration;
            var target = PrimaryTarget;
            var timer = Timer;
            var panTarget = SecondaryTarget;

            if (maxTime > 0 && target != Vector2.Zero)
            {
                Vector2 offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

                if (timer <= 30) //go out
                    cameraPosition.CameraPosition = EaseInFunction(cameraPosition.OriginalCameraCenter + offset, target + offset, timer / 30f);
                else if (timer >= maxTime - 30) //go in
                    cameraPosition.CameraPosition = EaseOutFunction((panTarget == Vector2.Zero ? target : panTarget) + offset, cameraPosition.OriginalCameraCenter + offset, (timer - (maxTime - 30)) / 30f);
                else
                {
                    if (panTarget == Vector2.Zero)
                        cameraPosition.CameraPosition = offset + target; //stay on target
                    else if (timer <= maxTime - 150)
                        cameraPosition.CameraPosition = offset + PanFunction(target, panTarget, timer / (float)(maxTime - 150));
                    else
                        cameraPosition.CameraPosition = offset + panTarget;
                }
            }
        }

        public void Reset()
		{
            TotalDuration = 0;
            PrimaryTarget = Vector2.Zero;
            SecondaryTarget = Vector2.Zero;
        }
	}
}
