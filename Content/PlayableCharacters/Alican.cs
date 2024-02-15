using StarlightRiver.Core.Systems.PlayableCharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace StarlightRiver.Content.PlayableCharacters
{
	internal class Alican : PlayableCharacter
	{
		int jumpCharge = 300;

		public override void PreUpdate()
		{
			// Adjust health/mana correctly while still forbidding vanilla upgrades
			player.statLifeMax = 500;
			player.statManaMax = 200;

			player.statLifeMax2 = 300;
			player.statManaMax2 = 0;

			if (jumpCharge < 300)
				jumpCharge++;

			if (player.velocity.Y == 0 && jumpCharge < 300)
				jumpCharge += 5;

			if (jumpCharge > 300)
				jumpCharge = 300;
		}

		public override bool OnJumpInput()
		{
			Main.NewText(player.releaseJump + " | " + player.controlJump + " | " + player.jump);

			if (player.controlJump && player.releaseJump)
			{
				if (player.velocity.Y == 0)
				{
					player.jump = 7;
				}
				else if (jumpCharge >= 100)
				{
					jumpCharge -= 100;
					player.velocity *= 0.1f;
					player.jump = 6;

					player.direction = player.velocity.X > 0 ? 1 : -1;
				}

				player.releaseJump = false;
			}

			if (!player.controlJump)
			{
				player.releaseJump = true;
				player.jump = 0;

				player.velocity.X *= 0.9f;
			}
			else
			{
				// Glide
				player.velocity.Y *= 0.9f;
			}


			player.velocity.Y -= player.jump;

			if (player.controlLeft)
				player.velocity.X -= player.jump * 0.5f;

			if (player.controlRight)
				player.velocity.X += player.jump * 0.5f;

			if (player.jump > 0)
				player.jump--;

			return false;
		}

		public override void UpdatePhysics()
		{
			if (player.velocity.Y != player.gravity)
			{
				player.velocity.Y *= 0.97f; // Innate glide
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/PlayableCharacters/Alican").Value;
			var effects = player.direction == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			drawInfo.DrawDataCache.Add(
				new DrawData(tex, player.Center - Main.screenPosition + new Vector2(0, -8 + player.gfxOffY), null, Lighting.GetColor((player.Center / 16).ToPoint()), player.fullRotation, tex.Size() / 2f, 1, effects, 0)
				);

			var bar = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/SmallBar1").Value;
			var target = new Rectangle((int)(player.Center.X - Main.screenPosition.X - bar.Width / 2), (int)(player.Center.Y - Main.screenPosition.Y - 32), (int)(bar.Width * (jumpCharge / 300f)), bar.Height);
			var source = new Rectangle(0, 0, (int)(bar.Width * (jumpCharge / 300f)), bar.Height);
			drawInfo.DrawDataCache.Add(
				new DrawData(bar, target, source, Color.White, 0, Vector2.Zero, 0, 0)
				);
		}
	}
}
