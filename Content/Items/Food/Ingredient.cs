using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	public enum IngredientType
    {
        Main = 0,
        Side = 1,
        Seasoning = 2
    };

    public abstract class Ingredient : ModItem
    {
        public string ItemTooltip;
        public int Fill = 0;
        public IngredientType ThisType { get; set; }

        protected Ingredient(string tooltip, int filling, IngredientType type)
        {
            Fill = filling;
            ItemTooltip = tooltip;
            ThisType = type;
        }

        public override string Texture => AssetDirectory.FoodItem + Name;

        public override bool Autoload(ref string name)
        {
            return base.Autoload(ref name);
        }

        ///<summary>Where the effects of this food item's buff will go. use the multiplier param for any effect that should be multiplier-sensitive</summary>
        public virtual void BuffEffects(Player player, float multiplier) { }

        /// <summary>
        /// Make sure to reset appropriate buff updates here
        /// </summary>
        public virtual void ResetBuffEffects(Player player, float multiplier) { }

        public virtual void SafeSetDefaults() { }

        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("\n\n");
        }

        public override void SetDefaults()
        {
            item.maxStack = 99;
            item.width = 32;
            item.height = 32;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = 10;
            item.useAnimation = 10;

            SafeSetDefaults();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string description;
            Color nameColor;
            Color descriptionColor;

            switch (ThisType)
            {
                case IngredientType.Main: description = "Main Course"; nameColor = new Color(255, 220, 140); descriptionColor = new Color(255, 220, 80); break;
                case IngredientType.Side: description = "Side Dish"; nameColor = new Color(140, 255, 140); descriptionColor = new Color(80, 255, 80); break;
                case IngredientType.Seasoning: description = "Seasonings"; nameColor = new Color(140, 200, 255); descriptionColor = new Color(80, 140, 255); break;
                default: description = "ERROR"; nameColor = Color.Black; descriptionColor = Color.Black; break;
            }

            foreach (TooltipLine line in tooltips)
            {
                if (line.mod == "Terraria" && line.Name == "Tooltip0") { line.text = description; line.overrideColor = nameColor; }
                if (line.mod == "Terraria" && line.Name == "Tooltip1") { line.text = ItemTooltip; line.overrideColor = descriptionColor; }
            }

            TooltipLine fullLine = new TooltipLine(mod, "StarlightRiver: Fullness", "adds " + Fill / 60 + " seconds duration to food")
            {
                overrideColor = new Color(110, 235, 255)
            };

            tooltips.Add(fullLine);
        }

        public Color GetColor()
        {
            switch (ThisType)
            {
                case IngredientType.Main: return new Color(255, 220, 140);
                case IngredientType.Side: return new Color(140, 255, 140);
                case IngredientType.Seasoning: return new Color(140, 200, 255);
                default: return Color.Black;
            }
        }
    }
}