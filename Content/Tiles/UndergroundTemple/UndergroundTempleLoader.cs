using Microsoft.Xna.Framework;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Items;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
    class UndergroundTempleLoader : TileLoader
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
                    soundType: SoundID.Tink,
                    mapColor: new Color(150, 150, 150),
                    stone: true
                    )
                );
        }
    }
}
