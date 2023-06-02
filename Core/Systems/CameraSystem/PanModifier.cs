using System;
using Terraria.Graphics.CameraModifiers;

namespace StarlightRiver.Core.Systems.CameraSystem
{
	internal class PanModifier : ICameraModifier
	{
		public Func<Vector2, Vector2, float, Vector2> EaseInFunction = Vector2.SmoothStep;
		public Func<Vector2, Vector2, float, Vector2> EaseOutFunction = Vector2.SmoothStep;
		public Func<Vector2, Vector2, float, Vector2> PanFunction = Vector2.Lerp;

		public int TotalDuration = 0;
		public int Timer = 0;
		public Vector2 PrimaryTarget = new(0, 0);
		public Vector2 SecondaryTarget = new(0, 0);

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
			int maxTime = TotalDuration;
			Vector2 target = PrimaryTarget;
			int timer = Timer;
			Vector2 panTarget = SecondaryTarget;

			if (maxTime > 0 && target != Vector2.Zero)
			{
				var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

				if (timer <= 30) //go out
				{
					cameraPosition.CameraPosition = EaseInFunction(cameraPosition.OriginalCameraCenter + offset, target + offset, timer / 30f);
				}
				else if (timer >= maxTime - 30) //go in
				{
					cameraPosition.CameraPosition = EaseOutFunction((panTarget == Vector2.Zero ? target : panTarget) + offset, cameraPosition.OriginalCameraCenter + offset, (timer - (maxTime - 30)) / 30f);
				}
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