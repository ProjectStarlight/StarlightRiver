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
	}

	public class SquareColliderSystem : ModSystem
	{
		public override void Load()
		{
			On_Player.DryCollision += SideCollision;
		}

		private void SideCollision(On_Player.orig_DryCollision orig, Player self, bool fallThrough, bool ignorePlats)
		{
			foreach (NPC npc in Main.ActiveNPCs)
			{
				if (npc.ModNPC == null || npc.ModNPC is not SquareCollider || (npc.ModNPC as SquareCollider).dontCollide)
					continue;

				var nextPlayer = self.Hitbox;
				nextPlayer.Offset(self.velocity.ToPoint());

				var nextBox = npc.Hitbox;
				nextBox.Offset(npc.velocity.ToPoint());

				var PlayerRect = new Rectangle((int)self.position.X, (int)self.position.Y + self.height, self.width, 1);
				var NPCRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, 8 + (self.velocity.Y > 0 ? (int)self.velocity.Y : 0));

				bool platforming = PlayerRect.Intersects(NPCRect);

				if (nextPlayer.Intersects(nextBox) && !platforming)
				{
					if (self.position.X <= npc.position.X && self.velocity.X > npc.velocity.X)
						self.velocity.X += npc.velocity.X - self.velocity.X;

					if ((self.position.X + self.width) >= (npc.position.X + npc.width) && self.velocity.X < npc.velocity.X)
						self.velocity.X += npc.velocity.X - self.velocity.X;

					if ((self.position.Y + self.height) >= (npc.position.Y + npc.height) && self.velocity.Y < npc.velocity.Y)
						self.velocity.Y += npc.velocity.Y - self.velocity.Y;
				}
			}

			orig(self, fallThrough, ignorePlats);
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
			spriteBatch.Draw(Assets.MagicPixel.Value, NPC.position - Main.screenPosition, null, Color.White, 0, Vector2.Zero, Assets.MagicPixel.Width()/500f, 0, 0);

			return false;
		}
	}
}
