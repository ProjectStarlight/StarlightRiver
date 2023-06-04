using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core.Systems.CameraSystem;
using System;
using System.Linq;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class Laser : InteractiveProjectile, IUnderwater
	{
		public NPC parent;

		public int height;

		public override string Texture => AssetDirectory.SquidBoss + Name;

		public override void SetDefaults()
		{
			Projectile.width = 60;
			Projectile.height = 1;
			Projectile.damage = 50;
			Projectile.hostile = true;
			Projectile.timeLeft = Main.masterMode ? 360 : Main.expertMode ? 510 : 660;
			Projectile.aiStyle = -1;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			return false;
		}

		public override bool PreDraw(ref Color drawColor)
		{
			return false;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 659 || Main.expertMode && Projectile.timeLeft == 509 || Main.masterMode && Projectile.timeLeft == 359)
			{
				int y = (int)Projectile.Center.Y / 16 - 28;

				int xOff = (parent.ModNPC as SquidBoss).variantAttack ? 18 : -76;

				for (int k = 0; k < 59; k++)
				{
					if (Main.masterMode && ((k + 1) % 20 <= 5 || (k + 1) % 20 >= 15))
						continue;

					int x = (int)Projectile.Center.X / 16 + xOff + k;
					ValidPoints.Add(new Point16(x, y));
				}
			}

			Projectile.ai[1]++;

			Projectile.Center = parent.Center;

			//collision
			int height = 0;

			for (int k = 0; k < 200; k++)
			{
				Vector2 pos = Projectile.Center + new Vector2(0, -16 * k);
				height += 16;

				for (int i = -2; i <= 2; i++)
				{
					if (Main.tile[(int)pos.X / 16 + i, (int)pos.Y / 16].HasTile)
						k = 200;
				}
			}

			this.height = height;

			var rect = new Rectangle((int)Projectile.position.X, (int)Projectile.position.Y - height + 16, Projectile.width, height - 16);

			float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
			var color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);

			if (Main.masterMode)
				color = new Color(1, 0.65f + sin * 0.25f, 0.25f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			for (int k = 0; k < rect.Height; k += 500)
			{
				int i = Dust.NewDust(rect.TopLeft() + Vector2.UnitY * k, rect.Width, rect.Height - k, ModContent.DustType<Dusts.Glow>(), 0, -6, 0, color, Main.rand.NextFloat(0.4f, 0.6f));
				Main.dust[i].noLight = true;
			}

			if (Projectile.timeLeft > 30)
			{
				Vector2 endPos = Projectile.Center - Vector2.UnitY * (height - 84);

				for (int k = 0; k < 5; k++)
				{
					Vector2 vel = Vector2.UnitY.RotatedByRandom(2f) * Main.rand.NextFloat(15);
					Dust.NewDustPerfect(endPos, ModContent.DustType<Dusts.ColoredSpark>(), vel, 0, color, Main.rand.NextFloat(1.2f, 2.6f));
				}

				if (CameraSystem.shake < 10)
					CameraSystem.shake += (int)Math.Max(0, 1.5f - Math.Abs(Main.LocalPlayer.Center.X - endPos.X) * 0.0025f);
			}

			foreach (Player Player in Main.player.Where(n => n.active && n.Hitbox.Intersects(rect)))
			{
				Player.Hurt(PlayerDeathReason.ByCustomReason(Player.name + " got lasered to death by a squid..."), 50, 0);
			}
		}

		public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
		{
			float sin = 1 + (float)Math.Sin(Projectile.ai[1] / 10f);
			float cos = 1 + (float)Math.Cos(Projectile.ai[1] / 10f);
			Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.05f;

			if (Main.masterMode)
				color = new Color(1, 0.5f + sin * 0.25f, 0.25f) * (Projectile.timeLeft < 30 ? (Projectile.timeLeft / 30f) : 1);

			int denom = Main.masterMode ? 330 : Main.expertMode ? 480 : 630;

			float alpha = Projectile.timeLeft > denom ? 1 - (Projectile.timeLeft - denom) / 30f : Projectile.timeLeft < 30 ? Projectile.timeLeft / 30f : 1;
			color *= alpha;

			Texture2D texBeam = ModContent.Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value;
			Texture2D texBeam2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;
			Texture2D texStar = ModContent.Request<Texture2D>(AssetDirectory.GUI + "ItemGlow").Value;

			var origin = new Vector2(0, texBeam.Height / 2);
			var origin2 = new Vector2(0, texBeam2.Height / 2);

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/GlowingDust").Value;

			effect.Parameters["uColor"].SetValue(color.ToVector3());

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointWrap, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			float height = texBeam2.Height / 2f * 1.5f;
			int adjustedLaserHeight = this.height - 32;

			for (int k = 0; k <= adjustedLaserHeight; k += 500)
			{
				if (k > (adjustedLaserHeight - 500)) //Change to end for the last segment
					texBeam2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailOneEnd").Value;

				Vector2 pos = Projectile.Center + Vector2.UnitY * -k - Main.screenPosition;
				int thisHeight = k > (adjustedLaserHeight - 500) ? (adjustedLaserHeight % 500) : 500;

				var source = new Rectangle((int)(Projectile.ai[1] * 0.01f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
				var source1 = new Rectangle((int)(Projectile.ai[1] * 0.023f * -texBeam.Width), 0, (int)(texBeam.Width * thisHeight / 500f), texBeam.Height);
				var source2 = new Rectangle(0, 0, (int)(texBeam2.Width * thisHeight / 500f), texBeam2.Height);

				var target = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(height * 1.25f * alpha));
				var target2 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(height * 2.8f * alpha));
				var target3 = new Rectangle((int)pos.X, (int)pos.Y, thisHeight, (int)(50 * alpha));

				spriteBatch.Draw(texBeam, target, source, color * 0.65f, -1.57f, origin, 0, 0);
				spriteBatch.Draw(texBeam, target, source1, color * 0.45f, -1.57f, origin, 0, 0);
				spriteBatch.Draw(texBeam2, target2, source2, color * 0.65f, -1.57f, origin2, 0, 0);
				spriteBatch.Draw(texBeam2, target3, source2, color * 1.1f, -1.57f, origin2, 0, 0);
			}

			spriteBatch.Draw(texStar, Projectile.Center - Vector2.UnitY * (this.height - 16) - Main.screenPosition, null, color * 1.1f, Projectile.ai[1] * 0.025f, texStar.Size() / 2, 1, 0, 0);
			spriteBatch.Draw(texStar, Projectile.Center - Vector2.UnitY * (this.height - 16) - Main.screenPosition, null, color * 1.1f, Projectile.ai[1] * -0.045f, texStar.Size() / 2, 0.65f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

		}
	}
}