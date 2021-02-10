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
        /// <summary>
        /// The list of tile types which will transform into a given purified tile. When reverted, they will transform back into the first entry into this list.
        /// </summary>
        public List<int> transformFrom;
        public int transformTo;

        public PurifyTransformation(List<int> from, int to)
        {
            transformFrom = from;
            transformTo = to;
        }

        public PurifyTransformation(int from, int to)
        {
            transformFrom = new List<int>() { from };
            transformTo = to;
        }

        public static void TransformTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;

            Tile tile = Framing.GetTileSafely(x, y);
            var transformation = TransformationLoader.transformations.FirstOrDefault(n => n.transformFrom.Contains(tile.type));

            if(transformation.transformFrom != null)
            {
                tile.type = (ushort)transformation.transformTo;
                WorldGen.TileFrame(x, y);
            }
        }

        public static void RevertTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y)) return;

            Tile tile = Framing.GetTileSafely(x, y);
            var transformation = TransformationLoader.transformations.FirstOrDefault(n => n.transformTo == tile.type);

            if (transformation.transformTo != 0)
            {
                tile.type = (ushort)transformation.transformFrom[0];
                WorldGen.TileFrame(x, y);
            }
        }
    }
}
