using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
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
			ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver : 10);
			Item.rare = ItemRarityID.Blue;
			Item.maxStack = 30;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
		}

		public override void ExtractinatorUse(ref int resultType, ref int resultStack)
		{
			int[] gems = new int[]
			{
				ItemID.Topaz,
				ItemID.Amethyst,
				ItemID.Sapphire,
				ItemID.Emerald,
				ItemID.Diamond,
				ItemID.Ruby
			};
			resultStack = Main.rand.Next(7, 15);
			resultType = Main.rand.Next(gems);
		}
	}
}