using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric
{
	internal class VitricOre : DummyTile
	{
		public override int DummyType => ProjectileType<VitricOreDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;
			MinPick = int.MaxValue;
			TileID.Sets.Ore[Type] = true;

			var bottomAnchor = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
			this.QuickSetFurniture(2, 3, DustType<Air>(), SoundID.Shatter, new Color(200, 255, 230), 18, false, false, "Vitric Ore", bottomAnchor);
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 12);

			// Sync the drop for multiPlayer
			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}

		public override void SafeNearbyEffects(int i, int j, bool closer)
		{
			if (Main.rand.NextBool(50))
			{
				var pos = new Vector2(i * 16 + Main.rand.Next(16), j * 16 + Main.rand.Next(16));

				if (Main.rand.NextBool())
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle>(), Vector2.Zero);
				else
					Dust.NewDustPerfect(pos, DustType<CrystalSparkle2>(), Vector2.Zero);
			}

			base.SafeNearbyEffects(i, j, closer);
		}

		public override bool SpawnConditions(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			return tile.TileFrameY == 0 && tile.TileFrameX % 36 == 0;
		}
	}

	internal class VitricOreFloat : DummyTile
	{
		public override int DummyType => ProjectileType<VitricOreFloatDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 2, DustType<Air>(), SoundID.Shatter, false, new Color(200, 255, 230), false, false, "Vitric Ore");
			MinPick = int.MaxValue;
			TileID.Sets.Ore[Type] = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 6);

			// Sync the drop for multiPlayer
			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}
	}

	internal class VitricOreDummy : Dummy
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricOreGlow";

		public VitricOreDummy() : base(TileType<VitricOre>(), 32, 48) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					WorldGen.KillTile((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f));
					NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 3, TileChangeType.None);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
				}

				for (int k = 0; k <= 10; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
					Dust.NewDustPerfect(Projectile.Center, DustType<Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			int texNum = 1 + Parent.TileFrameX / 36;
			Texture2D tex = Request<Texture2D>(Texture + texNum).Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);

			var offset = new Vector2(-1, -1);

			if (texNum > 1)
				offset.Y -= 2;

			Main.spriteBatch.Draw(tex, Projectile.position + offset - Main.screenPosition, color);
		}
	}

	internal class VitricOreFloatDummy : Dummy
	{
		public override string Texture => AssetDirectory.VitricTile + "VitricOreFloatGlow";

		public VitricOreFloatDummy() : base(TileType<VitricOreFloat>(), 32, 32) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					WorldGen.KillTile((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f));
					NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 2, TileChangeType.None);
				}
				else
				{
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
				}

				for (int k = 0; k <= 10; k++)
				{
					Dust.NewDustPerfect(Projectile.Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
					Dust.NewDustPerfect(Projectile.Center, DustType<Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Request<Texture2D>(AssetDirectory.VitricTile + "VitricOreFloatGlow").Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Projectile.Center);

			Main.spriteBatch.Draw(tex, Projectile.position - new Vector2(1, 5) - Main.screenPosition, color);
		}
	}

	class VitricOreItem : QuickTileItem
	{
		public VitricOreItem() : base("Vitric Ore Crystal Item", "", "VitricOre", 1, AssetDirectory.VitricTile, false) { }
	}

	class VitricOreFloatItem : QuickTileItem
	{
		public VitricOreFloatItem() : base("Floating Vitric Ore Crystal Item", "", "VitricOreFloat", 1, AssetDirectory.VitricTile, false) { }
	}
}