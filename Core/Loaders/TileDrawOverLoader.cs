using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using System;
using Terraria;
using Terraria.ModLoader;


namespace StarlightRiver.Core.Loaders
{
	class TileDrawOverLoader : ILoadable
    {
        public float Priority { get => 1.1f; }

        public static RenderTarget2D projTarget;

        public void Load()
        {
            if (Main.dedServ)
                return;

            ResizeTarget();

            //TODO: Move these to wherever the fuck scalie keeps his detours
            On.Terraria.Main.DrawProjectiles += Main_DrawProjectiles;
            Main.OnPreDraw += Main_OnPreDraw;
            On.Terraria.Main.Update += Main_Update;
        }

        public static void ResizeTarget() => projTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight);
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
            RenderTarget2D tileTarget = Main.instance.tileTarget;
            if (tileTarget == null)
                return;
            GraphicsDevice gD = Main.graphics.GraphicsDevice;
            SpriteBatch spriteBatch = Main.spriteBatch;

            RenderTargetBinding[] bindings = gD.GetRenderTargets();

            gD.SetRenderTarget(projTarget);
            gD.Clear(Color.Transparent);
            spriteBatch.Begin();

            for (int i = 0; i < Main.projectile.Length; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.modProjectile is IDrawOverTiles iface)
                {
                    iface.DrawOverTiles(spriteBatch);
                }
            }

            spriteBatch.End();
            gD.SetRenderTargets(bindings);
        }

        private void DrawTarget()
        {
            RenderTarget2D tileTarget = Main.instance.tileTarget;
            if (tileTarget == null)
                return;


            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Effect effect = Filters.Scene["OverTileShader"].GetShader().Shader;
            effect.Parameters["TileTarget"].SetValue(tileTarget);
            effect.CurrentTechnique.Passes[0].Apply();

            Main.spriteBatch.Draw(projTarget, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            Main.spriteBatch.End();
        }
    }
}
