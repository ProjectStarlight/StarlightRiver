﻿using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
	public class WindTotemArtifactItem : ModItem
	{
		public override string Texture => AssetDirectory.Archaeology + "WindTotemArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Wind Totem");
			Tooltip.SetDefault("Summons or banishes a great wind");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
			Item.autoReuse = false;
			Item.consumable = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = Item.useAnimation = 50;
			Item.UseSound = SoundID.DD2_BookStaffCast;
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
			Tooltip.SetDefault("Summons or banishes a great storm");
		}

		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
			Item.autoReuse = false;
			Item.consumable = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = Item.useAnimation = 50;
			Item.UseSound = SoundID.Item20;
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