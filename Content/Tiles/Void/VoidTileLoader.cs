using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.Tiles.Void
{
    class VoidTileLoader : TileLoader
    {
        public override string AssetRoot => AssetDirectory.VoidTile;

        public override void Load()
        {
            LoadTile(
                "VoidBrick",
                "Void Bricks",
                new TileLoadData(
                    minPick: 200,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    mapColor: new Color(45, 50, 30)
                    )
                );

            LoadTile(
                "VoidStone",
                "Voidstone",
                new TileLoadData(
                    minPick: 200,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    mapColor: new Color(35, 40, 20)
                    )
                );

            //Furniture

            LoadFurniture(
                "VoidPillarBase",
                "Void Pillar Base",
                new FurnitureLoadData(
                    width: 3,
                    height: 2,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    tallBottom: false,
                    mapColor: new Color(55, 60, 40),
                    bottomAnchor: new AnchorData(AnchorType.AlternateTile, 3, 0)
                    )
                );

            LoadFurniture(
                "VoidPillarMiddle",
                "Void Pillar",
                new FurnitureLoadData(
                    width: 3,
                    height: 2,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    tallBottom: false,
                    mapColor: new Color(55, 60, 40),
                    bottomAnchor: new AnchorData(AnchorType.AlternateTile, 3, 0),
                    anchorTiles: new int[] { mod.TileType("VoidPillarBase"), mod.TileType("VoidPillarMiddle") }
                    )
                );

            LoadFurniture(
                "VoidPillarTop",
                "Void Pillar Support",
                new FurnitureLoadData(
                    width: 3,
                    height: 1,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    tallBottom: false,
                    mapColor: new Color(55, 60, 40),
                    bottomAnchor: new AnchorData(AnchorType.AlternateTile, 3, 0),
                    anchorTiles: new int[] { mod.TileType("VoidPillarBase"), mod.TileType("VoidPillarMiddle") }
                    )
                );

            LoadFurniture(
                "VoidPillarTopAlt",
                "Void Pillar Pedestal",
                new FurnitureLoadData(
                    width: 3,
                    height: 1,
                    dustType: DustType<Dusts.Darkness>(),
                    soundType: SoundID.Tink,
                    tallBottom: false,
                    mapColor: new Color(55, 60, 40),
                    solidTop: true,
                    bottomAnchor: new AnchorData(AnchorType.AlternateTile, 3, 0),
                    anchorTiles: new int[] { mod.TileType("VoidPillarBase"), mod.TileType("VoidPillarMiddle") }
                    )
                );
        }
    }
}