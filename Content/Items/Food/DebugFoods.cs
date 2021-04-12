using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food
{
    internal class DebugGreen : Ingredient
    {
        public DebugGreen() : base("+5% melee damage", 600, IngredientType.Main) { }
        public override string Texture => AssetDirectory.FoodItem + "Rabbit";

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Green;
    }

    internal class DebugOrange : Ingredient
    {
        public DebugOrange() : base("+5% melee damage", 600, IngredientType.Main) { }
		public override string Texture => AssetDirectory.FoodItem + "Rabbit";

		public override void SafeSetDefaults() => item.rare = ItemRarityID.Orange;
    }

    internal class DebugRed : Ingredient
    {
        public DebugRed() : base("+5% melee damage", 600, IngredientType.Main) { }
        public override string Texture => AssetDirectory.FoodItem + "Rabbit";

        public override void SafeSetDefaults() => item.rare = ItemRarityID.LightRed;
    }

    internal class DebugPink : Ingredient
    {
        public DebugPink() : base("+5% melee damage", 600, IngredientType.Main) { }
        public override string Texture => AssetDirectory.FoodItem + "Rabbit";

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Pink;
    }

    internal class DebugPurple : Ingredient
    {
        public DebugPurple() : base("+5% melee damage", 600, IngredientType.Main) { }
        public override string Texture => AssetDirectory.FoodItem + "Rabbit";

        public override void SafeSetDefaults() => item.rare = ItemRarityID.LightPurple;
    }
}
