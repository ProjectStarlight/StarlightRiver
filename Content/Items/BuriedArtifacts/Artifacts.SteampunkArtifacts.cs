using System;
using Terraria.Utilities;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public abstract class SteampunkArtifactItem : ModItem
	{
		public string name;

		public string description;
		public SteampunkArtifactItem(string _name, string _description)
		{
			name = _name;
			description = _description;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(name);
			Tooltip.SetDefault("Can be used at the extractinator \n" + description);
			ItemID.Sets.ExtractinatorMode[Item.type] = Item.type;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(0,0,25,0);
			Item.rare = ItemRarityID.Blue;
		}

		public override void ExtractinatorUse(ref int resultType, ref int resultStack)
		{
			resultType = ModContent.ItemType<Items.SteampunkSet.AncientGear>();
			resultStack = Main.rand.Next(1, 4);
		}
	}
	public class CopperCogArtifactItem : SteampunkArtifactItem
	{
		public override string Texture => AssetDirectory.Archaeology + "CopperCogArtifact";
		public CopperCogArtifactItem() : base("Copper Cog", "'Our editors work hard to keep our work polished!'") { }
	}

	public class ImportantScrewArtifactItem : SteampunkArtifactItem
	{
		public override string Texture => AssetDirectory.Archaeology + "ImportantScrewArtifact";
		public ImportantScrewArtifactItem() : base("Important Screw", "'No machine would be complete (Or work...) Without it!'") { }
	}

	public class SuspiciouslyStrangeBrewArtifactItem : SteampunkArtifactItem
	{
		public override string Texture => AssetDirectory.Archaeology + "SuspiciouslyStrangeBrewArtifact";
		public SuspiciouslyStrangeBrewArtifactItem() : base("Suspiciously Strange Brew", "'1,500 standard proof!'") { }
	}

	public class InconspicuousPlatingArtifactItem : SteampunkArtifactItem
	{
		public override string Texture => AssetDirectory.Archaeology + "InconspicuousPlatingArtifact";
		public InconspicuousPlatingArtifactItem() : base("Inconspicuous Plating", "'Stronger than diamonds! Kinda...'") { }
	}
}