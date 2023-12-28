using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Events;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.ArmatureSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items
{
	[SLRDebug]
	class DebugStick : ModItem
	{
		public Arm arm = new(Main.LocalPlayer.Center, 5, 110, ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/DebugArm").Value);

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
			return true;
		}

		public override void HoldItem(Player player)
		{
			arm.start = player.Center;
			arm.IKToPoint(Main.MouseWorld);
			arm.Update();
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			arm.DrawArm(spriteBatch);
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