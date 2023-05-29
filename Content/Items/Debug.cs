using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Events;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	class DebugStick : ModItem
	{
		public NPC target;
		public Projectile target2;
		public Player owner;

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

		}

		public override bool? UseItem(Player player)
		{
			StarlightEventSequenceSystem.sequence = 0;
			player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().InfusionLimit = 0;
			return true;
		}
	}

	class DebugModerEnabler : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/DebugStick";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Debug Mode");
			Tooltip.SetDefault("Enables debug mode");
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