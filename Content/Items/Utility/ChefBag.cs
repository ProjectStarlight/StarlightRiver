using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Utility
{
	class ChefBag : ModItem
    {
        public static List<int> ingredientTypes = new List<int>();

        public List<Item> Items = new List<Item>();

        public override bool CloneNewInstances => true;

        public override string Texture => "StarlightRiver/Assets/Items/Utility/ArmorBag";

        public override bool CanRightClick() => true;

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

        public override void RightClick(Player Player)
        {
            UpdateBagSlots();

            Item.stack++;
            ChefBagUI.openBag = this;
            UILoader.GetUIState<ChefBagUI>().OnInitialize();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            for (int k = 0; k < Items.Count; k++)
            {
                var a = new TooltipLine(Mod, "test", Items[k].Name);
                a.overrideColor = ItemRarity.GetColor(Items[k].rare);
                tooltips.Add(a);
            }
        }

        private void UpdateBagSlots()
        {
            Items.Clear();

            ingredientTypes.Sort((n, t) => SortIngredient(n, t));

            for (int k = 0; k < ingredientTypes.Count; k++)
            {
                if (Items.Count <= k || Items[k].type != ingredientTypes[k])
                {
                    Item Item = new Item();
                    Item.SetDefaults(ingredientTypes[k]);
                    Items.Insert(k, Item);
                }
            }
        }

        private int SortIngredient(int n, int t)
        {
            Item temp = new Item();
            temp.SetDefaults(n);
            int x = temp.rare;
            temp.SetDefaults(t);
            int y = temp.rare;

            return x > y ? 1 : -1;
        }

        public override void SaveData(TagCompound tag)
        {
            return new TagCompound()
            {
                ["Items"] = Items
            };
        }

        public override void LoadData(TagCompound tag)
        {
            Items = (List<Item>)tag.GetList<Item>("Items");
            UpdateBagSlots();
        }
    }
}
