using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
	public class ShoeSpikesModification : GlobalItem, IPostLoadable
    {
        private static List<int> ShoeSpikeAccessories = new List<int>();

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (ItemIsDerivativeOfShoeSpikes(item))
            {
                TooltipLine tooltipLine = new TooltipLine(mod, "StarlightRiver:ShoeSpikesInfo", "Massively increased acceleration when touching the ground");

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
                return true;

            return ShoeSpikeAccessories.Contains(item.type);
        }

		public void PostLoad()
		{
            RecipeFinder finder = new RecipeFinder();

            for (int k = Main.maxItems; k < ItemLoader.ItemCount; k++)
            {
                finder.SetResult(k);
                finder.AddIngredient(ItemID.ShoeSpikes);

                if (finder.SearchRecipes().Count > 0)
                    ShoeSpikeAccessories.Add(k);
            }
        }

        public void PostLoadUnload()
		{
            ShoeSpikeAccessories.Clear();
        }
	}
}
