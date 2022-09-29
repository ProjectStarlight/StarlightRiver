using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using System;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.CustomHooks;

namespace StarlightRiver.Core.Loaders
{
    class TileDrawOverLoader : IOrderedLoadable
    {
        public float Priority { get => 1.1f; }

        public static RenderTarget2D projTarget;
        public static RenderTarget2D tileTarget;

        private Vector2 oldScreenPos;
        private Vector2 offsetOverTime;

        public void Load()
        {
            if (Main.dedServ)
                return;

            ResizeTarget();

            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
            Main.OnPreDraw += Main_OnPreDraw;
            On.Terraria.Main.Update += Main_Update;
        }

        public static void ResizeTarget()
        {
            projTarget?.Dispose();
            tileTarget?.Dispose();

            Main.QueueMainThreadAction(() =>
            {
                projTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
                tileTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
            });
        }

        private void Main_DrawProjectiles(On.Terraria.Main.orig_DrawProjectiles orig, Main self)
        {
            orig(self);
            DrawTarget();
        }

        private void Main_OnPreDraw(GameTime obj)
        {
            if (Main.spriteBatch != null)
                DrawToTarget();
        }

        private static void Main_Update(On.Terraria.Main.orig_Update orig, Main self, GameTime gameTime)
        {
            if (StarlightRiver.Instance != null)
                StarlightRiver.Instance.CheckScreenSize();

            orig(self, gameTime);
        }

        public void Unload()
        {
            On.Terraria.Main.DrawProjectiles -= Main_DrawProjectiles;
            Main.OnPreDraw -= Main_OnPreDraw;
            On.Terraria.Main.Update -= Main_Update;

            projTarget = null;
        }

        private void DrawToTarget()
        {
            if (Main.gameMenu || Main.instance.tileTarget == null || Main.instance.tileTarget.IsDisposed)
                return;

            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            RenderTargetBinding[] bindings = gD.GetRenderTargets();
            //ScreenPosition Rounds, Translation Matrix doesn't work :(. If someone finds a better way to do this I would be glad cat :).

            Vector2 translation = new Vector2(Main.LocalPlayer.velocity.X, Main.LocalPlayer.velocity.Y);

            Vector2 input =
                new Vector2(Main.leftWorld + 656f, Main.topWorld + 656f) - Main.GameViewMatrix.Translation;

            Vector2 input2 =
                new Vector2(Main.rightWorld - (float)Main.screenWidth / Main.GameViewMatrix.Zoom.X - 672f,
                Main.bottomWorld - (float)Main.screenHeight / Main.GameViewMatrix.Zoom.Y - 672f) - Main.GameViewMatrix.Translation;

            if (
               Main.screenPosition.X <= input.X
               || Main.screenPosition.X >= input2.X)
            {
                translation.X = 0;
            }
            
            if (
               Main.screenPosition.Y <= input.Y
               || Main.screenPosition.Y >= input2.Y)
            {
                translation.Y = 0;
            }

            gD.SetRenderTarget(projTarget);
            gD.Clear(Color.Transparent);

            spriteBatch.Begin(
                SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                RasterizerState.CullNone, null,
                Matrix.CreateTranslation(new Vector3(-translation.X, -translation.Y, 0)));

            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.ModProjectile is IDrawOverTiles iface)
                {
                    iface.DrawOverTiles(spriteBatch);
                }
            }

            spriteBatch.End();

            gD.SetRenderTarget(tileTarget); //doing this to (hopefully) crop the rendertarget?
            gD.Clear(Color.Transparent);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null);
            spriteBatch.Draw(Main.instance.tileTarget, Main.sceneTilePos - Main.screenPosition - translation, Color.White);
            spriteBatch.Draw(Main.instance.tile2Target, Main.sceneTile2Pos - Main.screenPosition - translation , Color.White);
            spriteBatch.End();

            gD.SetRenderTargets(bindings);

            oldScreenPos = Main.screenPosition;
        }

        private void DrawTarget()
        {
            if (tileTarget == null || projTarget == null)
                return;

            Effect effect = Filters.Scene["OverTileShader"].GetShader().Shader;

            if (effect is null)
                return;

            effect.Parameters["TileTarget"].SetValue(tileTarget);

            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            effect.CurrentTechnique.Passes[0].Apply();
            Main.spriteBatch.Draw(projTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
        }
    }
}
