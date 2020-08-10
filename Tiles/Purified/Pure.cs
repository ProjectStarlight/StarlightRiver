using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Purified
{
    internal class StonePure : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileStone[Type] = true;
            drop = ItemType<Items.Pure.StonePureItem>();
            dustType = mod.DustType("Purify");
            AddMapEntry(new Color(208, 201, 199));
        }
    }

    internal class StonePure2 : StonePure
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            name = "StonePure2";
            texture = "StarlightRiver/Tiles/Purified/StonePure";
            return true;
        }
    }
    internal class GrassPure : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.Grass[Type] = true;
            SetModTree(new TreePure());
            dustType = mod.DustType("Purify");
            drop = ItemID.DirtBlock;
            AddMapEntry(new Color(208, 201, 199));
        }
    }
    internal class SandPure : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSand[Type] = true;
            dustType = mod.DustType("Purify");
            AddMapEntry(new Color(208, 201, 199));
        }
    }
}
