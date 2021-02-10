using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Core
{
    class LearnableRecipe : ModRecipe
    {
        private readonly string recipeName;

        public LearnableRecipe(string recipeName) : base(StarlightRiver.Instance) { this.recipeName = recipeName; }

        public override bool RecipeAvailable() => StarlightWorld.knownRecipies.Contains(recipeName);
    }
}
