﻿using Terraria.ID;

namespace StarlightRiver.Content.Items.Dungeon
{
	class InertStaff : ModItem
	{
		public override string Texture => AssetDirectory.DungeonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Inert Crescent Staff");
			Tooltip.SetDefault("'Still holds some latent lightning...'");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
		}
	}
}