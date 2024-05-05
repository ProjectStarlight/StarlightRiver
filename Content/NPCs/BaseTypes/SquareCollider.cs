using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.NPCs.BaseTypes
{
	internal abstract class SquareCollider : MovingPlatform
	{
		public override bool CanFallThrough => false;

		public override void SafeAI()
		{
			
		}
	}

	internal class TestCollider : SquareCollider
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void SafeSetDefaults()
		{
			NPC.width = 500;
			NPC.height = 500;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			spriteBatch.Draw();

			return false;
		}
	}
}
