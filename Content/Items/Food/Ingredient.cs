using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.Utility;
using StarlightRiver.Core;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
	public enum IngredientType
    {
        Main = 0,
        Side = 1,
        Seasoning = 2,
        Bonus = 3
    };

    public abstract class Ingredient : ModItem
    {
        public string ItemTooltip;
        public int Fill = 0;
        public IngredientType ThisType { get; set; }
        public float FullnessMult = 1;
        public float WellFedMult = 1;

        /// <param name="tooltip">Extra tooltip lines</param>
        /// <param name="filling">How much time this should add to the food, time in seconds is this divided by 60</param>
        protected Ingredient(string tooltip, int filling, IngredientType type, float totalFillMult = 1f, float fullMult = 1)
        {
            Fill = filling;
            ItemTooltip = tooltip;
            ThisType = type;
            FullnessMult = totalFillMult;
            WellFedMult = fullMult;
        }

        public override string Texture => AssetDirectory.FoodItem + Name;

        public override void AddRecipes() //this is dumb, too bad!
        {
            ChefBag.ingredientTypes.Add(Item.type);
        }

        protected bool Active(Player player) => player.GetModPlayer<FoodBuffHandler>().Consumed.Any(n => n.type == Type);

        /// <summary>
        /// Effects which are applied immediately on consumption
        /// </summary>
        /// <param name="player">The player eating the food</param>
        /// <param name="multiplier">The power which should be applied to numeric effects</param>
        public virtual void OnUseEffects(Player player, float multiplier) { }

        /// <summary>
        /// The passive effects of a food item while the buff is active
        /// </summary>
        /// <param name="Player">The palyer eating the food</param>
        /// <param name="multiplier">The power which should be applied to numeric effects</param>
        public virtual void BuffEffects(Player Player, float multiplier) { }

        /// <summary>
        /// Allows you to reset buffs applied in BuffEffects
        /// </summary>
        /// <param name="Player">The palyer eating the food</param>
        /// <param name="multiplier">The power which should be applied to numeric effects</param>
        public virtual void ResetBuffEffects(Player Player, float multiplier) { }

        public virtual void SafeSetDefaults() { }

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
                case IngredientType.Bonus: description = "Bonus Effects"; nameColor = new Color(255, 200, 200); descriptionColor = new Color(255, 140, 140); break;
                default: description = "ERROR"; nameColor = Color.Black; descriptionColor = Color.Black; break;
            }

            foreach (TooltipLine line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name == "Tooltip0") { line.Text = description; line.OverrideColor = nameColor; }
                if (line.Mod == "Terraria" && line.Name == "Tooltip1") { line.Text = ItemTooltip; line.OverrideColor = descriptionColor; }
            }

            if (ThisType != IngredientType.Bonus)
            {
                TooltipLine fullLine = new TooltipLine(Mod, "StarlightRiver: Fullness", "adds " + Fill / 60 + " seconds duration to food")
                {
                    OverrideColor = new Color(110, 235, 255)
                };

                tooltips.Add(fullLine);
            }
        }

        public Color GetColor()
        {
            switch (ThisType)
            {
                case IngredientType.Main: return new Color(255, 220, 140);
                case IngredientType.Side: return new Color(140, 255, 140);
                case IngredientType.Seasoning: return new Color(140, 200, 255);
                case IngredientType.Bonus: return new Color(255, 150, 150);
                default: return Color.Black;
            }
        }
    }
}