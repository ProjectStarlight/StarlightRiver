using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System;
using StarlightRiver.Content.Items.Food;

namespace StarlightRiver.Content.Items.Utility
{
	class ChefBag : ModItem
    {
        public static List<int> ingredientTypes = new List<int>();

        public List<Item> Items = new List<Item>();

        public override string Texture => "StarlightRiver/Assets/Items/Utility/ArmorBag";

        public override bool CanRightClick() => true;

		public override void Load()
		{
            StarlightItem.OnPickupEvent += SpecialIngredientPickup;
		}

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chef's Bag");
            Tooltip.SetDefault("Stores lots of every ingredient\nRight click an ingredient to make it with others in the bag");
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = ItemRarityID.Orange;
            Item.value = 500000;
        }

		public override ModItem Clone(Item newEntity)
		{
            var clone = base.Clone(newEntity) as ChefBag;  
            clone.Items = new List<Item>();

            return clone;
        }

        private bool SpecialIngredientPickup(Item Item, Player Player)
        {
            var bag = Player.inventory.FirstOrDefault(n => n.type == ModContent.ItemType<ChefBag>());

            if (bag != null)
            {
                if ((bag.ModItem as ChefBag).InsertItem(Item))
                {
                    CombatText.NewText(Player.Hitbox, Microsoft.Xna.Framework.Color.White, "Ingredient added to chefs bag");
                    Helpers.Helper.PlayPitched("Effects/PickupGeneric", 1, 0.5f, Player.Center);
                    return false;
                }
            }

            return true;
        }

        public override void RightClick(Player Player)
        {
            Item.stack++;

            if (!Main.mouseItem.IsAir)
			{
                InsertItem(Main.mouseItem);
                return;
			}

            ChefBagUI.visible = true;
            ChefBagUI.openBag = this;
            UILoader.GetUIState<ChefBagUI>().OnInitialize();
        }

        private int SortIngredient(int n, int t)
        {
            Item temp = new Item();

            temp.SetDefaults(n);
            int x = temp.rare * 3 + (int)(temp.ModItem as Ingredient).ThisType;

            temp.SetDefaults(t);
            int y = temp.rare * 3 + (int)(temp.ModItem as Ingredient).ThisType;

            return x >= y ? 1 : -1;
        }

        public bool InsertItem(Item item)
        {
            if (ingredientTypes.Contains(item.type))
            {
                if (Items.Any(n => n.type == item.type))
                    Items.FirstOrDefault(n => n.type == item.type).stack += item.stack;
                else
                    Items.Add(item.Clone());

                item.TurnToAir();
                return true;
            }

            return false;
        }

        public Item RemoveItem(int type, int amount = -1)
		{
            if(ingredientTypes.Contains(type) && Items.Any(n => n.type == type))
			{
                var storedItem = Items.FirstOrDefault(n => n.type == type);

                if (amount == -1)
                    amount = storedItem.maxStack;

                if (storedItem.stack <= amount)
				{
                    Items.Remove(storedItem);
                    return storedItem.Clone();
				}
                else
				{
                    storedItem.stack -= amount;
                    var item = new Item();
                    item.SetDefaults(type);
                    item.stack = amount;
                    return item;
				}
			}

            return null;
		}

        public override void SaveData(TagCompound tag)
        {
            tag["Items"] = Items;
        }

        public override void LoadData(TagCompound tag)
        {
            Items = (List<Item>)tag.GetList<Item>("Items");
        }
    }
}
