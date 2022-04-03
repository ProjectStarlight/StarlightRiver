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

        public override void ModifyTooltips(Item Item, List<TooltipLine> tooltips)
        {
            if (ItemIsDerivativeOfShoeSpikes(Item))
            {
                TooltipLine tooltipLine = new TooltipLine(Mod, "StarlightRiver:ShoeSpikesInfo", "Massively increased acceleration when touching the ground");

                tooltips.Add(tooltipLine);
            }
        }

        public override void UpdateAccessory(Item Item, Player Player, bool hideVisual)
        {
            if (ItemIsDerivativeOfShoeSpikes(Item) && Player.velocity.Y == 0)
            {
                Player.runAcceleration *= 3;
                Player.runSlowdown *= 3;
            }
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);

            recipe.AddIngredient(ItemID.Silk, 15);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
            recipe.AddTile(TileID.TinkerersWorkbench);

            recipe.SetResult(ItemID.ShoeSpikes);

            recipe.AddRecipe();
        }

        private bool ItemIsDerivativeOfShoeSpikes(Item Item)
        {
            if (Item.type == ItemID.ShoeSpikes || Item.type == ItemID.ClimbingClaws || Item.type == ItemID.MasterNinjaGear)
                return true;

            return ShoeSpikeAccessories.Contains(Item.type);
        }

		public void PostLoad()
		{
            RecipeFinder finder = new RecipeFinder();

            for (int k = Main.maxItemTypes; k < ItemLoader.ItemCount; k++)
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
