using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	[SLRDebug]
	class DebugStick : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick");
			Tooltip.SetDefault("Has whatever effects are needed");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 40;
			Item.useTime = 18;

			Item.useAnimation = 18;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5f;
			Item.value = 1000;
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetHandler().StaminaMaxBonus = 1000;

			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}

		public override bool? UseItem(Player player)
		{
			TextCard.Display("Blorgus", "schmungus!", () => Main.LocalPlayer.velocity.Y < 0);
			player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().InfusionLimit = 1;

			//Main.time = 53999;
			//Main.dayTime = true;
			//StarlightEventSequenceSystem.willOccur = true;

			return true;
		}

		private void GrayBlob(int x, int y)
		{
			WorldGen.TileRunner(x - 2, y, 3, 25, ModContent.TileType<GrayMatter>(), true, 1f, 0, true);

			GrayMatterSpike(x, y);

			if (WorldGen.genRand.NextBool())
				GrayMatterSpike(x + WorldGen.genRand.Next(-2, 3), y);

			for (int k = x-5; k < x+5; k++)
			{
				for (int j = y - 10; j < y + 10; j++)
				{
					var tile = Framing.GetTileSafely(k, j);
					if (tile.HasTile && tile.TileType == ModContent.TileType<GrayMatter>() && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(1, 4)))
					{
						if (WorldGen.genRand.NextBool(3))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.Helper.PlaceMultitile(new Point16(k, j - 4), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x4").Type); break;
								case 1: Helpers.Helper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x2").Type); break;
								case 2: Helpers.Helper.PlaceMultitile(new Point16(k, j - 1), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco1x1").Type); break;
							}
							k++;
						}
						else if (WorldGen.genRand.NextBool(3) && Framing.GetTileSafely(k + 1, j).HasTile && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(2, 4)))
						{
							switch (Main.rand.Next(3))
							{
								case 0: Helpers.Helper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x3").Type); break;
								case 1: Helpers.Helper.PlaceMultitile(new Point16(k, j - 2), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco2x2").Type); break;
							}
							k += 2;
						}
						else if (Framing.GetTileSafely(k + 1, j).HasTile && Framing.GetTileSafely(k + 2, j).HasTile && Helpers.Helper.CheckAirRectangle(new Point16(k, j - 4), new Point16(3, 4)))
						{
							Helpers.Helper.PlaceMultitile(new Point16(k, j - 3), StarlightRiver.Instance.Find<ModTile>("GraymatterDeco3x3").Type);
							k += 3;
						}

						continue;
					}
				}
			}
		}

		private void GrayMatterSpike(int x, int y)
		{
			int maxDown = WorldGen.genRand.Next(3, 6);
			for (int down = 0; down < maxDown; down++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				y++;
			}

			int maxSide = WorldGen.genRand.Next(2, 3);
			int dir = WorldGen.genRand.NextBool() ? -1 : 1;
			for (int side = 0; side < maxSide; side++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				x += dir;
			}

			maxDown = WorldGen.genRand.Next(2, 4);
			for (int down = 0; down < maxDown; down++)
			{
				WorldGen.PlaceTile(x, y, ModContent.TileType<GrayMatter>(), true, true);
				y++;
			}
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("Enables {{Debug}} mode");
		}

		public override void SetDefaults()
		{
			Item.damage = 10;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override bool? UseItem(Player Player)
		{
			StarlightRiver.debugMode = !StarlightRiver.debugMode;
			return true;
		}
	}
}