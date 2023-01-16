using System;
using Terraria.Graphics.CameraModifiers;

namespace StarlightRiver.Core.Systems.CameraSystem
{
	public class CameraSystem : ModSystem
	{
		public static int shake = 0;

		private static PanModifier PanModifier = new();
		private static readonly MoveModifier MoveModifier = new();

		/// <summary>
		/// Sets up a panning animation for the screen. Great for use with things like boss spawns or defeats, or other events happening near the player.
		/// </summary>
		/// <param name="duration"> How long the animation should last </param>
		/// <param name="target"> Where the camera should pan to </param>
		/// <param name="secondaryTarget"> Where the camera will scroll to after panning to the initial target. Leave blank to keep the camera in place at the initial target </param>
		/// <param name="easeIn"> Changes the easing function for the motion from the player to the primary target. Default is Vector2.Smoothstep </param>
		/// <param name="easeOut"> Changes the easing function for the motion from the primary/secondary target back to the player. Default is Vector2.Smoothstep </param>
		/// <param name="easePan"> Changes the easing function for the motion from primary to secondary target if applicable. Default is Vector2.Lerp </param>
		public static void DoPanAnimation(int duration, Vector2 target, Vector2 secondaryTarget = default, Func<Vector2, Vector2, float, Vector2> easeIn = null, Func<Vector2, Vector2, float, Vector2> easeOut = null, Func<Vector2, Vector2, float, Vector2> easePan = null)
		{
			PanModifier.TotalDuration = duration;
			PanModifier.PrimaryTarget = target;
			PanModifier.SecondaryTarget = secondaryTarget;

			PanModifier.EaseInFunction = easeIn ?? Vector2.SmoothStep;
			PanModifier.EaseOutFunction = easeOut ?? Vector2.SmoothStep;
			PanModifier.PanFunction = easePan ?? Vector2.Lerp;
		}

		/// <summary>
		/// Moves the camera to a set point, with an animation of the specified duration, and stays there. Use ReturnCamera to retrieve it later.
		/// </summary>
		/// <param name="duration"> How long it takes the camera to get to it's destination </param>
		/// <param name="target"> Where the camera should end up </param>
		/// <param name="ease"> The easing function the camera should follow on it's journey. Default is Vector2.Smoothstep </param>
		public static void MoveCameraOut(int duration, Vector2 target, Func<Vector2, Vector2, float, Vector2> ease = null)
		{
			MoveModifier.Timer = 0;
			MoveModifier.MovementDuration = duration;
			MoveModifier.Target = target;

			MoveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
		}

		/// <summary>
		/// Returns the camera to the player after it has been sent out by MoveCameraOut.
		/// </summary>
		/// <param name="duration"> How long it takes for the camera to get back to the player </param>
		/// <param name="ease"> The easing function the camera should follow on it's journey. Default is Vector2.Smoothstep </param>
		public static void ReturnCamera(int duration, Func<Vector2, Vector2, float, Vector2> ease = null)
		{
			MoveModifier.Timer = 0;
			MoveModifier.MovementDuration = duration;

			MoveModifier.EaseFunction = ease ?? Vector2.SmoothStep;
		}

		public override void PostUpdateEverything()
		{
			if (shake > 120 * ModContent.GetInstance<Content.Configs.GraphicsConfig>().ScreenshakeMult) //clamp screenshake to (120 * config) to prevent utter chaos
				shake = (int)(120 * ModContent.GetInstance<Content.Configs.GraphicsConfig>().ScreenshakeMult);

			PanModifier.PassiveUpdate();
			MoveModifier.PassiveUpdate();
		}

		public override void ModifyScreenPosition()
		{
			float mult = ModContent.GetInstance<Content.Configs.GraphicsConfig>().ScreenshakeMult;
			mult *= Main.screenWidth / 2048f * 1.2f; //normalize for screen resolution
			Main.instance.CameraModifiers.Add(new PunchCameraModifier(Main.LocalPlayer.position, Main.rand.NextFloat(3.14f).ToRotationVector2(), shake * mult, 15f, 30, 2000, "Starlight Shake"));

			if (PanModifier.TotalDuration > 0 && PanModifier.PrimaryTarget != Vector2.Zero)
				Main.instance.CameraModifiers.Add(PanModifier);

			if (shake > 0)
				shake--;
		}

		public static void Reset()
		{
			shake = 0;

			PanModifier.Reset();
			MoveModifier.Reset();
		}

		public override void OnWorldLoad()
		{
			Reset();
		}

		public override void Unload()
		{
			PanModifier = null;
		}
	}
}
