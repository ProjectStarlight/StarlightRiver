using System;
using Terraria.Graphics.CameraModifiers;

namespace StarlightRiver.Core.Systems.CameraSystem
{
	internal class AsymetricalPanModifier : ICameraModifier
	{
		public Func<Vector2, Vector2, float, Vector2> EaseInFunction = Vector2.SmoothStep;
		public Func<Vector2, Vector2, float, Vector2> EaseOutFunction = Vector2.SmoothStep;

		public int timeOut = 0;
		public int timeHold = 0;
		public int timeIn = 0;

		public int timer = 0;
		public Vector2 target = new(0, 0);

		public int TotalDuration => timeOut + timeHold + timeIn;

		public string UniqueIdentity => "Starlight River AsymPan";

		public bool Finished => false;

		public void PassiveUpdate()
		{
			if (TotalDuration > 0 && target != Vector2.Zero)
			{
				//cutscene timers
				if (timer >= TotalDuration)
				{
					timeOut = 0;
					timeHold = 0;
					timeIn = 0;

					timer = 0;

					target = Vector2.Zero;
				}

				if (timer < TotalDuration)
					timer++;
			}
		}

		public void Update(ref CameraInfo cameraPosition)
		{
			if (TotalDuration > 0 && target != Vector2.Zero)
			{
				var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

				if (timer <= timeOut) //go out
					cameraPosition.CameraPosition = EaseOutFunction(cameraPosition.OriginalCameraCenter + offset, target + offset, timer / (float)timeOut);
				else if (timer >= TotalDuration - timeIn) //go in
					cameraPosition.CameraPosition = EaseInFunction(target + offset, cameraPosition.OriginalCameraCenter + offset, (timer - (TotalDuration - timeIn)) / (float)timeIn);
				else
					cameraPosition.CameraPosition = offset + target; //stay on target
			}
		}

		public void Reset()
		{
			timeOut = 0;
			timeHold = 0;
			timeIn = 0;

			timer = 0;

			target = Vector2.Zero;
		}
	}
}
