using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Magnet;
using System.Collections.Generic;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Beach
{
	public class ExoticTimepiece : SmartAccessory
	{
		public override string Texture => AssetDirectory.Assets + "Items/Beach/" + Name;

		public ExoticTimepiece() : base("Exotic Timepiece",
			"Your sentries fire faster and shock nearby enemies when placed\n" +
			"Your minions and sentries occasionally deal extra damage\n" +
			"This extra damage counts as a critical strike\n" +
			"200% increased sentry placement speed")
		{ }

		public override List<int> ChildTypes =>
			new()
			{
				ModContent.ItemType<SaltCogs>(),
				ModContent.ItemType<WatchBattery>(),
				ModContent.ItemType<SeaglassLens>()
			};

		public override void Load()
		{
			StarlightItem.UseTimeMultiplierEvent += PlaceSpeed;
			StarlightItem.UseAnimationMultiplierEvent += PlaceAnimation;
		}

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(gold: 3);
		}

		private float PlaceSpeed(Item item, Player player)
		{
			if (Equipped(player) && item.sentry)
				return 0.5f;

			return 1f;
		}

		private float PlaceAnimation(Item item, Player player)
		{
			if (Equipped(player) && item.sentry)
				return 0.5f;

			return 1f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ModContent.ItemType<SaltCogs>());
			recipe.AddIngredient(ModContent.ItemType<WatchBattery>());
			recipe.AddIngredient(ModContent.ItemType<SeaglassLens>());

			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
}
