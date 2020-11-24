using Microsoft.Xna.Framework;
using StarlightRiver.Core.Loaders;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Vitric
{
    class BasicVitricTileLoader : TileLoader
    {
        public override string AssetRoot => Directory.VitricTileDir;

        public override void Load()
        {
            LoadTile(
                "AncientSandstone",
                "Ancient Sandstone",
                new TileLoadData(
                    minPick: 200,
                    dustType: DustID.Copper,
                    soundType: SoundID.Tink,
                    mapColor: new Color(160, 115, 75)
                    )
                );

            LoadTile(
                "AncientSandstoneTile",
                "Ancient Sandstone Tile",
                new TileLoadData(
                    minPick: 200,
                    dustType: DustID.Copper,
                    soundType: SoundID.Tink,
                    mapColor: new Color(160, 115, 75)
                    )
                );

            LoadTile(
                "VitricBrick",
                "Vitric Bricks",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustType<Dusts.Glass3>(),
                    soundType: SoundID.Shatter,
                    mapColor: new Color(190, 255, 245)
                )
              );

            LoadTile(
                "VitricCactus",
                "Crystal Cactus",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustType<Dusts.Glass3>(),
                    soundType: SoundID.Shatter,
                    mapColor: new Color(190, 255, 245)
                )
              );

            LoadTile(
                "VitricGlass",
                "Vitric Glass",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustType<Dusts.Glass3>(),
                    soundType: SoundID.Shatter,
                    mapColor: new Color(190, 255, 245)
                )
              );

            LoadTile(
                "VitricSand",
                "Glassy Sand",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustType<Dusts.Air3>(),
                    soundType: SoundID.Dig,
                    mapColor: new Color(172, 131, 105)
                )
              );

            LoadTile(
                "VitricSoftSand",
                "Soft Glassy Sand",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustType<Dusts.Air3>(),
                    soundType: SoundID.Dig,
                    mapColor: new Color(182, 141, 115)
                )
              );

        }
    }
}
