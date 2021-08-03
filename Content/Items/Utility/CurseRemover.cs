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
			Tooltip.SetDefault("Place over an equipped cursed item to destroy it\n'There's no turning back, most of the time'");
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.maxStack = 1;
			item.rare = ItemRarityID.LightRed;
			item.accessory = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Remove(tooltips.FirstOrDefault(n => n.mod == "Terraria" && n.Name == "Equipable"));
		}

		public override bool CanEquipAccessory(Player player, int slot)
		{
			if (player.armor[slot].modItem is CursedAccessory && slot <= 7 + player.extraAccessorySlots)
			{
				(player.armor[slot].modItem as CursedAccessory).GoingBoom = true;

				item.TurnToAir();
			}

			return false;
		}
	}
}
