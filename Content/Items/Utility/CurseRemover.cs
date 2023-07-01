﻿using StarlightRiver.Content.Items.BaseTypes;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Utility
{
	class CurseRemover : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scroll of Undoing");
			Tooltip.SetDefault("Place over an equipped Cursed item to destroy it\n'There's no turning back, most of the time'");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.LightRed;
			Item.accessory = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Remove(tooltips.FirstOrDefault(n => n.Mod == "Terraria" && n.Name == "Equipable"));
		}

		public override bool CanEquipAccessory(Player Player, int slot, bool modded)
		{
			if (Player.armor[slot].ModItem is CursedAccessory && slot <= (Main.masterMode ? 9 : 8) + Player.extraAccessorySlots)
			{
				(Player.armor[slot].ModItem as CursedAccessory).GoingBoom = true;
				Item.TurnToAir();
			}

			return false;
		}
	}
}