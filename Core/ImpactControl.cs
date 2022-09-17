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
            if (Main.gameMenu)
                return;

            timer += 0.04f;

            if (!active)
            {
                timer = 0;
                if (Filters.Scene["ImpactFrame"].IsActive())
                    Filters.Scene["ImpactFrame"].Deactivate();

                return;
            }

            if (timer > 2)
                active = false;

            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["uProgress"].SetValue(timer);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["vnoiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);
            Filters.Scene["ImpactFrame"].GetShader().Shader.Parameters["uTargetPosition"].SetValue((position - Main.screenPosition) / Main.ScreenSize.ToVector2());
            if (Main.netMode != NetmodeID.Server && !Filters.Scene["ImpactFrame"].IsActive())
            {
                Filters.Scene.Activate("ImpactFrame").GetShader().UseColor(Color.White.ToVector3()).UseSecondaryColor(Color.Black.ToVector3()).UseProgress(timer);
            }
        }
    }
}
