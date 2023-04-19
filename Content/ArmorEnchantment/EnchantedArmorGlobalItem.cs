﻿using System;
using System.Reflection;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.ArmorEnchantment
{
	class EnchantedArmorGlobalItem : GlobalItem
	{
		public ArmorEnchantment Enchantment { get; set; }

		public override bool InstancePerEntity => true;

		public override void PostDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			Enchantment?.DrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override bool PreDrawInInventory(Item Item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (Enchantment is null)
				return base.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
			return Enchantment.PreDrawInInventory(Item, spriteBatch, position, frame, drawColor, ItemColor, origin, scale);
		}

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem<EnchantedArmorGlobalItem>(out EnchantedArmorGlobalItem gi) ? gi : this;
		}

		public override string IsArmorSet(Item head, Item body, Item legs)
		{
			if (GetEnchant(head) != null && //so unenchanted Items are unaffected
				GetEnchant(body) != null &&
				GetEnchant(legs) != null &&
				GetEnchant(head).Equals(GetEnchant(body)) &&
				GetEnchant(head).Equals(GetEnchant(legs)))
			{
				return "CurrentEnchant";
			}

			return "";
		}

		public override void UpdateArmorSet(Player Player, string set)
		{
			ArmorEnchantment Enchantment = Player.armor[0].GetGlobalItem<EnchantedArmorGlobalItem>().Enchantment;

			if (set == "CurrentEnchant" && Enchantment != null)
			{
				Enchantment.UpdateSet(Player);
			}
		}

		public override void SaveData(Item Item, TagCompound tag)
		{
			if (Enchantment != null)
			{
				tag["EnchantGuid"] = Enchantment.Guid.ToString();
				tag["EnchantType"] = Enchantment.GetType().FullName;
				tag["Enchantment"] = Enchantment.SaveData();
			}
		}

		public override void LoadData(Item Item, TagCompound tag)
		{
			var guid = Guid.ParseExact(tag.GetString("EnchantGuid"), "D");
			string type = tag.GetString("EnchantType");

			if (type != null)
			{
				Type a = Assembly.GetAssembly(GetType()).GetType(type);

				Enchantment = (ArmorEnchantment)Activator.CreateInstance
					(
						a,
						new object[] { guid }
					);

				Enchantment.LoadData(tag.GetCompound("Enchantment"));
			}
		}

		public static ArmorEnchantment GetEnchant(Item Item)
		{
			if (Item.IsAir)
				return null;

			EnchantedArmorGlobalItem globalItem = Item.GetGlobalItem<EnchantedArmorGlobalItem>();

			if (globalItem is null)
				return null;

			return globalItem.Enchantment;
		}
	}
}