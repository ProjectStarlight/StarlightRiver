using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Food
{
    internal class FoodBuffHandler : ModPlayer
    {
        public List<Item> Consumed { get; set; } = new List<Item>(); //all of the ingredients in the food the player ate
        public float Multiplier { get; set; } = 1; //the multipler that should be applied to those ingredients

        //public override void PostUpdateBuffs()
        //{
        //}

        public override void ResetEffects()
        {
            if (!player.HasBuff(BuffType<Buffs.FoodBuff>()) && Consumed.Count > 0)
                Consumed.Clear(); //clears the player's "belly" if they're not under the effects of food anymore, also resets the multiplier just in case.

            Multiplier = 1;

            foreach (Item item in Consumed.Where(n => n.modItem is Ingredient))
                (item.modItem as Ingredient).ResetBuffEffects(player, Multiplier);
        }
    }
}