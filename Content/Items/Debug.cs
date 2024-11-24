using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Bosses.BrainRedux;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Items.UndergroundTemple;
using StarlightRiver.Content.Items.Vitric;
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

	class BrainSpawner : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thinker Test");
			Tooltip.SetDefault("Equips you with gear and spawns the thinker");
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
		}

		public override bool? UseItem(Player player)
		{
			foreach (var item in player.inventory)
			{
				item.TurnToAir();
			}

			foreach(var item in player.armor)
			{
				item.TurnToAir();
			}

			foreach(var npc in Main.npc)
			{
				npc.active = false;
			}

			player.armor[0].SetDefaults(ItemID.MoltenHelmet);
			player.armor[1].SetDefaults(ItemID.MoltenBreastplate);
			player.armor[2].SetDefaults(ItemID.MoltenGreaves);

			player.armor[3].SetDefaults(ItemID.FrostsparkBoots);
			player.armor[4].SetDefaults(ItemID.CreativeWings);
			player.armor[5].SetDefaults(ItemID.ObsidianShield);
			player.armor[6].SetDefaults(ModContent.ItemType<WardedMail>());
			player.armor[7].SetDefaults(ModContent.ItemType<TempleRune>());

			player.inventory[0].SetDefaults(ItemID.Minishark);
			player.inventory[1].SetDefaults(ItemID.DemonScythe);
			player.inventory[2].SetDefaults(ModContent.ItemType<CoachGunUpgrade>());
			player.inventory[3].SetDefaults(ModContent.ItemType<ThousandthDegree>());
			player.inventory[9].SetDefaults(Item.type);
			player.inventory[10].SetDefaults(ItemID.HealingPotion);
			player.inventory[10].stack = 9999;
			player.inventory[11].SetDefaults(ItemID.ManaPotion);
			player.inventory[11].stack = 9999;
			player.inventory[54].SetDefaults(ItemID.EndlessMusketPouch);

			player.statLifeMax = 400;
			player.statManaMax = 200;

			player.statLife = player.statLifeMax;
			player.statMana = player.statManaMax;

			player.Center = new Vector2(Main.maxTilesX * 8, Main.maxTilesY * 8);

			NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 200, ModContent.NPCType<TheThinker>());
			NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y - 200, ModContent.NPCType<DeadBrain>());

			return true;
		}
	}

}