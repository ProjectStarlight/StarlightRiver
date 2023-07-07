using Terraria.ID;

namespace StarlightRiver.Content.Items.SpaceEvent
{
	class Astroscrap : ModItem
	{
		public override string Texture => "StarlightRiver/Assets/Items/SpaceEvent/Astroscrap";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Astroscrap");
			Tooltip.SetDefault("‘Alloy salvaged from enigmatic wreckage’");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 5, 0);
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 999;
		}
	}
}