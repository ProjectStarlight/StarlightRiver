using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems;
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
		public override int DummyType => DummySystem.DummyType<VitricOreDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.RandomStyleRange = 3;
			TileObjectData.newTile.StyleHorizontal = true;
			MinPick = int.MaxValue;
			Main.tileOreFinderPriority[Type] = 490;//just below chests
			TileID.Sets.Ore[Type] = true;
			Main.tileSpelunker[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;

			var bottomAnchor = new AnchorData(Terraria.Enums.AnchorType.SolidTile, 2, 0);
			this.QuickSetFurniture(2, 3, DustType<GlassGravity>(), SoundID.Shatter, new Color(200, 255, 230), 18, false, false, "Vitric Ore", bottomAnchor);
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.WindsHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
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

		public override bool CanDrop(int i, int j)
		{
			return false;
		}
	}

	internal class VitricOreFloat : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<VitricOreFloatDummy>();

		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 2, DustType<GlassGravity>(), SoundID.Shatter, false, new Color(200, 255, 230), false, false, "Vitric Ore");
			MinPick = int.MaxValue;
			TileID.Sets.Ore[Type] = true;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.WindsHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<Items.Vitric.VitricOre>(), 6);

			// Sync the drop for multiPlayer
			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}

		public override bool CanDrop(int i, int j)
		{
			return false;
		}
	}

	internal class VitricOreDummy : Dummy
	{
		public override bool DoesCollision => true;

		public string Texture => AssetDirectory.VitricTile + "VitricOreGlow";

		public VitricOreDummy() : base(TileType<VitricOre>(), 32, 48) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Hitbox))
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					WorldGen.KillTile((int)(position.X / 16f), (int)(position.Y / 16f));
					NetMessage.SendTileSquare(Player.whoAmI, (int)(position.X / 16f), (int)(position.Y / 16f), 2, 3, TileChangeType.None);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);

				for (int k = 0; k <= 10; k++)
				{
					Dust.NewDustPerfect(Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
					Dust.NewDustPerfect(Center, DustType<Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			int texNum = 1 + Parent.TileFrameX / 36;
			Texture2D tex = Request<Texture2D>(Texture + texNum).Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Center);

			var offset = new Vector2(-1, -1);

			if (texNum > 1)
				offset.Y -= 2;

			Main.spriteBatch.Draw(tex, position + offset - Main.screenPosition, color);
		}
	}

	internal class VitricOreFloatDummy : Dummy
	{
		public override bool DoesCollision => true;

		public string Texture => AssetDirectory.VitricTile + "VitricOreFloatGlow";

		public VitricOreFloatDummy() : base(TileType<VitricOreFloat>(), 32, 32) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Hitbox))
			{
				if (Main.myPlayer == Player.whoAmI)
				{
					WorldGen.KillTile((int)(position.X / 16f), (int)(position.Y / 16f));
					NetMessage.SendTileSquare(Player.whoAmI, (int)(position.X / 16f), (int)(position.Y / 16f), 2, 2, TileChangeType.None);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);

				for (int k = 0; k <= 10; k++)
				{
					Dust.NewDustPerfect(Center, DustType<GlassGravity>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 1.3f);
					Dust.NewDustPerfect(Center, DustType<Air>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(2), 0, default, 0.8f);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Assets.Tiles.Vitric.VitricOreFloatGlow.Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Center);

			Main.spriteBatch.Draw(tex, position - new Vector2(1, 1) - Main.screenPosition, color);
		}
	}

	[SLRDebug]
	class VitricOreItem : QuickTileItem
	{
		public VitricOreItem() : base("Vitric Ore Crystal Item", "", "VitricOre", 1, AssetDirectory.VitricTile, false) { }
	}

	[SLRDebug]
	class VitricOreFloatItem : QuickTileItem
	{
		public VitricOreFloatItem() : base("Floating Vitric Ore Crystal Item", "", "VitricOreFloat", 1, AssetDirectory.VitricTile, false) { }
	}
}