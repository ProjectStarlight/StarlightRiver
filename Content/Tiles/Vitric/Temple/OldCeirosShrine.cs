﻿using StarlightRiver.Content.Biomes;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class OldCeirosShrine : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<OldCeirosShrineDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
			Main.tileLighted[Type] = true;

			this.QuickSetFurniture(4, 3, DustID.Stone, SoundID.Tink, false, new Color(217, 193, 154));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				(r, g, b) = (0.8f, 0.6f, 0.4f);
		}

		public override bool SpawnConditions(int i, int j)
		{
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.TileFrameX < 16 && tile.TileFrameY == 0;
		}

		public override bool RightClick(int i, int j)
		{
			if (Main.LocalPlayer.HeldItem.type == ItemType<Items.DebugStick>())
			{
				Tile tile = Framing.GetTileSafely(i, j);

				if (tile.TileFrameX < 16 && tile.TileFrameY == 0)
				{
					tile.TileFrameX++;
					tile.TileFrameX %= 4;
				}

				return true;
			}

			Tile tile2 = Framing.GetTileSafely(i, j);
			while (tile2.TileFrameX >= 16)
			{
				i--;
				tile2 = Framing.GetTileSafely(i, j);
			}

			while (tile2.TileFrameY > 0)
			{
				j--;
				tile2 = Framing.GetTileSafely(i, j);
			}

			SoundPuzzle.SoundPuzzleHandler.lastTries.Add(tile2.TileFrameX);
			Helpers.Helper.PlayPitched("GlassMiniboss/GlassBounce", 1f, -1f + tile2.TileFrameX * 0.5f, new Vector2(i, j) * 16);
			(GetDummy<OldCeirosShrineDummy>(i, j) as OldCeirosShrineDummy).echoTimer = 30;

			return false;
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Framing.GetTileSafely(i, j);

			if (tile.TileFrameX < 16 && tile.TileFrameY == 0)
			{
				Texture2D tileTex = TextureAssets.Tile[tile.TileType].Value;
				spriteBatch.Draw(tileTex, (new Vector2(i, j) + Helpers.Helper.TileAdj) * 16 - Main.screenPosition, new Rectangle(0, 0, 16, 16), Lighting.GetColor(i, j));

				return false;
			}

			return true;
		}
	}

	class OldCeirosShrineDummy : Dummy, IDrawAdditive
	{
		public int echoTimer = 0;

		public OldCeirosShrineDummy() : base(TileType<OldCeirosShrine>(), 16, 16) { }

		public override void Update()
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			if (echoTimer > 0)
				echoTimer--;

			if (Main.rand.NextBool(20))
			{
				Vector2 pos = position + Vector2.UnitX * Main.rand.NextFloat(64);
				Dust.NewDustPerfect(pos, DustType<Dusts.Aurora>(), Vector2.UnitY * Main.rand.NextFloat(-4, -1), 0, new Color(255, Main.rand.Next(150, 255), 50), Main.rand.NextFloat(0.5f, 1f));
			}
		}

		public override void PostDraw(Color lightColor)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "OldCeirosOrnament" + Parent.TileFrameX).Value;
			float sin = (float)Math.Sin(Main.GameUpdateCount / 30f);
			Vector2 pos = position - Main.screenPosition + new Vector2(32, -32 + sin * 4);

			Main.spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);

			Main.spriteBatch.Draw(tex, pos, null, Color.White * (echoTimer / 30f), 0, tex.Size() / 2, 1 + (2 - echoTimer / 30f * 2), 0, 0);
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D texGlow = Assets.Bosses.VitricBoss.LongGlow.Value;
			Vector2 pos = position - Main.screenPosition + new Vector2(33, 10);

			float sin = (float)Math.Sin(Main.GameUpdateCount / 18f);

			spriteBatch.Draw(texGlow, pos + Vector2.UnitY * 2, null, new Color(255, 180, 100) * (0.5f + sin * 0.1f), 0, new Vector2(texGlow.Width / 2, texGlow.Height), 0.32f, 0, 0);
			spriteBatch.Draw(texGlow, pos, null, new Color(255, 255, 100) * (0.8f + sin * 0.2f), 0, new Vector2(texGlow.Width / 2, texGlow.Height), 0.12f, 0, 0);
		}
	}

	[SLRDebug]
	class OldCeirosShrineItem : QuickTileItem
	{
		public OldCeirosShrineItem() : base("Old Ceiros Shrine", "{{Debug}} Item", "OldCeirosShrine", 0) { }
	}
}