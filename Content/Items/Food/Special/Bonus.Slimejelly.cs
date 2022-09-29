﻿using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class Slimejelly : BonusIngredient
	{
		public Slimejelly() : base("Release a friendly slime when damaged\nSlimes become passive") { }

		public override FoodRecipie Recipie() => new FoodRecipie(
			ModContent.ItemType<Slimejelly>(),
			ModContent.ItemType<Gelatine>(),
			ModContent.ItemType<GelBerry>(),
			ModContent.ItemType<StarlightWater>(),
			ModContent.ItemType<Sugar>()
			);

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.slime = true;
		}
	}
}
