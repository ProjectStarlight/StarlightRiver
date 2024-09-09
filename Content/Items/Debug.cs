using StarlightRiver.Content.Abilities;

using StarlightRiver.Content.Bosses.BrainRedux;

using StarlightRiver.Content.Abilities.ForbiddenWinds;

using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Tiles.Crimson;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.PersistentDataSystem;
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
			//StarlightEventSequenceSystem.sequence = 0;
			//player.GetHandler().unlockedAbilities.Add(typeof(Dash), new Dash());
			//player.GetHandler().InfusionLimit = 1;

			//Main.time = 53999;
			//Main.dayTime = true;
			//StarlightEventSequenceSystem.willOccur = true;

			int k = (int)(Main.MouseWorld.X / 16);
			int y = (int)(Main.MouseWorld.Y / 16);

			WorldGen.TileRunner(k - 2, y, 3, 25, ModContent.TileType<GrayMatter>(), true, 1f, 0, true);

			GrayMatterSpike(k, y);

			if (WorldGen.genRand.NextBool())
				GrayMatterSpike(k + WorldGen.genRand.Next(-2, 3), y);

			k += 30;


			return true;
		}

		private void GrayMatterSpike(int x, int y)
		{
			int maxDown = WorldGen.genRand.Next(3, 7);
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