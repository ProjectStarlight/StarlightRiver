using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class ShoeSpikesModification : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ItemIsDerivativeOfShoeSpikes(item))
            {
                TooltipLine tooltipLine = new TooltipLine(mod, "StarlightRiver:ShoeSpikesInfo", "Greatly increases running acceleration if the player is touching the ground");

                tooltips.Add(tooltipLine);
            }
        }

        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            if (ItemIsDerivativeOfShoeSpikes(item) && player.velocity.Y == 0)
            {
                player.runAcceleration *= 3;
                player.runSlowdown *= 3;
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);

            recipe.SetResult(ItemID.ShoeSpikes);

            recipe.AddRecipe();
        }

        private bool ItemIsDerivativeOfShoeSpikes(Item item)
        {
            if (item.type == ItemID.ShoeSpikes || item.type == ItemID.ClimbingClaws || item.type == ItemID.MasterNinjaGear)
            {
                return true;
            }

            // TODO: This is an attempt at mod compatibility, but to work properly this would likely require a few layers of recursive searching for items with huge crafting trees.
            RecipeFinder finder = new RecipeFinder();

            finder.SetResult(item.type);
            finder.AddIngredient(ItemID.ShoeSpikes);

            return finder.SearchRecipes().Count > 0;
        }
    }
}
