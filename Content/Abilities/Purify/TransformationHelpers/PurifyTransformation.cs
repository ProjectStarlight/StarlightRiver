using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.Abilities.Purify.TransformationHelpers
{
    struct PurifyTransformation
    {
        public List<int> transformFrom;
        public int transformTo;

        public static void TransformTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;

            Tile tile = Framing.GetTileSafely(x, y);
            var transformation = TransformationLoader.transformations.FirstOrDefault(n => n.transformFrom.Contains(tile.type));

            if(transformation != default)
            {
                tile.type = transformation.transformTo;
                WorldGen.TileFrame(x, y);
            }
        }
    }
}
