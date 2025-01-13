using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Core.Loaders.UILoading;
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
}