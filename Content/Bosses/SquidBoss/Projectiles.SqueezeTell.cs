using System;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class SqueezeTell : ModProjectile, IDrawAdditive
	{
		public Vector2 endPoint;

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tentacle Warning");
		}

		public override void SetDefaults()
		{
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 180;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.damage = 0;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D arrow = ModContent.Request<Texture2D>(AssetDirectory.SquidBoss + "SqueezeTellArrow").Value;
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrailNoEnd").Value;
			Texture2D flat = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			float timer = 180 - Projectile.timeLeft;
			float prog = timer / 180f;

			Vector2 basePos = Projectile.Center - Main.screenPosition;

			var leftTarget = new Rectangle((int)(basePos.X - 100), (int)basePos.Y, 4, 1000);
			var rightTarget = new Rectangle((int)(basePos.X + 100), (int)basePos.Y, 4, 1000);
			var thinSource = new Rectangle(50, 0, glow.Width - 100, glow.Height);
			var origin = new Vector2(glow.Height / 2, 0);
			var underGlowTarget = new Rectangle((int)basePos.X - 98, (int)basePos.Y + 450, 1000, 100);
			var flashGlowTarget = new Rectangle((int)basePos.X - 98, (int)basePos.Y + 450, 1000, (int)(timer / 20f * 800));
			var underGlowSource = new Rectangle(0, 0, glow.Width, glow.Height / 2);

			spriteBatch.Draw(flat, underGlowTarget, null, new Color(255, 40, 40) * (float)Math.Sin(prog * 3.14f) * 0.7f, 0, origin, 0, 0);
			spriteBatch.Draw(glow, underGlowTarget, underGlowSource, new Color(255, 80, 80) * (float)Math.Sin(prog * 3.14f), 1.57f, origin, SpriteEffects.FlipVertically, 0);
			spriteBatch.Draw(glow, flashGlowTarget, underGlowSource, new Color(255, 120, 120) * (1 - timer / 20f), 1.57f, origin, SpriteEffects.FlipVertically, 0);

			underGlowTarget = new Rectangle((int)basePos.X + 198, (int)basePos.Y + 450, 1000, 100);
			flashGlowTarget = new Rectangle((int)basePos.X + 100 + (int)(timer / 20f * 800), (int)basePos.Y + 450, 1000, (int)(timer / 20f * 800));

			spriteBatch.Draw(flat, underGlowTarget, null, new Color(255, 40, 40) * (float)Math.Sin(prog * 3.14f) * 0.7f, 0, origin, 0, 0);
			spriteBatch.Draw(glow, underGlowTarget, underGlowSource, new Color(255, 80, 80) * (float)Math.Sin(prog * 3.14f), 1.57f, origin, 0, 0);
			spriteBatch.Draw(glow, flashGlowTarget, underGlowSource, new Color(255, 120, 120) * (1 - timer / 20f), 1.57f, origin, 0, 0);

			spriteBatch.Draw(glow, leftTarget, thinSource, new Color(255, 120, 120) * (float)Math.Sin(prog * 3.14f), 0, glow.Size() / 4, 0, 0);
			spriteBatch.Draw(glow, rightTarget, thinSource, new Color(255, 120, 120) * (float)Math.Sin(prog * 3.14f), 0, glow.Size() / 4, 0, 0);

			for (int k = 1; k < 9; k++)
			{
				Vector2 posL = Projectile.Center + new Vector2(-220 + Helpers.Helper.BezierEase(timer) * 40, k * 100) - Main.screenPosition;
				Vector2 posR = Projectile.Center + new Vector2(220 - Helpers.Helper.BezierEase(timer) * 40, k * 100) - Main.screenPosition;

				for (int i = 0; i < 6; i++)
				{
					Color arrowColor = new Color(255, 100, 100) * (float)Math.Sin(prog * 6.28f * 4 - i) * (float)Math.Sin(prog * 3.14f);

					spriteBatch.Draw(arrow, posL + Vector2.UnitX * i * 16, null, arrowColor, 0, arrow.Size() / 2, 1, 0, 0);
					spriteBatch.Draw(arrow, posR - Vector2.UnitX * i * 16, null, arrowColor, 0, arrow.Size() / 2, 1, SpriteEffects.FlipHorizontally, 0);
				}
			}
		}
	}
}
