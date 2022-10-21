using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Underground
{
	class HotspringFountain : DummyTile
	{
		public override int DummyType => ModContent.ProjectileType<HotspringFountainDummy>();

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/HotspringFountain";

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.DrawYOffset = 2;
			var anchor = new AnchorData(AnchorType.SolidTile, 5, 0);
			QuickBlock.QuickSetFurniture(this, 5, 5, DustID.Stone, SoundID.Tink, false, new Color(100, 100, 100), bottomAnchor: anchor);
			AnimationFrameHeight = 18 * 5;
		}

		public override void NumDust(int i, int j, bool fail, ref int num)
		{
			num = 1;
		}

		public override void AnimateTile(ref int frame, ref int frameCounter)
		{
			if (++frameCounter >= 5)
			{
				frameCounter = 0;
				frame++;

				if (frame > 3)
					frame = 0;
			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ModContent.ItemType<HotspringFountainItem>());
		}
	}

	class HotspringFountainItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/HotspringFountainItem";

		public HotspringFountainItem() : base("Hotspring Source", "Transforms nearby water into a hotspring!", "HotspringFountain") { }
	}

	class HotspringFountainDummy : Dummy
	{
		public static bool AnyOnscreen => Main.projectile.Any(n => n.active && n.type == ModContent.ProjectileType<HotspringFountainDummy>() && Vector2.Distance(n.Center, Main.screenPosition + Helpers.Helper.ScreenSize / 2) < 1000);

		public HotspringFountainDummy() : base(ModContent.TileType<HotspringFountain>(), 5 * 16, 5 * 16) { }

		public override void Update()
		{
			Lighting.AddLight(Projectile.Center, new Vector3(150, 220, 230) * 0.002f);

			if (Main.rand.NextBool(10))
			{
				Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY * -20, ModContent.DustType<Dusts.Mist>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), Main.rand.Next(50, 70), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
			}

			foreach (Player player in Main.player.Where(n => n.wet && Vector2.Distance(n.Center, Projectile.Center) < 30 * 16))
				player.AddBuff(ModContent.BuffType<HotspringHeal>(), 10);

			for (int x = -30; x < 30; x += 1)
			{
				for (int y = -30; y < 30; y += 1)
				{
					if (new Vector2(x, y).Length() > 30)
						continue;

					int checkX1 = (int)(Projectile.Center.X / 16 + x);
					int checkY1 = (int)(Projectile.Center.Y / 16 + y);
					Tile tile1 = Framing.GetTileSafely(checkX1, checkY1);
					Tile tile2 = Framing.GetTileSafely(checkX1, checkY1 - 1);

					if (!(tile1.HasTile && Main.tileSolid[tile1.TileType]) && tile1.LiquidType == LiquidID.Water && tile1.LiquidAmount > 0)
					{
						if (tile2.LiquidAmount == 0)
						{
							Lighting.AddLight(new Vector2(checkX1 * 16, checkY1 * 16), new Vector3(150, 220, 230) * 0.002f);

							if (Main.rand.Next(40) == 0 && !(tile2.HasTile && Main.tileSolid[tile2.TileType]))//not sure if correct way to check solid
							{
								Vector2 pos = Projectile.Center + new Vector2(x, y - 1) * 16 + Vector2.UnitX * Main.rand.NextFloat(16);
								Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.Mist>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), Main.rand.Next(50, 70), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
							}
						}

						if (Main.rand.Next(600) == 0)
						{
							Vector2 pos = Projectile.Center + new Vector2(x, y) * 16 + Vector2.UnitX * Main.rand.NextFloat(16);
							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.SpringBubble>(), Vector2.UnitY * -Main.rand.NextFloat(0.5f, 1.2f), Main.rand.Next(40, 55), new Color(230, 255, 255), Main.rand.NextFloat(0.3f, 0.4f));
						}
					}
				}
			}

			float angle = Main.rand.NextFloat(6.28f);

			int x1 = (int)(Math.Sin(angle) * Main.rand.Next(30));
			int y1 = (int)(Math.Cos(angle) * Main.rand.Next(30));

			int checkX = (int)(Projectile.Center.X / 16 + x1);
			int checkY = (int)(Projectile.Center.Y / 16 + y1);
			Tile tile = Framing.GetTileSafely(checkX, checkY);

			if (tile.LiquidType == LiquidID.Water && tile.LiquidAmount > 0)
			{
				var d = Dust.NewDustPerfect(Projectile.Center + new Vector2(x1, y1) * 16 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(16), ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(150, 255, 255) * 0.3f, 1);
				d.customData = Main.rand.NextFloat(0.6f, 0.9f);
			}
		}

		public void DrawMap(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0, tex.Size() / 2, scale: 18f, 0, 0);
		}
	}
}
