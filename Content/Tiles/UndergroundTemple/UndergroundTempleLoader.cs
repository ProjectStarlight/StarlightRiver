using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class UndergroundTempleLoader : SimpleTileLoader
    {
        public override string AssetRoot => AssetDirectory.UndergroundTempleTile;

        public override void Load()
        {
            LoadTile(
                "TempleBrick",
                "Ancient Bricks",
                new TileLoadData(
                    minPick: 0,
                    dustType: DustID.Stone,
                    hitSound: SoundID.Tink,
                    mapColor: new Color(150, 150, 150),
                    stone: true
                    )
                );
        }
    }
}
