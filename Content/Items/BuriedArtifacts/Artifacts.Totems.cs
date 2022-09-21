using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using System;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class WindTotemArtifactItem : ModItem
	{
		public override string Texture => AssetDirectory.Archaeology + "WindTotemArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wind Totem");
			Tooltip.SetDefault("Starts or stops the force of the great winds");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 30;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = Item.useAnimation = 50;
		}

		public override bool? UseItem(Player player)
		{
			if (Math.Abs(Main.windSpeedTarget) < 0.34f)
				Main.windSpeedTarget = Math.Sign(Main.windSpeedTarget) * 0.5f;
			else if (Main.windSpeedTarget != 0)
				Main.windSpeedTarget = Math.Sign(Main.windSpeedTarget) * 0.05f;
			else
				Main.windSpeedTarget = 0.05f;

			return true;
		}
	}

	public class RainTotemArtifactItem : ModItem
	{
		public override string Texture => AssetDirectory.Archaeology + "RainTotemArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rain Totem");
			Tooltip.SetDefault("Starts or stops the force of the great storms");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver:50);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 30;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = Item.useAnimation = 50;
		}

		public override bool? UseItem(Player player)
		{
			if (Main.raining)
				Main.StopRain();
			else
				Main.StartRain();

			return true;
		}
	}
}