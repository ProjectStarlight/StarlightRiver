using StarlightRiver.Core.Loaders.TileLoading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Starlight
{
	internal class BasicStarlightTileLoader : SimpleTileLoader
	{
		public override string AssetRoot => AssetDirectory.StarlightTile;

		public override float Priority => 2.04f;

		public override void Load()
		{

		}

		public override void PostLoad()
		{

		}
	}
}
