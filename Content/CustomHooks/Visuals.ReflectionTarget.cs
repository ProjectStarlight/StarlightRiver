﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Configs;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace StarlightRiver.Content.CustomHooks
{
    class ReflectionTarget : HookGroup
    {
        //Drawing Player to Target. Should be safe. Excuse me if im duplicating something that alr exists :p
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public const String simpleReflectionShaderPath = "StarlightRiver:SimpleReflection";

        private MethodInfo NpcsOverTilesDrawMethod;
        private MethodInfo PlayerBehindNPCsDrawMethod;
        private MethodInfo NpcsBehindTilesDrawMethod;
        private MethodInfo PlayerAfterProjDrawMethod;
        private MethodInfo ProjectileDrawMethod;
        private MethodInfo drawCachedProjsMethod;
        private MethodInfo drawCachedNPCsMethod;
        private MethodInfo dustDrawMethod;
        private MethodInfo goreDrawMethod;

        public static RenderTarget2D Target;
        private static RenderTarget2D reflectionNormalMapTarget;

        /// <summary>
        /// lets other components know that targets on this component are being rendered so they cannot try to use them.
        /// </summary>
        public static bool canUseTarget = false;

        /// <summary>
        /// determines whether or not to draw the entities to the reflection RT, set to true if either the wall reflections are active OR the homogenized version is in use
        /// </summary>
        public static bool isDrawReflectablesThisFrame = false;

        /// <summary>
        /// determines whether or not to apply the shader and actually draw the wall reflections to the screen, only set to true if the wall reflections are in use this frame
        /// </summary>
        public static bool applyWallReflectionsThisFrame = false;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.QueueMainThreadAction(() =>
            {
                Target = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                reflectionNormalMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });


            
            NpcsOverTilesDrawMethod = typeof(Main).GetMethod("DoDraw_DrawNPCsOverTiles", BindingFlags.NonPublic | BindingFlags.Instance);
            NpcsBehindTilesDrawMethod = typeof(Main).GetMethod("DoDraw_DrawNPCsBehindTiles", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerBehindNPCsDrawMethod = typeof(Main).GetMethod("DrawPlayers_BehindNPCs", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerAfterProjDrawMethod = typeof(Main).GetMethod("DrawPlayers_AfterProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);
            ProjectileDrawMethod = typeof(Main).GetMethod("DrawProjectiles", BindingFlags.NonPublic | BindingFlags.Instance);
            drawCachedProjsMethod = typeof(Main).GetMethod("DrawCachedProjs", BindingFlags.NonPublic | BindingFlags.Instance);
            drawCachedNPCsMethod = typeof(Main).GetMethod("DrawCachedNPCs", BindingFlags.NonPublic | BindingFlags.Instance);
            dustDrawMethod = typeof(Main).GetMethod("DrawDust", BindingFlags.NonPublic | BindingFlags.Instance);
            goreDrawMethod = typeof(Main).GetMethod("DrawGore", BindingFlags.NonPublic | BindingFlags.Instance);

            On.Terraria.Main.SetDisplayMode += RefreshTargets;
            On.Terraria.Main.CheckMonoliths += DrawTargets;
            On.Terraria.Main.DoDraw_WallsAndBlacks += DrawWallReflectionLayer;

            ReflectionTarget.DrawWallReflectionNormalMapEvent += drawGlassWallReflectionNormalMap;

            GameShaders.Misc[simpleReflectionShaderPath] = new MiscShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/SimpleReflection").Value), "TileReflectionPass");
        }

        public override void Unload()
        {
            On.Terraria.Main.SetDisplayMode -= RefreshTargets;
            On.Terraria.Main.CheckMonoliths -= DrawTargets;
            On.Terraria.Main.DoDraw_WallsAndBlacks -= DrawWallReflectionLayer;

            ReflectionTarget.DrawWallReflectionNormalMapEvent -= drawGlassWallReflectionNormalMap;
        }

        private void RefreshTargets(On.Terraria.Main.orig_SetDisplayMode orig, int width, int height, bool fullscreen)
        {
            if (!Main.gameInactive && (width != Main.screenWidth || height != Main.screenHeight))
            {
                Target = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height);
                reflectionNormalMapTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, width, height);
            }

            orig(width, height, fullscreen);
        }

        /// <summary>
        /// Use this event for anything that wants to add reflections to the layer right after walls are drawn, 
        /// elements higher up in the draw chain or with custom offsets will have to use the version of the reflection shader with homogenized coordinates (look at vitricbossAltar as an example)
        /// </summary>
        public static event DrawWallReflectionNormalMapDelegate DrawWallReflectionNormalMapEvent;
        public delegate void DrawWallReflectionNormalMapDelegate(SpriteBatch spritebatch);

        private void DrawTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            //TODO: this may benefit from adding booleans for other places in the code to check if they're going to use the RTs since we don't necessarily need these generated on every frame for some performance improvements

            orig();

            if (Main.gameMenu)
                return;

            ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

            if (reflectionConfig.isReflectingAnything())
            {
                GraphicsDevice GD = Main.graphics.GraphicsDevice;
                SpriteBatch sb = Main.spriteBatch;

                RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();
                canUseTarget = false;

                GD.SetRenderTarget(reflectionNormalMapTarget);
                GD.Clear(Color.Transparent);
                Main.GameViewMatrix.Zoom = new Vector2(1,1);
                Main.spriteBatch.Begin(SpriteSortMode.Texture, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                DrawWallReflectionNormalMapEvent?.Invoke(sb);

                sb.End();


                if (isDrawReflectablesThisFrame)
                {
                    DrawReflectableEntities(GD, sb, reflectionConfig);
                    ReflectionTarget.isDrawReflectablesThisFrame = false;
                }

                Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets1);
            }

            canUseTarget = true;
        }

        private void DrawReflectableEntities(GraphicsDevice GD, SpriteBatch sb, ReflectionSubConfig reflectionConfig)
        {
            GD.SetRenderTarget(Target);
            GD.Clear(Color.Transparent);

            if (reflectionConfig.NpcReflectionsOn)
                NpcsBehindTilesDrawMethod?.Invoke(Main.instance, null);

            if (reflectionConfig.PlayerReflectionsOn)
                PlayerBehindNPCsDrawMethod?.Invoke(Main.instance, null);

            if (reflectionConfig.NpcReflectionsOn)
                NpcsOverTilesDrawMethod?.Invoke(Main.instance, null);

            if (reflectionConfig.ProjReflectionsOn)
            {
                drawCachedProjsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheProjsBehindProjectiles, true });
                ProjectileDrawMethod?.Invoke(Main.instance, null);
            }

            if (reflectionConfig.PlayerReflectionsOn)
                PlayerAfterProjDrawMethod?.Invoke(Main.instance, new object[] { }); ;

            if (reflectionConfig.ProjReflectionsOn) { }
            drawCachedProjsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheProjsOverPlayers, true });

                
                if (reflectionConfig.NpcReflectionsOn)
                {
                    drawCachedNPCsMethod?.Invoke(Main.instance, new object[] { Main.instance.DrawCacheNPCsOverPlayers, false });

                    if (Main.LocalPlayer.InModBiome(ModContent.GetInstance<Biomes.PermafrostTempleBiome>()))
                    {
                        Main.spriteBatch.Begin();
                        DrawUnderCathedralWater.DrawWater();
                        Main.spriteBatch.End();
                    }
                }

            if (reflectionConfig.DustReflectionsOn)
            {
                sb.Begin(SpriteSortMode.Deferred, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                try
                {
                    //tml does this try catch for some reason, maybe gores are bugged in this version, v2022.3.35.3, possible TODO: remove the try catch if tml removes theirs
                    goreDrawMethod?.Invoke(Main.instance, null);
                }
                catch (Exception e2)
                {
                    TimeLogger.DrawException(e2);
                }
                sb.End();

                dustDrawMethod?.Invoke(Main.instance, null);
            }

            Overlays.Scene.Draw(sb, RenderLayers.Entities, true);
        }

        /// <summary>
        /// draw background reflections immediately after wall tiles are drawn 
        /// </summary>
        /// <param name="orig"></param>
        ///
        public void DrawWallReflectionLayer(On.Terraria.Main.orig_DoDraw_WallsAndBlacks orig, Main self)
        {
            orig(self);

            if (ReflectionTarget.applyWallReflectionsThisFrame)
            {
                DrawReflection(Main.spriteBatch, screenPos: Vector2.Zero, normalMap: ReflectionTarget.reflectionNormalMapTarget, flatOffset: new Vector2(-0.0075f, 0.016f), offsetScale: 0.05f, tintColor: Color.White, restartSpriteBatch: true);
                ReflectionTarget.applyWallReflectionsThisFrame = false;
            }
        }

        /// <summary>
        /// helper function to set params onto the reflection shader and draw to screen when called
        /// </summary>
        public static void DrawReflection(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D normalMap, Vector2 flatOffset, float offsetScale, Color tintColor, bool restartSpriteBatch = true)
        {
            ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

            if (ReflectionTarget.canUseTarget && reflectionConfig.isReflectingAnything())
            {
                
                if (restartSpriteBatch)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
                }

                DrawData data = new DrawData(normalMap, screenPos, new Color(255,255,255, 0));

                //need to force the registers into using the proper data
                Main.graphics.GraphicsDevice.Textures[1] = ReflectionTarget.Target;
                Main.graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

                GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["reflectionTargetSize"].SetValue(ReflectionTarget.Target.Size());
                GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["flatOffset"].SetValue(flatOffset);
                GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["offsetScale"].SetValue(offsetScale);
                GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["normalMapPosition"].SetValue(new Vector2(screenPos.X / ReflectionTarget.Target.Width, screenPos.Y / ReflectionTarget.Target.Height));
                GameShaders.Misc[simpleReflectionShaderPath].Shader.Parameters["tintColor"].SetValue(tintColor.ToVector4());

                GameShaders.Misc[simpleReflectionShaderPath].Apply(data);

                data.Draw(spriteBatch);

                if (restartSpriteBatch)
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
                }
            }
        }

        public void drawGlassWallReflectionNormalMap(SpriteBatch spriteBatch)
        {
            var shader = Filters.Scene["ReflectionMapper"].GetShader().Shader;

            if (shader is null)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Texture, BlendState.AlphaBlend, SamplerState.PointClamp, default, default, Filters.Scene["ReflectionMapper"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);

            shader.Parameters["uColor"].SetValue(new Vector3(0.5f, 0.5f, 1f));
            shader.Parameters["uIntensity"].SetValue(0.5f);

            int TileSearchSize = 30; //limit distance from Player for getting these wall tiles
            for (int i = -TileSearchSize; i < TileSearchSize; i++)
            {
                for (int j = -TileSearchSize; j < TileSearchSize; j++)
                {
                    Point p = (Main.LocalPlayer.position / 16).ToPoint();
                    Point pij = new Point(p.X + i, p.Y + j);

                    if (WorldGen.InWorld(pij.X, pij.Y))
                    {
                        Tile tile = Framing.GetTileSafely(pij);
                        ushort type = tile.WallType;

                        if (type == WallID.Glass
                         || type == WallID.BlueStainedGlass
                         || type == WallID.GreenStainedGlass
                         || type == WallID.PurpleStainedGlass
                         || type == WallID.YellowStainedGlass
                         || type == WallID.RedStainedGlass)
                        {
                            Vector2 pos = pij.ToVector2() * 16;
                            Texture2D tex = TextureAssets.Wall[type].Value;
                            //not sure if tile.WallFrame* is the correct value
                            if (tex != null) spriteBatch.Draw(TextureAssets.Wall[type].Value, pos - Main.screenPosition - new Vector2(8, 8), new Rectangle(tile.WallFrameX, tile.WallFrameY, 36, 36), new Color(128, 128, 255, 255));
                            ReflectionTarget.isDrawReflectablesThisFrame = true;
                            ReflectionTarget.applyWallReflectionsThisFrame = true;
                        }
                    }

                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Texture, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}