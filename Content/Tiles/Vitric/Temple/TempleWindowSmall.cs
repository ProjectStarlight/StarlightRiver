using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class TallWindow : DummyTile
	{
		public override int DummyType => ProjectileType<TallWindowDummy>();

		public override string Texture => AssetDirectory.VitricTile + "TallWindow";

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 6, 18, DustType<Dusts.Air>(), SoundID.Shatter, false, Color.Black);
			Main.tileLighted[Type] = true;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			(r, g, b) = (0.5f, 0.35f, 0.2f);
		}
	}

	class TallWindowItem : QuickTileItem
	{
		public TallWindowItem() : base("Window Actor", "Debug Item", "TallWindow", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
	}

	class TallWindowDummy : Dummy
	{
		public TallWindowDummy() : base(TileType<TallWindow>(), 16, 16) { }

		public override void SafeSetDefaults()
		{
			Projectile.hide = true;
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			behindNPCsAndTiles.Add(index);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;
			Vector2 pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;

			var bgTarget = new Rectangle(6, 32, 84, 256);
			bgTarget.Offset(pos.ToPoint());

			TempleTileUtils.DrawBackground(spriteBatch, bgTarget);

			return true;
		}

		public override void PostDraw(Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "TallWindowOver").Value;
			Vector2 pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;

			spriteBatch.End();

			LightingBufferRenderer.DrawWithLighting(pos, tex, Color.White);

			spriteBatch.Begin(default, default, default, default, default, default, Main.Transform);

			//spriteBatch.Draw(tex, pos, new Color(0.5f, 0.35f, 0.2f));         
		}
	}
}