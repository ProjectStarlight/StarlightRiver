using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
    public class PirateChestArtifactItem : ModItem
    {
        public override string Texture => AssetDirectory.Archaeology + "PirateChestArtifact";
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Pirate Chest");
			Tooltip.SetDefault("Right click to open\n'Sought after for centuries'");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 30;
			Item.autoReuse = true;
		}

		public override bool CanRightClick() => true;

		public override void RightClick(Player player)
		{
			
		}
	}
}