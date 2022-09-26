using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Utility
{
	class RuneOfUndoing : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Rune of Undoing");
			Tooltip.SetDefault("Place over an equipped cursed Item to take it off \nThis consumes the rune");
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
			if (Player.armor[slot].ModItem is CursedAccessory && slot <= (Main.masterMode ? 9 : 8) + Player.extraAccessorySlots && Main.mouseLeft)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCHit55);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item123);
				Main.mouseItem = Player.armor[slot].Clone();
				Player.armor[slot].TurnToAir();
				Item.TurnToAir();
			}

			return false;
		}
	}
}
