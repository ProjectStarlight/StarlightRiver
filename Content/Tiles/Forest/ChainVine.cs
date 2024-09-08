using StarlightRiver.Content.Tiles.BaseTypes;
using StarlightRiver.Core.Systems.FoliageLayerSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class ChainVine : PhysicsChain
	{
		public ChainVine() 
		{
			segmentLength = 24;
			segmentLengthMultiplier = 1.25f;
		}

		public override void PerPointDraw(SpriteBatch spriteBatch, Vector2 worldPos, Vector2 nextPos)
		{
			var tex = Assets.Tiles.Forest.ForestBerryBushItem.Value;
			FoliageLayerSystem.data.Add(new(tex, worldPos, null, new Color(Lighting.GetSubLight(worldPos)), worldPos.DirectionTo(nextPos).ToRotation(), tex.Size() / 2f, 1f, 0, 0));
		}
	}
}
