using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.AstralMeteor;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Herbology
{
	public class GreenhouseGlass : ModTile
    {
        public override string Texture => AssetDirectory.HerbologyTile + Name;

        public override void SetDefaults()
        {
            this.QuickSet(0, 13, SoundID.Shatter, new Color(156, 172, 177), ModContent.ItemType<GreenhouseGlassItem>(), false, false, "Greenhouse Glass");
            Main.tileBlockLight[Type] = false;
            //Main.tileLighted[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
        }

        /*public override void RightClick(int i, int j)//Debug
		{
            Main.NewText("Manual Update");
            TileLoader.GetTile(Main.tile[i, j].type)?.RandomUpdate(i, j);
        }*/

        public override void RandomUpdate(int i, int j)
        {
            for (int k = 0; k < 10; k++)//k = max range up, this checks the area above it
                if (Main.tileSolid[Main.tile[i, j - 1 - k].type] && Main.tile[i, j - 1 - k].HasTile && !Main.tileSolidTop[Main.tile[i, j - 1 - k].type] && Main.tile[i, j - 1 - k].type != Type && Main.tile[i, j - 1 - k].type != TileID.Glass)//maybe check for just blocks that stop light?
                    break;//breaks if Solid if all of the above checks are true: Solid, active, No solidTop, not This type of block, and not glass
                else if (k == 9)
                    for (int m = 0; m < 10; m++)//k = max range down, if the area above it clear this looks for the first plant below it
                        if (Main.tileSolid[Main.tile[i, j + 1 + m].type] && Main.tile[i, j + 1 + m].HasTile && !Main.tileSolidTop[Main.tile[i, j + 1 + m].type])
                            break;//breaks if Solid is true, Active is true, and solidTop is false
                        else if (Main.tile[i, j + 1 + m].HasTile && Main.tileFrameImportant[Main.tile[i, j + 1 + m].type] && !Main.tileSolid[Main.tile[i, j + 1 + m].type])//chooses if frameimportant, non-solid, and active
                        {
                            TileLoader.GetTile(Main.tile[i, j + 1 + m].type)?.RandomUpdate(i, j + 1 + m);//runs randomUpdate on selected block
                            //TODO: I believe this doesn't work on vanilla plants since they dont use randomUpdate, figure out a way to fix this or make a case for vanilla plants
                            break;
                        }
        }
    }

    public class GreenhouseGlassItem : QuickTileItem
    {
        public GreenhouseGlassItem() : base("Greenhouse Glass", "Speeds up the growth of any plant below it\nNeeds a clear area above it", ModContent.TileType<GreenhouseGlass>(), 1, AssetDirectory.HerbologyTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);
            recipe.AddIngredient(ItemID.Glass, 10);
            recipe.AddIngredient(ModContent.ItemType<AluminumBarItem>(), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 10);
            recipe.AddRecipe();

            recipe = new ModRecipe(Mod);//4 wall to 1 tile
            recipe.AddIngredient(ModContent.ItemType<GreenhouseWallItem>(), 4);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 1);
            recipe.AddRecipe();
        }
    }

    public class GreenhouseWall : ModWall
    {
        public override string Texture => AssetDirectory.HerbologyTile + Name;

        public override void SetDefaults()
        {
            Main.wallHouse[Type] = true;
            drop = ModContent.ItemType<GreenhouseWallItem>();
        }
    }

    public class GreenhouseWallItem : QuickWallItem
    {
        public GreenhouseWallItem() : base("Greenhouse Glass Wall", "Fancy!", ModContent.WallType<GreenhouseWall>(), 0, AssetDirectory.HerbologyTile) { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(Mod);//1 tile to 4 wall
            recipe.AddIngredient(ModContent.ItemType<GreenhouseGlassItem>(), 1);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 4);
            recipe.AddRecipe();

            //recipe = new ModRecipe(Mod);//old recipe when this wall was seperate from the tile
            //recipe.AddIngredient(ItemID.Glass, 10);
            //recipe.AddIngredient(ItemID.Wood, 5);
            //recipe.AddTile(TileID.WorkBenches);
            //recipe.SetResult(this, 10);
            //recipe.AddRecipe();
        }
    }
}