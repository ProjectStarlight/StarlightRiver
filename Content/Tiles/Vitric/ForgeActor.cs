using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	class ForgeActor : DummyTile
    {
        public override int DummyType => ProjectileType<ForgeActorDummy>();

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => (this).QuickSetFurniture(1, 1, DustType<Content.Dusts.Air>(), SoundID.Shatter, false, Color.Black);
    }

    class ForgeActorDummy : Dummy
    {
        public ForgeActorDummy() : base(TileType<ForgeActor>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
            Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
            behindNPCsAndTiles.Add(index);
		}

		public override void PostDraw(Color lightColor)
		{
            var spriteBatch = Main.spriteBatch;

            Player player = Main.player[Main.myPlayer];

            Vector2 pos = (Projectile.position - new Vector2(567, 400))- Main.screenPosition;
            Texture2D backdrop = Request<Texture2D>(AssetDirectory.Glassweaver + "Backdrop").Value;
            Texture2D backdropGlow = Request<Texture2D>(AssetDirectory.Glassweaver + "BackdropGlow").Value;

            Vector2 parallaxOffset = new Vector2(player.Center.X - Projectile.position.X, 0) * 0.15f;
            Texture2D farBackdrop = Request<Texture2D>(AssetDirectory.Glassweaver + "FarBackdrop").Value;
            Texture2D farBackdropGlow = Request<Texture2D>(AssetDirectory.Glassweaver + "FarBackdropGlow").Value;

            Texture2D backdropBlack = Request<Texture2D>(AssetDirectory.Glassweaver + "BackdropBlack").Value;

            var frame = new Rectangle(0, 0, backdrop.Width, backdrop.Height);


            LightingBufferRenderer.DrawWithLighting(pos, backdropBlack, frame);

            LightingBufferRenderer.DrawWithLighting(pos + parallaxOffset, farBackdrop, frame);
            //spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

            LightingBufferRenderer.DrawWithLighting(pos, backdrop, frame);
            //spriteBatch.Draw(backdropGlow, pos, frame, Color.White);

        }
    }
}