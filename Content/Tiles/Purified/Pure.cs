using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Purified
{
	internal class StonePure : ModTile
    {
        public override string Texture => AssetDirectory.PureTile + Name;

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileStone[Type] = true;
            dustType = Mod.DustType("Purify");
            drop = TileID.Stone;
            AddMapEntry(new Color(208, 201, 199));
        }
    }

    internal class GrassPure : ModTile
    {
        public override string Texture => AssetDirectory.PureTile + Name;

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            TileID.Sets.Grass[Type] = true;
            SetModTree(new TreePure());
            dustType = Mod.DustType("Purify");
            drop = ItemID.DirtBlock;
            AddMapEntry(new Color(208, 201, 199));
        }
    }
    internal class SandPure : ModTile
    {
        public override string Texture => AssetDirectory.PureTile + Name;

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileSand[Type] = true;
            dustType = Mod.DustType("Purify");
            AddMapEntry(new Color(208, 201, 199));
        }
    }
}
