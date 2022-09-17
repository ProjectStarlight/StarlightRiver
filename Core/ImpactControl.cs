//TODO:
//Split up edge detection into its own shader
//Eliminate curving
//Fix wave
//Make lines more solid somehow

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

using ReLogic.Content;

namespace StarlightRiver.Core
{
    public class ImpactControl : ModSystem
    {
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Ref<Effect> screenRef = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/ImpactFrame", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["ImpactFrame"] = new Filter(new ScreenShaderData(screenRef, "MainPS"), EffectPriority.VeryHigh);
                Filters.Scene["ImpactFrame"].Load();
            }
        }

        public float timer = 0;

        public Vector2 position;

        public bool active = false;

        public override void PostUpdateProjectiles()
        {
            if (Main.gameMenu || Main.netMode == NetmodeID.Server)
                return;

            timer += 0.007f;

            if (!active)
            {
                timer = 0;
                if (Filters.Scene["ImpactFrame"].IsActive())
                    Filters.Scene["ImpactFrame"].Deactivate();
                return;
            }

            if (timer > 2)
                active = false;
            Filters.Scene["ImpactFrame"].GetShader().UseProgress(timer);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["vnoiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["threshhold"].SetValue(0.65f);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["noiseThreshhold"].SetValue(MathHelper.Lerp(0.7f, 0.9f, timer));
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["waveThreshhold"].SetValue(0.7f);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["noiseRepeats"].SetValue(3.45f);
            Filters.Scene["ImpactFrame"].GetShader().UseTargetPosition(position + new Vector2(Main.offScreenRange, Main.offScreenRange) - Main.screenPosition);
            if (!Filters.Scene["ImpactFrame"].IsActive())
                Filters.Scene.Activate("ImpactFrame").GetShader().UseColor(Color.White.ToVector3()).UseSecondaryColor(Color.Black.ToVector3()).UseProgress(timer).UseTargetPosition(position - Main.screenPosition);
        }
    }
}
