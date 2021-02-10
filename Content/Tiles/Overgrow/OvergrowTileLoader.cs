using Microsoft.Xna.Framework;
using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Overgrow
{
    class OvergrowTileLoader : TileLoader
    {
        public override string AssetRoot => AssetDirectory.OvergrowTile;

        public override void Load()
        {
            LoadTile(
                "LeafOvergrow",
                "Faerie Leaves",
                new TileLoadData(
                    minPick: 210,
                    dustType: DustType<Dusts.Leaf>(),
                    soundType: SoundID.Grass,
                    mapColor: new Color(79, 76, 71),
                    dirtMerge: true,
                    stone: true
                    )
                );

            LoadTile(
                "BrickOvergrow",
                "Runic Bricks",
                new TileLoadData(
                    minPick: 210,
                    dustType: DustID.Stone,
                    soundType: SoundID.Tink,
                    mapColor: new Color(221, 211, 67)
                    )
                );

            LoadTile(
                "StoneOvergrow",
                "Uhhhhh... Runic Stone?",
                new TileLoadData(
                    minPick: 210,
                    dustType: DustID.Stone,
                    soundType: SoundID.Tink,
                    mapColor: new Color(205, 200, 55)
                    )
                );
        }

        public override void PostLoad()
        {
            int typeLeafOvergrow = mod.TileType("LeafOvergrow");
            int typeBrickOvergrow = mod.TileType("BrickOvergrow");
            int typeStoneOvergrow = mod.TileType("StoneOvergrow");

            Main.tileMerge[typeLeafOvergrow][typeBrickOvergrow] = true;
            Main.tileMerge[typeLeafOvergrow][typeStoneOvergrow] = true;
            Main.tileMerge[typeLeafOvergrow][TileType<GlowBrickOvergrow>()] = true;
            Main.tileMerge[typeLeafOvergrow][TileType<GrassOvergrow>()] = true;

            Main.tileMerge[typeBrickOvergrow][typeLeafOvergrow] = true;
            Main.tileMerge[typeBrickOvergrow][typeStoneOvergrow] = true;
            Main.tileMerge[typeBrickOvergrow][TileType<GlowBrickOvergrow>()] = true;
            Main.tileMerge[typeBrickOvergrow][TileType<GrassOvergrow>()] = true;
            Main.tileMerge[typeBrickOvergrow][mod.GetTile("CrusherTile").Type] = true;
            Main.tileMerge[typeBrickOvergrow][TileID.BlueDungeonBrick] = true;
            Main.tileMerge[typeBrickOvergrow][TileID.GreenDungeonBrick] = true;
            Main.tileMerge[typeBrickOvergrow][TileID.PinkDungeonBrick] = true;

            Main.tileMerge[typeStoneOvergrow][typeLeafOvergrow] = true;
            Main.tileMerge[typeStoneOvergrow][typeBrickOvergrow] = true;
            Main.tileMerge[typeStoneOvergrow][TileType<GrassOvergrow>()] = true;
        }
    }
}