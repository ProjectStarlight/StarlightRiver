using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Tiles.Forest
{
    class Sod : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileMerge[Type][TileID.Grass] = true;
            Main.tileMerge[TileID.Grass][Type] = true;

            Main.tileSolid[Type] = true;
            //Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.NeedsGrassFraming[Type] = true;

            SetModTree(new TreeStarlight());
            drop = ItemID.DirtBlock;
            AddMapEntry(new Color(100, 200, 220));
            soundType = SoundID.Dig;
            dustType = DustID.Dirt;
        }
    }
}
