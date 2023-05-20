using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	abstract class TallWindowDummyBase : Dummy
	{
		public virtual string TextureOver => AssetDirectory.VitricTile + "TallWindowOver";

		public TallWindowDummyBase(int type) : base(type, 16, 16) { }

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

			Texture2D tex = Request<Texture2D>(TextureOver).Value;
			Vector2 pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;

			var tilePos = (Projectile.position / 16).ToPoint16();
			RedrawWall(tilePos.X, tilePos.Y + 2);
			RedrawWall(tilePos.X + 1, tilePos.Y + 2);
			RedrawWall(tilePos.X + 4, tilePos.Y + 2);
			RedrawWall(tilePos.X + 5, tilePos.Y + 2);

			spriteBatch.End();

			LightingBufferRenderer.DrawWithLighting(pos, tex, Color.White);

			spriteBatch.Begin(default, default, default, default, default, default, Main.Transform);
		}

		public void RedrawWall(int i, int j)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricTempleWall").Value;
			var target = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y);
			var source = new Rectangle(i % 14 * 16, j % 25 * 16, 16, 16);

			Tile tile = Framing.GetTileSafely(i, j);

			if (Lighting.NotRetro && !WorldGen.SolidTile(tile))
			{
				Color color = Lighting.GetColor(i, j);
				Main.spriteBatch.Draw(tex, target, source, color, 0, Vector2.Zero, 1f, 0, 0);
			}
		}
	}

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
			if (Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				(r, g, b) = (0.5f, 0.35f, 0.2f);
		}
	}

	class TallWindowItem : QuickTileItem
	{
		public TallWindowItem() : base("Window Actor", "Debug Item", "TallWindow", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
	}

	class TallWindowDummy : TallWindowDummyBase
	{
		public TallWindowDummy() : base(TileType<TallWindow>()) { }
	}

	class TallWindowLava : TallWindow
	{
		public override int DummyType => ProjectileType<TallWindowLavaDummy>();
	}

	class TallWindowLavaItem : QuickTileItem
	{
		public TallWindowLavaItem() : base("Lava Window Actor", "Debug Item", "TallWindowLava", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
	}

	class TallWindowLavaDummy : TallWindowDummyBase
	{
		public TallWindowLavaDummy() : base(ModContent.TileType<TallWindowLava>()) { }

		public override string TextureOver => AssetDirectory.VitricTile + "TallWindowOverLava";

		public override void PostDraw(Color lightColor)
		{
			base.PostDraw(lightColor);

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = Request<Texture2D>(TextureOver + "Glow").Value;
			Vector2 pos = Projectile.Center - Main.screenPosition - Vector2.One * 8;
			float sin = 0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + Projectile.position.X * 1 / 16f) * 0.25f;

			spriteBatch.Draw(tex, pos, Color.White * sin);
		}
	}

	class TallWindowCrystal : TallWindow
	{
		public override int DummyType => ProjectileType<TallWindowCrystalDummy>();
	}

	class TallWindowCrystalItem : QuickTileItem
	{
		public TallWindowCrystalItem() : base("Crystal Window Actor", "Debug Item", "TallWindowCrystal", 0, AssetDirectory.VitricTile + "WindsRoomOrnamentLeft", true) { }
	}

	class TallWindowCrystalDummy : TallWindowDummyBase
	{
		public TallWindowCrystalDummy() : base(ModContent.TileType<TallWindowCrystal>()) { }

		public override string TextureOver => AssetDirectory.VitricTile + "TallWindowOverCrystal";
	}
}