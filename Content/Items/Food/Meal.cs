using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class Meal : ModItem
    {
        public List<Item> Ingredients { get; set; } = new List<Item>();
        public int Fullness { get; set; }
        public override bool CloneNewInstances => true;

        public override string Texture => AssetDirectory.FoodItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Meal");
            Tooltip.SetDefault("Rich food that provides these buffs:");
        }

        public override void SetDefaults()
        {
            item.consumable = true;
            item.useAnimation = 30;
            item.useTime = 30;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.width = 32;
            item.height = 32;
        }

        public override bool CanUseItem(Player player)
        {
            FoodBuffHandler mp = player.GetModPlayer<FoodBuffHandler>();

            if (player.HasBuff(BuffType<Full>())) return false;

            if (Ingredients.Count > 0)
            {
                foreach (Item item in Ingredients) mp.Consumed.Add(item.DeepClone());
                player.AddBuff(BuffType<FoodBuff>(), Fullness);
                player.AddBuff(BuffType<Full>(), (int)(Fullness * 1.5f));
            }
            else Main.NewText("Bad food! Please report me to the mod devs.", Color.Red);

            item.stack--;
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string sidesName = "";

            if (Ingredients.Any(n => (n.modItem as Ingredient).ThisType == IngredientType.Side))
            {
                List<Item> sides = Ingredients.FindAll(n => (n.modItem as Ingredient).ThisType == IngredientType.Side);
                sidesName += " with " + sides[0].Name;
                if (sides.Count == 2) sidesName += " and " + sides[1].Name;
            }

            string mainName = "";

            if (Ingredients.Any(n => (n.modItem as Ingredient).ThisType == IngredientType.Main)) mainName = Ingredients.FirstOrDefault(n => (n.modItem as Ingredient).ThisType == IngredientType.Main).Name;

            string fullName = mainName + sidesName;

            tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.mod == "Terraria").text = fullName;

            foreach (Item item in Ingredients.Where(n => n.modItem is Ingredient))
            {
                TooltipLine line = new TooltipLine(mod, "StarlightRiver: Ingredient", (item.modItem as Ingredient).ItemTooltip)
                {
                    overrideColor = (item.modItem as Ingredient).GetColor()
                };
                tooltips.Add(line);
            }

            TooltipLine durationLine = new TooltipLine(mod, "StarlightRiver: Duration", Fullness / 60 + " seconds duration") { overrideColor = new Color(110, 235, 255) };
            tooltips.Add(durationLine);

            TooltipLine cooldownLine = new TooltipLine(mod, "StarlightRiver: Cooldown", (int)(Fullness * 1.5f) / 60 + " seconds fullness") { overrideColor = new Color(255, 170, 120) };
            tooltips.Add(cooldownLine);
        }

        public override TagCompound Save()
        {
            return new TagCompound()
            {
                ["Items"] = Ingredients,
                ["Fullness"] = Fullness
            };
        }

        public override void Load(TagCompound tag)
        {
            Ingredients = (List<Item>)tag.GetList<Item>("Items");
            Fullness = tag.GetInt("Fullness");
        }
    }
}