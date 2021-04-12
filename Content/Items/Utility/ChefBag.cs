using Microsoft.Xna.Framework;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public List<Item> items = new List<Item>();

        public override bool CloneNewInstances => true;

        public override string Texture => "StarlightRiver/Assets/Items/Utility/ArmorBag";

        public override bool CanRightClick() => true;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Chef's Bag");
            Tooltip.SetDefault("Stores a full stack of every ingredient\nRight click an ingredient to make it with others in the bag");
        }

        public override void SetDefaults()
        {
            item.width = 32;
            item.height = 32;
            item.rare = ItemRarityID.Blue;
        }

        public override void RightClick(Player player)
        {
            UpdateBagSlots();

            item.stack++;
            ChefBagUI.openBag = this;
            UILoader.GetUIState<ChefBagUI>().OnInitialize();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            for (int k = 0; k < items.Count; k++)
            {
                var a = new TooltipLine(mod, "test", items[k].Name);
                a.overrideColor = ItemRarity.GetColor(items[k].rare);
                tooltips.Add(a);
            }
        }

        private void UpdateBagSlots()
        {
            items.Clear();

            ingredientTypes.Sort((n, t) => SortIngredient(n, t));

            for (int k = 0; k < ingredientTypes.Count; k++)
            {
                if (items.Count <= k || items[k].type != ingredientTypes[k])
                {
                    Item item = new Item();
                    item.SetDefaults(ingredientTypes[k]);
                    items.Insert(k, item);
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

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["Items"] = items
            };
        }

        public override void Load(TagCompound tag)
        {
            items = (List<Item>)tag.GetList<Item>("Items");
            UpdateBagSlots();
        }
    }
}
