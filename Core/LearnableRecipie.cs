using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class LearnableRecipe : ModRecipe //PORTTODO: Figure out how to replace this
    {
        private readonly string recipeName;

        public LearnableRecipe(string recipeName) : base(StarlightRiver.Instance) { this.recipeName = recipeName; }

        public override bool RecipeAvailable() => StarlightWorld.knownRecipies.Contains(recipeName);
    }
}
