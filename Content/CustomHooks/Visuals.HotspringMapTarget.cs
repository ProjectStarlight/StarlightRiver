using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Underground;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class HotspringMapTarget : HookGroup
	{
        public static RenderTarget2D hotspringMapTarget;
        public static RenderTarget2D hotspringShineTarget;

        //Creates a RenderTarget for the hotpsring water shader.
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Main.OnPreDraw += HotspringTarget;
        }

        public override void Unload()
        {
            Main.OnPreDraw -= HotspringTarget;
            hotspringMapTarget = null;
        }


        private void HotspringTarget(GameTime obj)
        {
            if (Main.gameMenu) return;

            var graphics = Main.graphics.GraphicsDevice;

            if (hotspringMapTarget is null || hotspringMapTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                hotspringMapTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (hotspringShineTarget is null || hotspringShineTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                hotspringShineTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);



            graphics.SetRenderTarget(hotspringMapTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];
                if (proj.active && proj.modProjectile is HotspringFountainDummy)
                    (proj.modProjectile as HotspringFountainDummy).DrawMap(Main.spriteBatch);
            }

            Main.spriteBatch.End();
            graphics.SetRenderTarget(null);



            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

            Main.graphics.GraphicsDevice.SetRenderTarget(hotspringShineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Texture2D tex2 = Terraria.ModLoader.ModContent.GetTexture("StarlightRiver/Assets/Misc/HotspringWaterMap");

            for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
                for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
                {
                    var pos = (new Vector2(i, j) - new Vector2(Main.screenPosition.X % tex2.Width, Main.screenPosition.Y % tex2.Height));
                    Main.spriteBatch.Draw(tex2, pos, null, Color.White);
                }

            Main.spriteBatch.End();
            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
