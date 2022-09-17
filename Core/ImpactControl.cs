//TODO:
//Eliminate curving
//Fix wave
//Make lines more solid somehow
//Make it fade
//Fix targetting offset
//Make noise move with screen
//clean the fuck up

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
        public static RenderTarget2D baseTarget;
        public static RenderTarget2D shaderTarget;
        public static RenderTarget2D shaderTargetSwap;
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                Ref<Effect> screenRef = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/ImpactFrame_Outline", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["ImpactFrame_Outline"] = new Filter(new ScreenShaderData(screenRef, "MainPS"), EffectPriority.VeryHigh);
                Filters.Scene["ImpactFrame_Outline"].Load();

                Ref<Effect> screenRef2 = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/ImpactFrame_Blur", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["ImpactFrame_Blur"] = new Filter(new ScreenShaderData(screenRef2, "MainPS"), EffectPriority.VeryHigh);
                Filters.Scene["ImpactFrame_Blur"].Load();

                Ref<Effect> screenRef3 = new Ref<Effect>(Mod.Assets.Request<Effect>("Effects/ImpactFrame_Fade", AssetRequestMode.ImmediateLoad).Value);
                Filters.Scene["ImpactFrame_Fade"] = new Filter(new ScreenShaderData(screenRef3, "MainPS"), EffectPriority.VeryHigh);
                Filters.Scene["ImpactFrame_Fade"].Load();

                On.Terraria.Graphics.Effects.FilterManager.EndCapture += FilterManager_EndCapture;
                Main.OnPreDraw += Main_OnPreDraw;
                ResizeTarget();
            }
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            if (active && Main.screenTarget is not null)
            {
                GraphicsDevice gD = Main.graphics.GraphicsDevice;
                RenderTargetBinding[] bindings = gD.GetRenderTargets();

                gD.SetRenderTarget(baseTarget);
                gD.Clear(Main.ColorOfTheSkies);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
                Main.spriteBatch.Draw(Main.screenTarget, Vector2.Zero, Color.White);
                Main.spriteBatch.End();

                gD.SetRenderTarget(shaderTarget);
                gD.Clear(Color.Transparent);

                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["threshhold"].SetValue(1f);
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uIntensity"].SetValue(4);
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uColor"].SetValue(Color.White.ToVector3());
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uSecondaryColor"].SetValue(new Color(0,0,0.01f).ToVector3());
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uProgress"].SetValue(timer);
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uTargetPosition"].SetValue(position - Main.screenPosition);
                Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["uScreenResolution"].SetValue(Main.ScreenSize.ToVector2());
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, Filters.Scene["ImpactFrame_Outline"].GetShader().Shader);
                Main.spriteBatch.Draw(baseTarget, Vector2.Zero, Color.White);
                Main.spriteBatch.End();


                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["vnoiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["originalTex"].SetValue(baseTarget);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["noiseThreshhold"].SetValue(MathHelper.Lerp(0.7f, 0.9f, timer));
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["noiseRepeats"].SetValue(3.45f);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uColor"].SetValue(Color.White.ToVector3());
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uSecondaryColor"].SetValue(new Color(0, 0, 0.01f).ToVector3());
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uProgress"].SetValue(timer);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uIntensity"].SetValue(5);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uTargetPosition"].SetValue(position - Main.screenPosition);
                Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["uScreenResolution"].SetValue(Main.ScreenSize.ToVector2());

                bool useSwap = true;
                for (int i = 0; i < 5; i++)
                {
                    BlurTarget(useSwap);
                    useSwap = !useSwap;
                }


                Filters.Scene["ImpactFrame_Fade"].GetShader().Shader.Parameters["originalTex"].SetValue(baseTarget);
                Filters.Scene["ImpactFrame_Fade"].GetShader().Shader.Parameters["fade"].SetValue((float)Math.Min(1, Math.Pow(timer, 0.75f)));
                gD.SetRenderTarget(useSwap ? shaderTargetSwap : shaderTarget);
                gD.Clear(Color.Transparent);

                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, Filters.Scene["ImpactFrame_Fade"].GetShader().Shader);
                Main.spriteBatch.Draw(useSwap ? shaderTarget : shaderTargetSwap, Vector2.Zero, Color.White);
                Main.spriteBatch.End();
                gD.SetRenderTargets(bindings);

            }
        }

        private void BlurTarget(bool useSwap)
        {
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            gD.SetRenderTarget(useSwap ? shaderTargetSwap : shaderTarget);
            gD.Clear(Color.Transparent);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, Filters.Scene["ImpactFrame_Blur"].GetShader().Shader);
            Main.spriteBatch.Draw(useSwap ? shaderTarget : shaderTargetSwap, Vector2.Zero, Color.White);
            Main.spriteBatch.End();
        }

        public static void ResizeTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                baseTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                shaderTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                shaderTargetSwap = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });
        }

        private void FilterManager_EndCapture(On.Terraria.Graphics.Effects.FilterManager.orig_EndCapture orig, FilterManager self, RenderTarget2D finalTexture, RenderTarget2D screenTarget1, RenderTarget2D screenTarget2, Color clearColor)
        {
            orig(self, finalTexture, active ? shaderTarget : screenTarget1, screenTarget2, clearColor);
        }

        public float timer = 0;

        public Vector2 position;

        public bool active = false;

        public override void PostUpdateProjectiles()
        { 
            if (Main.gameMenu || Main.netMode == NetmodeID.Server)
                return;

            timer += 0.01f;

            /*if (Filters.Scene["ImpactFrame_Outline"].IsActive())
                Filters.Scene["ImpactFrame_Outline"].Deactivate();

            if (Filters.Scene["ImpactFrame_Blur"].IsActive())
                Filters.Scene["ImpactFrame_Blur"].Deactivate();

            if (Filters.Scene["ImpactFrame_Blur2"].IsActive())
                Filters.Scene["ImpactFrame_Blur2"].Deactivate();*/

            if (!active)
            {
                timer = 0;
                return;
            }

            if (timer > 1.3f)
                active = false;

           /* Filters.Scene["ImpactFrame_Blur"].GetShader().UseProgress(timer);
            Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["vnoiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);
            Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["originalTex"].SetValue(target);
            Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["noiseThreshhold"].SetValue(MathHelper.Lerp(0.7f, 0.9f, timer));
            Filters.Scene["ImpactFrame_Blur"].GetShader().Shader.Parameters["noiseRepeats"].SetValue(3.45f);
            Filters.Scene["ImpactFrame_Blur"].GetShader().UseTargetPosition(position + new Vector2(Main.offScreenRange, Main.offScreenRange) - Main.screenPosition);
            //if (!Filters.Scene["ImpactFrame_Blur"].IsActive())
            //    Filters.Scene.Activate("ImpactFrame_Blur").GetShader().UseColor(Color.White.ToVector3()).UseSecondaryColor(Color.Black.ToVector3()).UseProgress(1.3f).UseTargetPosition(position - Main.screenPosition);

            Filters.Scene["ImpactFrame_Blur2"].GetShader().UseProgress(timer);
            Filters.Scene["ImpactFrame_Blur2"].GetShader().Shader.Parameters["originalTex"].SetValue(target);
            Filters.Scene["ImpactFrame_Blur2"].GetShader().Shader.Parameters["vnoiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/ShaderNoiseLooping").Value);
            Filters.Scene["ImpactFrame_Blur2"].GetShader().Shader.Parameters["noiseThreshhold"].SetValue(MathHelper.Lerp(0.7f, 0.9f, timer));
            Filters.Scene["ImpactFrame_Blur2"].GetShader().Shader.Parameters["noiseRepeats"].SetValue(3.45f);
            Filters.Scene["ImpactFrame_Blur2"].GetShader().UseTargetPosition(position + new Vector2(Main.offScreenRange, Main.offScreenRange) - Main.screenPosition);
            //if (!Filters.Scene["ImpactFrame_Blur2"].IsActive())
            //    Filters.Scene.Activate("ImpactFrame_Blur2").GetShader().UseColor(Color.White.ToVector3()).UseSecondaryColor(Color.Black.ToVector3()).UseProgress(timer).UseTargetPosition(position - Main.screenPosition);

            Filters.Scene["ImpactFrame_Outline"].GetShader().UseProgress(timer);
            Filters.Scene["ImpactFrame_Outline"].GetShader().Shader.Parameters["threshhold"].SetValue(1f);
            Filters.Scene["ImpactFrame_Outline"].GetShader().UseTargetPosition(position + new Vector2(Main.offScreenRange, Main.offScreenRange) - Main.screenPosition);
            //if (!Filters.Scene["ImpactFrame_Outline"].IsActive())
            //    Filters.Scene.Activate("ImpactFrame_Outline").GetShader().UseColor(Color.White.ToVector3()).UseSecondaryColor(Color.Black.ToVector3()).UseProgress(timer).UseTargetPosition(position - Main.screenPosition);*/
        }
    }
}
