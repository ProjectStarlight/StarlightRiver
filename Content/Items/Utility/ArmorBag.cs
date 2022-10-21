using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Utility
{
	class ArmorBag : ModItem
	{
		public Item[] storedArmor = new Item[3];

		public override bool CanRightClick()
		{
			return true;
		}

		public override string Texture => "StarlightRiver/Assets/Items/Utility/ArmorBag";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Armor Bag");
			Tooltip.SetDefault("Stores armor for quick use\nContains:");
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Blue;
			Item.value = 50000;
		}

		public override ModItem Clone(Item item)
		{
			var newBag = (ArmorBag)MemberwiseClone();

			newBag.storedArmor = new Item[3];

			for (int k = 0; k < 3; k++)
			{
				newBag.storedArmor[k] = storedArmor[k]?.Clone();
			}

			if (newBag.storedArmor[0] is null || newBag.storedArmor[1] is null || newBag.storedArmor[2] is null)
			{
				for (int k = 0; k < 3; k++)
				{
					var Item = new Item();
					Item.TurnToAir();
					newBag.storedArmor[k] = Item;
				}
			}

			return newBag;
		}

		public override void RightClick(Player Player)
		{
			Item.stack = 2;

			Item mouseItem = Main.mouseItem;

			if (mouseItem.IsAir)
			{
				if (Player.controlSmart)
				{
					for (int k = 0; k < 3; k++)
					{
						if (storedArmor[k].IsAir)
							continue;

						int index = Item.NewItem(Player.GetSource_ItemUse(Item), Player.Center, storedArmor[k].type);
						Main.item[index] = storedArmor[k].Clone();
						storedArmor[k].TurnToAir();
					}

					return;
				}

				for (int k = 0; k < 3; k++)
				{
					(storedArmor[k], Player.armor[k]) = (Player.armor[k], storedArmor[k]);
				}

				return;
			}

			if (mouseItem.headSlot != -1)
			{
				Item temp = storedArmor[0];
				storedArmor[0] = mouseItem;
				Main.mouseItem = temp;
			}

			if (mouseItem.bodySlot != -1)
			{
				Item temp = storedArmor[1];
				storedArmor[1] = mouseItem;
				Main.mouseItem = temp;
			}

			if (mouseItem.legSlot != -1)
			{
				Item temp = storedArmor[2];
				storedArmor[2] = mouseItem;
				Main.mouseItem = temp;
			}
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			if (storedArmor[0] is null || storedArmor[1] is null || storedArmor[2] is null)
				return;

			var armorLineHead = new TooltipLine(Mod, "HelmetSlot",
				storedArmor[0].IsAir ? "No helmet" : storedArmor[0].Name)
			{
				OverrideColor = storedArmor[0].IsAir ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[0].rare)
			};
			tooltips.Add(armorLineHead);

			var armorLineChest = new TooltipLine(Mod, "ChestSlot",
				storedArmor[1].IsAir ? "No chestplate" : storedArmor[1].Name)
			{
				OverrideColor = storedArmor[1].IsAir ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[1].rare)
			};
			tooltips.Add(armorLineChest);

			var armorLineLegs = new TooltipLine(Mod, "LegsSlot",
				storedArmor[2].IsAir ? "No leggings" : storedArmor[2].Name)
			{
				OverrideColor = storedArmor[2].IsAir ? new Color(150, 150, 150) : ItemRarity.GetColor(storedArmor[2].rare)
			};
			tooltips.Add(armorLineLegs);

			var line = new TooltipLine(Mod, "Starlight",
				"Right click to equip stored armor\n" +
				"Right click with armor to add it to the bag\n" +
				"Ctrl-Right click to empty the bag");

			tooltips.Add(line);
		}

		public override void SaveData(TagCompound tag)
		{
			tag["Head"] = storedArmor[0];
			tag["Chest"] = storedArmor[1];
			tag["Legs"] = storedArmor[2];
		}

		public override void LoadData(TagCompound tag)
		{
			storedArmor[0] = tag.Get<Item>("Head");
			storedArmor[1] = tag.Get<Item>("Chest");
			storedArmor[2] = tag.Get<Item>("Legs");
		}
	}
}
