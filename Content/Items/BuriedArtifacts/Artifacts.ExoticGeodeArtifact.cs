using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
    public class ExoticGeodeArtifactItem : ModItem
    {
        public override string Texture => AssetDirectory.Archaeology + "ExoticGeodeArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Exotic Geode");
			Tooltip.SetDefault("'Incredibly shiny'");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver : 10);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 30;
			Item.autoReuse = true;
		}
	}
}