using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using System;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	public abstract class WalkableCrystalItem : QuickTileItem
	{
		private bool held = false;

		public WalkableCrystalItem(string name, string placetype, string texturepath) : base(placetype + "Item", name, "The slot this Item is in changes the type placed", placetype, ItemRarityID.Blue, texturepath) { }

		public override void HoldItem(Player Player)
		{
			held = true;
		}

		public override void UpdateInventory(Player Player)
		{
			held = false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (held)
			{
				var modtile = GetModTile(Item.createTile) as WalkableCrystal;
				float zoom = Main.GameViewMatrix.Zoom.X;
				Vector2 offset = new Vector2((modtile.maxWidth / 2 - 1) * 16, (modtile.maxHeight - 1) * 16 - 1) * zoom;

				spriteBatch.Draw(TextureAssets.Tile[Item.createTile].Value, (Main.MouseWorld / (16 * zoom)).PointAccur() * (16 * zoom) - Main.screenPosition - offset,
					TextureAssets.Tile[Item.createTile].Value.Frame(modtile.variantCount, 1, Main.LocalPlayer.selectedItem, 0),
					Color.White * 0.75f, 0, default, zoom, default, default);
			}
		}

		public override bool CanUseItem(Player Player)
		{
			Item.placeStyle = Player.selectedItem;
			return base.CanUseItem(Player);
		}
	}

	[SLRDebug]
	public class VitricSmallCrystalItem : WalkableCrystalItem
	{
		public VitricSmallCrystalItem() : base("Small vitric crystal", "VitricSmallCrystal", AssetDirectory.VitricTile) { }
	}

	[SLRDebug]
	public class VitricMediumCrystalItem : WalkableCrystalItem
	{
		public VitricMediumCrystalItem() : base("Medium vitric crystal", "VitricMediumCrystal", AssetDirectory.VitricTile) { }
	}

	[SLRDebug]
	public class VitricLargeCrystalItem : WalkableCrystalItem
	{
		public VitricLargeCrystalItem() : base("Large vitric crystal", "VitricLargeCrystal", AssetDirectory.VitricTile) { }
	}

	[SLRDebug]
	public class VitricGiantCrystalItem : WalkableCrystalItem
	{
		public VitricGiantCrystalItem() : base("Vitric Giant crystal", "VitricGiantCrystal", AssetDirectory.VitricTile) { }
	}

	internal abstract class VitricCrystal : WalkableCrystal
	{
		protected VitricCrystal(int maxWidth, int maxHeight, int variantCount = 1, string drop = null) : base(maxWidth, maxHeight, AssetDirectory.VitricTile, AssetDirectory.VitricCrystalStructs, variantCount, drop, DustType<GlassGravity>(), new Color(115, 202, 158), SoundID.Item27)
		{ }

		public override void SafeSetDefaults()
		{
			Main.tileMerge[TileType<VitricSpike>()][Type] = true;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 2;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Texture2D lavaFadeTex = Assets.Tiles.Vitric.VitricLavaFade.Value;

			if (Main.tile[i, j].TileType == Type)
			{
				int val = (int)(Math.Sin(Main.GameUpdateCount * 0.04f + (i + j)) * 15f + 240f);
				var col = new Color(val, val, val, 0);

				Tile sideTile = Main.tile[i - 1, j];
				Tile sideUpTile = Main.tile[i - 1, j - 1];

				if (sideTile.LiquidType == LiquidID.Lava)
				{
					spriteBatch.Draw(lavaFadeTex, (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(0, (255f - sideTile.LiquidAmount) / 16f), null, col, 0, default, new Vector2(val / 255f, sideTile.LiquidAmount / 255f), SpriteEffects.None, 0);
				}
				else if (sideUpTile.LiquidType == LiquidID.Lava && sideTile.TileType != Type)
				{
					spriteBatch.Draw(lavaFadeTex, (new Vector2(i, j) + Helper.TileAdj) * 16 - Main.screenPosition, null, col, 0, default, new Vector2(val / 255f, 1), SpriteEffects.None, 0);
				}
				else
				{
					sideTile = Main.tile[i + 1, j];
					sideUpTile = Main.tile[i + 1, j - 1];

					if (sideTile.LiquidType == LiquidID.Lava)
						spriteBatch.Draw(lavaFadeTex, (new Vector2(i - 2, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(lavaFadeTex.Width, (255f - sideTile.LiquidAmount) / 16f), null, col, 0, new Vector2(lavaFadeTex.Width, 0), new Vector2(val / 255f, sideTile.LiquidAmount / 255f), SpriteEffects.FlipHorizontally, 0);
					else if (sideUpTile.LiquidType == LiquidID.Lava && sideTile.TileType != Type)
						spriteBatch.Draw(lavaFadeTex, (new Vector2(i - 2, j) + Helper.TileAdj) * 16 - Main.screenPosition + new Vector2(lavaFadeTex.Width, 0), null, col, 0, new Vector2(lavaFadeTex.Width, 0), new Vector2(val / 255f, 1), SpriteEffects.FlipHorizontally, 0);
				}
			}
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.NextBool(1500))
			{
				var pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));

				if (Main.rand.NextBool())
					Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle>(), Vector2.Zero);
				else
					Dust.NewDustPerfect(pos, ModContent.DustType<CrystalSparkle2>(), Vector2.Zero);
			}

			ushort slabType = StarlightRiver.Instance.Find<ModTile>("AncientSandstone").Type;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == slabType;
			bool right = Framing.GetTileSafely(i + 1, j).TileType == slabType;
			bool up = Framing.GetTileSafely(i, j - 1).TileType == slabType;
			bool down = Framing.GetTileSafely(i, j + 1).TileType == slabType;

			if (left || right || up || down)
				WorldGen.KillTile(i, j);

			base.SafeNearbyEffects(i, j, closer);
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == Type;
			bool right = Framing.GetTileSafely(i + 1, j).TileType == Type;
			bool up = Framing.GetTileSafely(i, j - 1).TileType == Type;
			bool down = Framing.GetTileSafely(i, j + 1).TileType == Type;

			if (left)
				WorldGen.KillTile(i - 1, j);
			if (right)
				WorldGen.KillTile(i + 1, j);
			if (up)
				WorldGen.KillTile(i, j - 1);
			if (down)
				WorldGen.KillTile(i, j + 1);
		}
	}

	internal class VitricGiantCrystal : VitricCrystal
	{
		public VitricGiantCrystal() : base(10, 19, 4) { }

		public override int DummyType => DummySystem.DummyType<VitricGiantDummy>();
	}

	internal class VitricGiantDummy : WalkableCrystalDummy
	{
		public VitricGiantDummy() : base(TileType<VitricGiantCrystal>(), 4) { }
	}

	internal class VitricLargeCrystal : VitricCrystal
	{
		public VitricLargeCrystal() : base(13, 8, 2) { }

		public override int DummyType => DummySystem.DummyType<VitricLargeDummy>();
	}

	internal class VitricLargeDummy : WalkableCrystalDummy
	{
		public VitricLargeDummy() : base(TileType<VitricLargeCrystal>(), 2) { }
	}

	internal class VitricMediumCrystal : VitricCrystal
	{
		public VitricMediumCrystal() : base(7, 6, 4) { }

		public override int DummyType => DummySystem.DummyType<VitricMediumDummy>();
	}

	internal class VitricMediumDummy : WalkableCrystalDummy
	{
		public VitricMediumDummy() : base(TileType<VitricMediumCrystal>(), 4) { }
	}

	internal class VitricSmallCrystal : VitricCrystal
	{
		public VitricSmallCrystal() : base(3, 3, 2) { }

		public override int DummyType => DummySystem.DummyType<VitricSmallDummy>();
	}

	internal class VitricSmallDummy : WalkableCrystalDummy
	{
		public VitricSmallDummy() : base(TileType<VitricSmallCrystal>(), 2) { }
	}
}