using StarlightRiver.Content.Items.Utility;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

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

		protected Ingredient(string tooltip, int filling, IngredientType type)
		{
			Fill = filling;
			ItemTooltip = tooltip;
			ThisType = type;
		}

		public override string Texture => AssetDirectory.FoodItem + Name;

		public override void AddRecipes() //this is dumb, too bad!
		{
			ChefBag.ingredientTypes.Add(Item.type);
		}

		protected bool Active(Player player)
		{
			return player.GetModPlayer<FoodBuffHandler>().Consumed.Any(n => n.type == Type);
		}

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
				case IngredientType.Main:
					description = "$Mods.StarlightRiver.Items.Food.Ingredient.MainCourse";
					nameColor = new Color(255, 220, 140);
					descriptionColor = new Color(255, 220, 80);
					break;
				case IngredientType.Side:
					description = "$Mods.StarlightRiver.Items.Food.Ingredient.SideDish";
					nameColor = new Color(140, 255, 140);
					descriptionColor = new Color(80, 255, 80);
					break;
				case IngredientType.Seasoning:
					description = "$Mods.StarlightRiver.Items.Food.Ingredient.Seasonings";
					nameColor = new Color(140, 200, 255);
					descriptionColor = new Color(80, 140, 255);
					break;
				case IngredientType.Bonus:
					description = "$Mods.StarlightRiver.Items.Food.Ingredient.BonusEffects";
					nameColor = new Color(255, 200, 200);
					descriptionColor = new Color(255, 140, 140);
					break;
				default:
					description = "ERROR";
					nameColor = Color.Black;
					descriptionColor = Color.Black;
					break;
			}

			foreach (TooltipLine line in tooltips)
			{
				if (line.Mod == "Terraria" && line.Name == "Tooltip0")
				{
					line.Text = description;
					line.OverrideColor = nameColor;
				}

				if (line.Mod == "Terraria" && line.Name == "Tooltip1")
				{
					line.Text = ItemTooltip;
					line.OverrideColor = descriptionColor;
				}
			}

			if (ThisType != IngredientType.Bonus)
			{
				var fullLine = new TooltipLine(Mod, "StarlightRiver: Fullness",
					string.Format(Language.GetTextValue("$Mods.StarlightRiver.Items.Food.Ingredient.FullnessTooltip"),
						Fill / 60)) { OverrideColor = new Color(110, 235, 255) };

				tooltips.Add(fullLine);
			}
		}

		public Color GetColor()
		{
			return ThisType switch
			{
				IngredientType.Main => new Color(255, 220, 140),
				IngredientType.Side => new Color(140, 255, 140),
				IngredientType.Seasoning => new Color(140, 200, 255),
				IngredientType.Bonus => new Color(255, 150, 150),
				_ => Color.Black,
			};
		}
	}
}
