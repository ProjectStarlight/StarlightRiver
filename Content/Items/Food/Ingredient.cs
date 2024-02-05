using StarlightRiver.Content.Items.Utility;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;

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
		public float BuffLengthMult = 1;
		public float DebuffLengthMult = 1;

		/// <param name="tooltip">Extra tooltip lines</param>
		/// <param name="filling">How much time this should add to the food, time in seconds is this divided by 60</param>
		/// <param name="buffLengthMult">multiplies the buff length, but not full debuff length</param>
		/// <param name="debuffLengthMult">multiplies the full debuff length, but not the buff length</param>
		protected Ingredient(string tooltip, int filling, IngredientType type, float buffLengthMult = 1f, float debuffLengthMult = 1f)
		{
			Fill = filling;
			ItemTooltip = tooltip;
			ThisType = type;
			BuffLengthMult = buffLengthMult;
			DebuffLengthMult = debuffLengthMult;
		}

		public override string Texture => AssetDirectory.FoodItem + Name;

		public virtual void SafeAddRecipes() { }

		public sealed override void AddRecipes() //this is dumb, too bad!
		{
			SafeAddRecipes();

			if (ThisType != IngredientType.Bonus)
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
		/// <param name="Player">The player eating the food</param>
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
			string description = GetDescription(ThisType);
			Color nameColor = GetColor(ThisType);
			Color descriptionColor = GetDescriptionColor(ThisType);

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
				var fullLine = new TooltipLine(Mod, "StarlightRiver: Fullness", $"adds {Fill / 3600}m {Fill % 3600 / 60}s duration to food")
				{
					OverrideColor = new Color(110, 235, 255)
				};

				tooltips.Add(fullLine);
			}
		}

		public string GetDescription()
		{
			return GetDescription(ThisType);
		}

		public Color GetColor()
		{
			return GetColor(ThisType);
		}

		public Color GetDescriptionColor()
		{
			return GetDescriptionColor(ThisType);
		}

		public static string GetDescription(IngredientType type)
		{
			return type switch
			{
				IngredientType.Main => "Main Course",
				IngredientType.Side => "Side Dish",
				IngredientType.Seasoning => "Seasonings",
				IngredientType.Bonus => "Bonus Effects",
				_ => "ERROR",
			};
		}

		public static Color GetColor(IngredientType type)
		{
			return type switch
			{
				IngredientType.Main => new Color(255, 220, 140),
				IngredientType.Side => new Color(140, 255, 140),
				IngredientType.Seasoning => new Color(140, 200, 255),
				IngredientType.Bonus => new Color(255, 150, 150),
				_ => Color.Black,
			};
		}

		public static Color GetDescriptionColor(IngredientType type)
		{
			return type switch
			{
				IngredientType.Main => new Color(255, 220, 80),
				IngredientType.Side => new Color(80, 255, 80),
				IngredientType.Seasoning => new Color(80, 140, 255),
				IngredientType.Bonus => new Color(255, 140, 140),
				_ => Color.Black,
			};
		}
	}
}