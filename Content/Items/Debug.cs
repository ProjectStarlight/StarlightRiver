using ReLogic.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Content.NPCs.Starlight;
using StarlightRiver.Content.Tiles.BaseTypes;
using StarlightRiver.Content.Tiles.Starlight;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.ArmatureSystem;
using StarlightRiver.Core.Systems.CutsceneSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Core.Systems.LightingSystem;
using Terraria.ID;
using Terraria.WorldBuilding;

namespace StarlightRiver.Content.Items
{
	[SLRDebug]
	class DebugStick : ModItem
	{
		readonly Arm arm = new(Vector2.Zero, 5, 64, Assets.Invisible.Value);

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
			ObservatoryBiome.fade = 1f;
			return;

			player.GetHandler().StaminaMaxBonus = 10;

			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}
	}

	[SLRDebug]
	class DebugStick2 : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Stick 2");
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
			//player.GetHandler().StaminaMaxBonus = 1000;

			//int x = StarlightWorld.vitricBiome.X - 37;

			//Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

			Lighting.AddLight(Main.MouseWorld, new Vector3(0, 1, 0));

		}

		public override bool? UseItem(Player player)
		{
			//ModContent.GetInstance<StarlightWorld>().GraymatterGen(new GenerationProgress(), null);
			return true;

			ObservatorySystem.pylonAppearsOn = false;
			ObservatorySystem.observatoryOpen = false;

			player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().Shards.Clear();
			player.GetHandler().InfusionLimit = 0;

			AlicanSafetySystem.DebugForceState(0);

			if (player.controlDown)
				player.ActivateCutscene<StarlightPylonActivateCutscene>();

			//StarlightWorld.FlipFlag(WorldFlags.ThinkerBossOpen);
			//ModContent.GetInstance<StarlightWorld>().GraymatterGen(new GenerationProgress(), null);

			/*SplineGlow.Spawn(player.Center, Vector2.Lerp(player.Center, Main.MouseWorld, 0.5f) + Vector2.UnitX.RotatedByRandom(6.28f) * 50, Main.MouseWorld, 120, 1, Color.Teal);

			Tile tile2 = Framing.GetTileSafely((int)(Main.MouseWorld.X / 16), (int)(Main.MouseWorld.Y / 16));
			Main.NewText(tile2.TileType + " " + tile2.WallType);

			for (int x = -100; x < 100; x++)
			{
				for (int y = -100; y < 100; y++)
				{
					Tile tile = Framing.GetTileSafely(x + (int)(Main.MouseWorld.X / 16), y + (int)(Main.MouseWorld.Y / 16));
					//tile.IsActuated = false;

					if (tile.TileType == 922)
						tile.ClearEverything();
				}
			}*/

			//Point16 target = new Point16((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
			//Main.NewText($"Deviation at {target} along width 10: {WorldGenHelper.GetElevationDeviation(target, 10, 20, 10, true)}");

			ModContent.GetInstance<StarlightWorld>().ObservatoryGen(null, null);

			//Main.LocalPlayer.GetHandler().Shards.Clear();
			//CagePuzzleSystem.solved = false;

			//WorldGen.KillTile((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);

			return true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("{{Inoculation}} is a keyword\n{{Barrier}} is too\nHere is a really long line so that we can test the wrapping logic of the tooltip panels! I hope this is long enough.\n{{BUFF:OnFire}} {{BUFF:PlexusChaliceBuff}} {{BUFF:BarbedKnifeBleed}}");
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

	[SLRDebug]
	class SuperBreaker : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Super Breaker");
			Tooltip.SetDefault("Breaks anything!");
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
			Item.rare = ItemRarityID.Master;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item18;
			Item.useTurn = true;
		}

		public override bool? UseItem(Player player)
		{
			WorldGen.KillTile((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16);
			return true;
		}
	}
}