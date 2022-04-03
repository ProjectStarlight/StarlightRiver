using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Utility
{
	class CurseRemover : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scroll of Undoing");
			Tooltip.SetDefault("Place over an equipped cursed Item to destroy it\n'There's no turning back, most of the time'");
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

		public override bool CanEquipAccessory(Player Player, int slot)
		{
			if (Player.armor[slot].ModItem is CursedAccessory && slot <= 7 + Player.extraAccessorySlots)
			{
				(Player.armor[slot].ModItem as CursedAccessory).GoingBoom = true;

				Item.TurnToAir();
			}

			return false;
		}
	}
}
