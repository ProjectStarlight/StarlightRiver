using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Utility;
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

        public override void Load()
        {
            
        }

        public override void AddRecipes() //this is dumb, too bad!
        {
            ChefBag.ingredientTypes.Add(Item.type);
        }

        ///<summary>Where the effects of this food Item's buff will go. use the multiplier param for any effect that should be multiplier-sensitive</summary>
        public virtual void BuffEffects(Player Player, float multiplier) { }

        /// <summary>
        /// Make sure to reset appropriate buff updates here
        /// </summary>
        public virtual void ResetBuffEffects(Player Player, float multiplier) { }

        public virtual void SafeSetDefaults() { }

        //public override bool CloneNewInstances => true; //PORTTODO: Test to make sure commenting this out doesn't mess this up

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("\n\n");
        }

        public override void SetDefaults()
        {
            Item.maxStack = 99;
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 10;
            Item.useAnimation = 10;

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
                if (line.mod == "Terraria" && line.Name == "Tooltip0") { line.text = description; line.overrideColor = nameColor; } //PORTTODO: Replace line.Mod with something
                if (line.mod == "Terraria" && line.Name == "Tooltip1") { line.text = ItemTooltip; line.overrideColor = descriptionColor; }
            }

            TooltipLine fullLine = new TooltipLine(Mod, "StarlightRiver: Fullness", "adds " + Fill / 60 + " seconds duration to food")
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