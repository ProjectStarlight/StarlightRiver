using StarlightRiver.Core.Systems.AuroraWaterSystem;
using Terraria.Enums;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	public class SquidFins : ModItem
	{
		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Squid Fins");
			Tooltip.SetDefault("Allows you to swim like a jellysquid");
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
			Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 2));
		}

		public override void UpdateEquip(Player player)
		{
			bool canSwim = player.grapCount <= 0 && player.wet && !player.mount.Active;
			player.GetModPlayer<SwimPlayer>().ShouldSwim = canSwim;
			player.GetModPlayer<SwimPlayer>().SwimSpeed = 1.33f + player.moveSpeed * 1.33f;
		}
	}
}
