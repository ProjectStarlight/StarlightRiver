using Microsoft.Xna.Framework;
using StarlightRiver.Items;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Food
{
    internal class DoesThisModAddAnyDropsToOGRE : GlobalNPC
    {
        public override void NPCLoot(NPC npc)
        {
            if (npc.type == NPCID.DD2OgreT3 || npc.type == NPCID.DD2OgreT2) Item.NewItem(npc.Center, ItemType<Onion>());
        }
    }

    public class Onion : QuickMaterial
    { public Onion() : base("Onion", "Does this mod add any drops to ogre?", 69420, 69420, -12) { } }


    public class OnionRings : Ingredient
    {
        public OnionRings() : base("Damaging stink aura", 600, IngredientType.Side) { }

        public override void BuffEffects(Player player, float multiplier)
        {
            if (Main.rand.Next(10) == 0)
            {
                foreach (NPC npc in Main.npc.Where(n => n.active && Vector2.Distance(n.Center, player.Center) <= 100 && !n.immortal))
                {
                    npc.StrikeNPC((int)(5 * multiplier), 0, 0);
                }
            }
            Dust.NewDustPerfect(player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), DustType<Dusts.GasGreen>(), null, 0, default, 10);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<Onion>());
            recipe.AddTile(TileType<Tiles.Crafting.Oven>());
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}