using Microsoft.Xna.Framework;
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

        public static bool canUseTarget = false;

        public override void Load()
        {
            if (Main.dedServ) //PORTTODO: Re-enable this when appropriate
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
            On.Terraria.Main.DoDraw_Tiles_NonSolid += DrawReflectionLayer;

            ReflectionTarget.DrawReflectionNormalMapEvent += drawGlassWallReflectionNormalMap;

            GameShaders.Misc["StarlightRiver:TileReflection"] = new MiscShaderData(new Ref<Effect>(StarlightRiver.Instance.Assets.Request<Effect>("Effects/TileReflection").Value), "TileReflectionPass");
        }

        public override void Unload()
        {
            On.Terraria.Main.SetDisplayMode -= RefreshTargets;
            On.Terraria.Main.CheckMonoliths -= DrawTargets;
            On.Terraria.Main.DoDraw_Tiles_NonSolid -= DrawReflectionLayer;

            ReflectionTarget.DrawReflectionNormalMapEvent -= drawGlassWallReflectionNormalMap;
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
        /// Use this event for anything that wants to add reflections to the layer before entities are drawn
        /// </summary>
        public static event DrawReflectionNormalMapDelegate DrawReflectionNormalMapEvent;
        public delegate void DrawReflectionNormalMapDelegate(SpriteBatch spritebatch);

        private void DrawTargets(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            //TODO: this may benefit from adding booleans for other places in the code to check if they're going to use the RTs since we don't necessarily need these generated on every frame for some performance improvements

            orig();

            if (Main.gameMenu)
                return;

            ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

            if (reflectionConfig.isReflectingAnything())
            {
                RenderTargetBinding[] oldtargets1 = Main.graphics.GraphicsDevice.GetRenderTargets();
                canUseTarget = false;

                GraphicsDevice GD = Main.graphics.GraphicsDevice;
                SpriteBatch sb = Main.spriteBatch;

                GD.SetRenderTarget(Target);
                GD.Clear(Color.Transparent);

                Vector2 originalZoom = Main.GameViewMatrix.Zoom;
                Main.GameViewMatrix.Zoom = originalZoom;

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
                    PlayerAfterProjDrawMethod?.Invoke(Main.instance, new object[] { });;

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

                GD.SetRenderTarget(reflectionNormalMapTarget);
                GD.Clear(Color.Transparent);

                Main.GameViewMatrix.Zoom = originalZoom;

                Main.spriteBatch.Begin(SpriteSortMode.Texture, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
                DrawReflectionNormalMapEvent?.Invoke(sb);
                sb.End();

                Main.graphics.GraphicsDevice.SetRenderTargets(oldtargets1);
            }

            canUseTarget = true;
        }

        /// <summary>
        /// draw background reflections immediately after non-solid tiles are drawn 
        /// </summary>
        /// <param name="orig"></param>
        ///
        public void DrawReflectionLayer(On.Terraria.Main.orig_DoDraw_Tiles_NonSolid orig, Main self)
        {
            orig(self);

            ReflectionSubConfig reflectionConfig = ModContent.GetInstance<GraphicsConfig>().ReflectionConfig;

            if (ReflectionTarget.canUseTarget && reflectionConfig.isReflectingAnything())
            {
                SpriteBatch spriteBatch = Main.spriteBatch;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

                DrawData data = new DrawData(ReflectionTarget.reflectionNormalMapTarget, Vector2.Zero, Color.White);

                GameShaders.Misc["StarlightRiver:TileReflection"].Shader.Parameters["ReflectionTarget"].SetValue(ReflectionTarget.Target);
                GameShaders.Misc["StarlightRiver:TileReflection"].Shader.Parameters["flatOffset"].SetValue(new Vector2(-0.0075f, 0.015f) * Main.GameViewMatrix.Zoom);
                GameShaders.Misc["StarlightRiver:TileReflection"].Shader.Parameters["offsetScale"].SetValue(0.05f * Main.GameViewMatrix.Zoom.Length());

                GameShaders.Misc["StarlightRiver:TileReflection"].Apply(data);

                data.Draw(spriteBatch);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, (Effect)null, Main.Transform);
            }
        }

        public void drawGlassWallReflectionNormalMap(SpriteBatch spriteBatch)
        {
            var shader = Filters.Scene["ReflectionMapper"].GetShader().Shader;

            if (shader is null)
                return;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Texture, default, SamplerState.PointClamp, default, default, Filters.Scene["ReflectionMapper"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);

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
                        }
                    }

                }
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Texture, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}