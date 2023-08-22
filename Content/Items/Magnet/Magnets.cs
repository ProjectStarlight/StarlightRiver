﻿using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Magnet
{
	class UnchargedMagnet : ModItem
	{
		public override string Texture => AssetDirectory.MagnetItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Uncharged Magnet");
			Tooltip.SetDefault("Charged enemies are attracted with this in your inventory\nCharged enemies are stronger than normal, but charge this item on death");
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 5, 0);
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 9999;
		}
	}

	class ChargedMagnet : ModItem
	{
		public override string Texture => AssetDirectory.MagnetItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Charged Magnet");
			Tooltip.SetDefault("'Pulsing with magnetic power'");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 7));
		}

		public override void SetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 15, 0);
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 9999;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			if (Item.lavaWet)
			{
				int i = Item.NewItem(Item.GetSource_Misc("Transform"), Item.getRect(), ModContent.ItemType<GrayGoo>()); // may need syncing idk

				Item item = Main.item[i];
				item.velocity += new Vector2(0f, -1.5f);
				Item.TurnToAir();
			}
		}
	}
}