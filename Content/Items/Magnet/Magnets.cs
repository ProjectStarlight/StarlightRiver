using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Magnet
{
	class UnchargedMagnet : ModItem
	{
		public override string Texture => AssetDirectory.MagnetItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Uncharged Magnet");
			Tooltip.SetDefault("May attract charged enemies");
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
			Item.maxStack = 999;
		}
	}
}