using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.NPCs.BossRush
{
	internal class BossRushOrbGoreProjectile : ModProjectile
	{
		int type = 1;

		public override string Texture => "StarlightRiver/Assets/NPCs/BossRush/Gore/Rock1";

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.timeLeft = 420;
			Projectile.damage = -1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			type = Main.rand.Next(6) + 1;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.2f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity.X *= 0.95f;

			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.2f;
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Color.Cyan;
			color.R += 128;

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/NPCs/BossRush/Gore/Rock" + type.ToString()).Value;
			Texture2D glow = ModContent.Request<Texture2D>("StarlightRiver/Assets/NPCs/BossRush/Gore/RockGlow" + type.ToString()).Value;
			Texture2D bloom = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() * 0.5f, 1, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Black, color, (Projectile.timeLeft - 240) / 180f), Projectile.rotation, tex.Size() * 0.5f, 1, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Black, color, (Projectile.timeLeft - 240) / 180f), Projectile.rotation, glow.Size() * 0.5f, 1, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(bloom, Projectile.Center - Main.screenPosition, null, Color.Lerp(Color.Black, color, (Projectile.timeLeft - 240) / 180f) * 0.5f, Projectile.rotation, bloom.Size() * 0.5f, 1, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
				Dust.NewDustPerfect(Projectile.Center, DustID.Obsidian);
		}
	}
}