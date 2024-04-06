using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.LightingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Bosses.BrainRedux
{
	internal class BrainPlatform : MovingPlatform
	{
		public override string Texture => "StarlightRiver/Assets/MagicPixel";

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;
			var target = new Rectangle((int)(NPC.position.X - Main.screenPosition.X), (int)(NPC.position.Y - Main.screenPosition.Y), NPC.width, NPC.height);
			//spriteBatch.Draw(tex, target, Color.White);

			LightingBufferRenderer.DrawWithLighting(target, tex, new Rectangle(0, 0, NPC.width, NPC.height), default, Vector2.One);

			return false;
		}

		public override void SafeSetDefaults()
		{
			NPC.width = 400;
			NPC.height = 12;
		}
	}
}
