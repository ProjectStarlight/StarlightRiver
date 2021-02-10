using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Herbology
{
    public class GreenhouseGlass : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.HerbologyTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = false;
            Main.tileBlockLight[Type] = false;
            //Main.tileLighted[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;
            drop = mod.ItemType("GreenhouseGlassItem");
            dustType = 13;
            soundType = SoundID.Shatter;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Greenhouse Glass");
            AddMapEntry(new Color(156, 172, 177), name);
        }

        /*public override void RightClick(int i, int j)//Debug
		{
            Main.NewText("Manual Update");
            TileLoader.GetTile(Main.tile[i, j].type)?.RandomUpdate(i, j);
        }*/

        public override void RandomUpdate(int i, int j)
        {
            for (int k = 0; k < 10; k++)//k = max range up, this checks the area above it
                if (Main.tileSolid[Main.tile[i, j - 1 - k].type] && Main.tile[i, j - 1 - k].active() && !Main.tileSolidTop[Main.tile[i, j - 1 - k].type] && Main.tile[i, j - 1 - k].type != Type && Main.tile[i, j - 1 - k].type != TileID.Glass)//maybe check for just blocks that stop light?
                    break;//breaks if Solid if all of the above checks are true: Solid, active, No solidTop, not This type of block, and not glass
                else if (k == 9)
                    for (int m = 0; m < 10; m++)//k = max range down, if the area above it clear this looks for the first plant below it
                        if (Main.tileSolid[Main.tile[i, j + 1 + m].type] && Main.tile[i, j + 1 + m].active() && !Main.tileSolidTop[Main.tile[i, j + 1 + m].type])
                            break;//breaks if Solid is true, Active is true, and solidTop is false
                        else if (Main.tile[i, j + 1 + m].active() && Main.tileFrameImportant[Main.tile[i, j + 1 + m].type] && !Main.tileSolid[Main.tile[i, j + 1 + m].type])//chooses if frameimportant, non-solid, and active
                        {
                            TileLoader.GetTile(Main.tile[i, j + 1 + m].type)?.RandomUpdate(i, j + 1 + m);//runs randomUpdate on selected block
                            break;
                        }
        }
    }
}