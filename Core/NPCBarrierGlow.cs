using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using System;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Core
{ 
    public class NPCBarrierGlowSystem : ModSystem
    {
        public override void PreUpdateNPCs()
        {
            NPCBarrierGlow.anyEnemiesWithBarrier = false;
        }
    }
    public class NPCBarrierGlow : IOrderedLoadable
    {
        public static bool anyEnemiesWithBarrier = false;

        public static RenderTarget2D NPCTarget;

        public static RenderTarget2D NPCTargetBehindTiles;

        public static Vector2 oldScreenPos = Vector2.Zero;

        private static Color barrierColor => Color.Cyan;

        public float Priority => 1.1f;

        public void Load()
        {
            if (Main.dedServ)
                return;

            ResizeTarget();

            Main.OnPreDraw += Main_OnPreDraw;
            On.Terraria.Main.DrawNPCs += DrawBarrierOverlay;
        }

        public void Unload()
        {
            Main.OnPreDraw -= Main_OnPreDraw;
            On.Terraria.Main.DrawNPCs -= DrawBarrierOverlay;
        }

        public static void ResizeTarget()
        {
            Main.QueueMainThreadAction(() =>
            {
                NPCTargetBehindTiles = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                NPCTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.gameMenu || Main.dedServ || spriteBatch is null  || NPCTarget is null || NPCTargetBehindTiles is null || gD is null || !anyEnemiesWithBarrier)
                return;

            oldScreenPos = Main.screenPosition;

            RenderTargetBinding[] bindings = gD.GetRenderTargets();
            gD.SetRenderTarget(NPCTarget);
            gD.Clear(Color.Transparent);

            spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

            DrawAllNPCS(spriteBatch, false);

            spriteBatch.End();

            gD.SetRenderTarget(NPCTargetBehindTiles);
            gD.Clear(Color.Transparent);

            spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

            DrawAllNPCS(spriteBatch, true);

            spriteBatch.End();
            gD.SetRenderTargets(bindings);
        }

        private static void DrawAllNPCS(SpriteBatch spriteBatch, bool behindTiles)
        {
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC NPC = Main.npc[i];

                if (NPC.behindTiles != behindTiles)
                    continue;

                if (NPC.active && NPC.GetGlobalNPC<BarrierNPC>().Barrier > 0)
                {
                    if (NPC.ModNPC != null)
                    {
                        if (NPC.ModNPC is ModNPC ModNPC)
                        {
                            if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
                                Main.instance.DrawNPC(i, false);

                            ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
                        }
                    }
                    else
                        Main.instance.DrawNPC(i, false);
                }
            }
        }
        private void DrawBarrierOverlay(On.Terraria.Main.orig_DrawNPCs orig, Main self, bool behindTiles)
        {
            if (anyEnemiesWithBarrier)
                DrawNPCTarget(behindTiles ? NPCTargetBehindTiles : NPCTarget);

            if (!behindTiles)
                oldScreenPos = Main.screenPosition;

            orig(self, behindTiles);
        }

        private static void DrawNPCTarget(RenderTarget2D target)
        {
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            if (Main.dedServ || spriteBatch == null || target == null || gD == null)
                return;

            Vector2 translation = Main.screenPosition - oldScreenPos;
            translation *= Main.GameViewMatrix.Zoom;

            Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(-translation, 0));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, translationMatrix);

            float sin = (float)Math.Sin(Main.timeForVisualEffects * 0.06f);
            float opacity = (1.5f + (sin * 0.75f)) * 0.3f;

            Effect effect = Filters.Scene["NPCBarrier"].GetShader().Shader;
            effect.Parameters["barrierColor"].SetValue(barrierColor.ToVector4() * opacity);
            effect.Parameters["lightingTexture"].SetValue(StarlightRiver.LightingBufferInstance.ScreenLightingTexture);

            effect.CurrentTechnique.Passes[0].Apply();

            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * MathHelper.TwoPi;
                float distance = 3 + (2 * sin);

                Vector2 offset = angle.ToRotationVector2() * distance;
                spriteBatch.Draw(target, new Rectangle((int)offset.X, (int)offset.Y, Main.screenWidth, Main.screenHeight), Color.White);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
