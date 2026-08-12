using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Prefixes.Accessory.Cursed;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Utility
{
	class CurseRemover : ModItem
	{
		public override string Texture => AssetDirectory.Assets + "Items/Utility/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Scroll of Scrying Flames");
			Tooltip.SetDefault("Right click a cursed accessory while holding this on your cursor to destroy it\nGrants you the residual darkness in it's place");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 9999;
			Item.rare = ItemRarityID.LightRed;
		}
	}

	class CurseRemoverApply : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.ModItem is CursedAccessory;
		}

		public override bool CanRightClick(Item item)
		{
			return Main.mouseItem.type == ModContent.ItemType<CurseRemover>();
		}

		public override void RightClick(Item item, Player player)
		{
			if (Main.mouseItem.type == ModContent.ItemType<CurseRemover>())
			{
				if (item.ModItem is CursedAccessory cursed)
				{
					cursed.GoingBoom = true;
					Main.mouseItem.stack--;

					if (Main.mouseItem.stack <= 0)
						Main.mouseItem.TurnToAir();
				}

				item.stack++;
			}
		}
	}
}