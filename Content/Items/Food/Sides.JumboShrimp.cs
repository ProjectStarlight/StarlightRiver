using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Food
{
    internal class JumboShrimp : Ingredient
    {
        public JumboShrimp() : base("+10% damage and movement speed underwater\nWip", 200, IngredientType.Side) { }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void BuffEffects(Player Player, float multiplier)
        {
            
        }
    }
}