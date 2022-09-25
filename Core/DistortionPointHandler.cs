using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Core
{
    public delegate float DistortionIntensityFunction(float currentIntensity, int ticksPassed);

    public delegate float DistortionProgressFunction(float currentProgress, int ticksPassed);

    public delegate bool DistortionActiveFunction(float currentProgress, float currentIntensity, int ticksPassed);

    public class DistortionPoint
    {
        public Vector2 Position;

        public bool Active = true;

        public float Intensity;
        public float Progress;

        public int TicksPassed = 0;

        public DistortionIntensityFunction UpdateIntensity;
        public DistortionProgressFunction UpdateProgress;
        public DistortionActiveFunction UpdateActive;

        public DistortionPoint(Vector2 position, float intensity, float progress, DistortionIntensityFunction updateIntensity, DistortionProgressFunction updateProgress, DistortionActiveFunction updateActive)
        {
            Position = position;
            Intensity = intensity;
            Progress = progress;
            UpdateIntensity = updateIntensity;
            UpdateProgress = updateProgress;
            UpdateActive = updateActive;
        }
    }
    public class DistortionPointHandler : ModSystem
    {
        public static List<DistortionPoint> DistortionPoints = new List<DistortionPoint>();

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Ref<Effect> screenRef = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/DistortionPulse").Value);
                Filters.Scene["DistortionPulse"] = new Filter(new ScreenShaderData(screenRef, "MainPS"), EffectPriority.VeryHigh);
                Filters.Scene["DistortionPulse"].Load();
            }
        }

        public override void PostUpdateProjectiles()
        {
            if (Main.gameMenu)
                return;

            int projectilesFound = 0;
            int numberOfShockwaves = 0;
            float[] progresses = new float[10];
            float[] intensity = new float[10];
            Vector2[] positions = new Vector2[10];

            foreach (DistortionPoint point in DistortionPoints.ToArray())
            {
                if (!point.Active)
                    DistortionPoints.Remove(point);
            }
            foreach (DistortionPoint point in DistortionPoints)
            {
                point.Intensity = point.UpdateIntensity.Invoke(point.Intensity, ++point.TicksPassed);
                point.Progress = point.UpdateProgress.Invoke(point.Progress, point.TicksPassed);
                point.Active = point.UpdateActive.Invoke(point.Progress, point.Intensity, point.TicksPassed);

                intensity[projectilesFound] = point.Intensity;
                positions[projectilesFound] = point.Position;
                progresses[projectilesFound] = point.Progress;
                numberOfShockwaves++;
                projectilesFound++;

                if (projectilesFound > 9)
                    break;
            }

            if (projectilesFound == 0)
            {
                if (Filters.Scene["DistortionPulse"].IsActive())
                    Filters.Scene["DistortionPulse"].Deactivate();

                return;
            }

            while (projectilesFound < 9)
            {
                projectilesFound++;
                progresses[projectilesFound] = 0;
                positions[projectilesFound] = Vector2.Zero;
                intensity[projectilesFound] = 0;
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
            Vector2 offScreen = new Vector2(Main.offScreenRange);
            if (Main.drawToScreen)
            {
                offScreen = Vector2.Zero;
            }

            if (DistortionPoints.Count < 10)
                DistortionPoints.Add(new DistortionPoint(position - offScreen, intensity, progress, updateIntensity, updateProgress, updateActive));
        }
    }
}
