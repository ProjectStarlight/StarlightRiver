using StarlightRiver.Content.Abilities;
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
			int x = StarlightWorld.vitricBiome.X - 37;

			Dust.NewDustPerfect(new Vector2((x + 80) * 16, (StarlightWorld.vitricBiome.Center.Y + 20) * 16), DustID.Firefly);

		}

		public override bool? UseItem(Player player)
		{
			TutorialDataStore store = PersistentDataStoreSystem.GetDataStore<TutorialDataStore>();
			UILoader.GetUIState<MessageBox>().Display("Warning - Master Mode", "Starlight River has unique behavior for its bosses in Master Mode. This behavior is intended to be immensely difficult over anything else, and assumes a high amount of knowldge about " +
	"both the mod and base game. Starlight River Master Mode is not intended for a first playthrough. Starlight River Master Mode is not intended to be fair. Starlight River Master Mode is not intended to be fun for everyone. " +
	"Please remember that the health, both physical and mental, of yourself and those around you is far more important than this game or anything inside of it.");

			UILoader.GetUIState<MessageBox>().AppendButton(Assets.GUI.BackButton, () =>
			{
				store.ignoreMasterWarning = true;
				UILoader.GetUIState<MessageBox>().Visible = false;
			}, "Dont show again");

			//StarlightEventSequenceSystem.sequence = 0;
			//player.GetHandler().unlockedAbilities.Clear();
			player.GetHandler().InfusionLimit = 0;

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
}