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
using static Terraria.Utils;
using static StarlightRiver.Helpers.Helper;

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

            On.Terraria.Main.CheckMonoliths += HotspringTarget;
        }

        public override void Unload()
        {
            hotspringMapTarget = null;
        }

        private void HotspringTarget(On.Terraria.Main.orig_CheckMonoliths orig)
        {
            orig();

            if (Main.gameMenu) 
                return;

            var graphics = Main.graphics.GraphicsDevice;

            if (hotspringMapTarget is null || hotspringMapTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                hotspringMapTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);

            if (hotspringShineTarget is null || hotspringShineTarget.Size() != new Vector2(Main.screenWidth, Main.screenHeight))
                hotspringShineTarget = new RenderTarget2D(graphics, Main.screenWidth, Main.screenHeight, default, default, default, default, RenderTargetUsage.PreserveContents);




            graphics.SetRenderTarget(hotspringMapTarget);

            graphics.Clear(Color.Transparent);
            Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default);

            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                var proj = Main.projectile[k];
                if (proj.active && proj.ModProjectile is HotspringFountainDummy)
                    (proj.ModProjectile as HotspringFountainDummy).DrawMap(Main.spriteBatch);
            }

            Main.spriteBatch.End();
            graphics.SetRenderTarget(null);


            //if (Main.renderCount == 3)//
            //{
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default);

            Main.graphics.GraphicsDevice.SetRenderTarget(hotspringShineTarget);
            Main.graphics.GraphicsDevice.Clear(Color.Transparent);

            Texture2D tex2 = Terraria.ModLoader.ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/HotspringWaterMap").Value;

            Vector2 screenSize = Helpers.Helper.ScreenSize;
            Vector2 textureSize = tex2.Size();

            Vector2 texWrapCount = (screenSize / textureSize).Round().TwoValueMax(Vector2.One);

            Vector2 texScaleDiv = textureSize / screenSize * texWrapCount;
            Vector2 texOutSize = textureSize / texScaleDiv;

            for (int i = 0; i <= texWrapCount.X; i++)
                for (int j = 0; j <= texWrapCount.Y; j++)
                {
                    Vector2 pos = new Vector2(i, j) * texOutSize;
                    Vector2 off = new Vector2((Main.screenPosition.X) % texOutSize.X, (Main.screenPosition.Y) % texOutSize.Y);
                    Main.spriteBatch.Draw(tex2, new Rectangle((int)(pos.X - off.X), (int)(pos.Y - off.Y), (int)texOutSize.X, (int)texOutSize.Y), null, Color.White);
                }

            //The seam issue is not in this file, See StarlightRiver.cs and enable the commented out PostDrawInterface hook to view RTs
            //for (int i = -tex2.Width; i <= Main.screenWidth + tex2.Width; i += tex2.Width)
            //    for (int j = -tex2.Height; j <= Main.screenHeight + tex2.Height; j += tex2.Height)
            //    {
            //        //the divide by 1.3 and 1.5 are what keep the tile tied to the world location, seems to be tied to the 2 magic numbers in HotspringAddon.cs
            //        Vector2 pos = (new Vector2(i, j) - new Vector2((Main.screenPosition.X / 1.3f) % tex2.Width, (Main.screenPosition.Y / 1.5f) % tex2.Height));
            //        Main.spriteBatch.Draw(tex2, pos, null, Color.White);

            //        //Vector2 debugSize = new Vector2(32, 156);

            //        //float posOffY = (float)Math.Sin(Main.GameUpdateCount / 15f) * (Main.screenHeight / 2.2f);
            //        //Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle(0, Main.screenHeight / 2 + (int)posOffY, (int)debugSize.X, (int)debugSize.Y), Color.Red);

            //        //float posOffY2 = (float)Math.Sin(Main.GameUpdateCount / 13.33f) * (Main.screenHeight / 2.2f);
            //        //Main.spriteBatch.Draw(Main.blackTileTexture, new Rectangle(Main.screenWidth - (int)debugSize.X, Main.screenHeight / 2 + (int)posOffY2, (int)debugSize.X, (int)debugSize.Y), Color.Red);
            //    }

            Main.spriteBatch.End();
            //}

            Main.graphics.GraphicsDevice.SetRenderTarget(null);
        }
    }
}
