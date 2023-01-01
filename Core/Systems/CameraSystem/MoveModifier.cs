using System;
using Terraria.Graphics.CameraModifiers;

namespace StarlightRiver.Core.Systems.CameraSystem
{
	internal class MoveModifier : ICameraModifier
	{
		public Func<Vector2, Vector2, float, Vector2> EaseFunction = Vector2.SmoothStep;

		public int MovementDuration = 0;
		public int Timer = 0;
		public Vector2 Target = new(0, 0);
		public bool Returning = false;

		public string UniqueIdentity => "Starlight River Move";

		public bool Finished => false;

		public void PassiveUpdate()
		{
			if (MovementDuration > 0 && Target != Vector2.Zero)
			{
				if (Timer < MovementDuration)
					Timer++;
			}
		}

		public void Update(ref CameraInfo cameraPosition)
		{
			if (MovementDuration > 0 && Target != Vector2.Zero)
			{
				var offset = new Vector2(-Main.screenWidth / 2f, -Main.screenHeight / 2f);

				if (Returning)
					cameraPosition.CameraPosition = EaseFunction(Target + offset, cameraPosition.OriginalCameraCenter + offset, Timer / (float)MovementDuration);
				else
					cameraPosition.CameraPosition = EaseFunction(cameraPosition.OriginalCameraCenter + offset, Target + offset, Timer / (float)MovementDuration);
			}
		}

		public void Reset()
		{
			MovementDuration = 0;
			Target = Vector2.Zero;
		}
	}
}
