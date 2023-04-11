using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Core
{
	public delegate float DistortionIntensityFunction(float currentIntensity, int ticksPassed);

	public delegate float DistortionProgressFunction(float currentProgress, int ticksPassed);

	public delegate bool DistortionActiveFunction(float currentProgress, float currentIntensity, int ticksPassed);

	public class DistortionPoint
	{
		public Vector2 position;

		public bool active = true;

		public float intensity;
		public float progress;

		public int tickspassed = 0;

		public DistortionIntensityFunction UpdateIntensity;
		public DistortionProgressFunction UpdateProgress;
		public DistortionActiveFunction UpdateActive;

		public DistortionPoint(Vector2 position, float intensity, float progress, DistortionIntensityFunction updateIntensity, DistortionProgressFunction updateProgress, DistortionActiveFunction updateActive)
		{
			this.position = position;
			this.intensity = intensity;
			this.progress = progress;
			UpdateIntensity = updateIntensity;
			UpdateProgress = updateProgress;
			UpdateActive = updateActive;
		}
	}

	public class DistortionPointHandler : ModSystem
	{
		public static List<DistortionPoint> DistortionPoints = new();

		public override void PostUpdateProjectiles()
		{
			if (Main.gameMenu || Main.dedServ)
				return;

			int pointsFound = 0;
			int numberOfShockwaves = 0;
			float[] progresses = new float[10];
			float[] intensity = new float[10];
			var positions = new Vector2[10];

			foreach (DistortionPoint point in DistortionPoints.ToArray())
			{
				if (!point.active)
					DistortionPoints.Remove(point);
			}

			foreach (DistortionPoint point in DistortionPoints)
			{
				point.intensity = point.UpdateIntensity.Invoke(point.intensity, ++point.tickspassed);
				point.progress = point.UpdateProgress.Invoke(point.progress, point.tickspassed);
				point.active = point.UpdateActive.Invoke(point.progress, point.intensity, point.tickspassed);

				intensity[pointsFound] = point.intensity;
				positions[pointsFound] = point.position;
				progresses[pointsFound] = point.progress;
				numberOfShockwaves++;
				pointsFound++;

				if (pointsFound > 9)
					break;
			}

			if (pointsFound == 0)
			{
				if (Filters.Scene["DistortionPulse"].IsActive())
					Filters.Scene["DistortionPulse"].Deactivate();

				return;
			}

			while (pointsFound < 9)
			{
				pointsFound++;
				progresses[pointsFound] = 0;
				positions[pointsFound] = Vector2.Zero;
				intensity[pointsFound] = 0;
			}

			Filters.Scene["DistortionPulse"].GetShader().Shader.Parameters["progresses"].SetValue(progresses);
			Filters.Scene["DistortionPulse"].GetShader().Shader.Parameters["positions"].SetValue(positions);
			Filters.Scene["DistortionPulse"].GetShader().Shader.Parameters["intensity"].SetValue(intensity);
			Filters.Scene["DistortionPulse"].GetShader().Shader.Parameters["numberOfPoints"].SetValue(numberOfShockwaves);

			if (Main.netMode != NetmodeID.Server && !Filters.Scene["DistortionPulse"].IsActive())
			{
				Filters.Scene.Activate("DistortionPulse").GetShader().UseProgress(0f).UseColor(Color.White.ToVector3()).UseOpacity(0.0001f);
			}
		}

		public static void AddPoint(Vector2 position, float intensity, float progress, DistortionIntensityFunction updateIntensity, DistortionProgressFunction updateProgress, DistortionActiveFunction updateActive)
		{
			var offScreen = new Vector2(Main.offScreenRange);
			if (Main.drawToScreen)
				offScreen = Vector2.Zero;

			if (DistortionPoints.Count < 10)
				DistortionPoints.Add(new DistortionPoint(position - offScreen, intensity, progress, updateIntensity, updateProgress, updateActive));
		}
	}
}