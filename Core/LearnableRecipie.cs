using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StarlightRiver.Buffs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Core
{
    class LearnableRecipe : ModRecipe
    {
        private readonly string recipeName;

        public LearnableRecipe(string recipeName) : base(StarlightRiver.Instance) { this.recipeName = recipeName; }

        public override bool RecipeAvailable() => StarlightWorld.knownRecipies.Contains(recipeName);
    }
}
