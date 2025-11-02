using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Bosses.TheThinkerBoss
{
	internal class FleshChunk : ModProjectile
	{
		public override string Texture => AssetDirectory.TheThinkerBoss + Name;

		public ref float Timer => ref Projectile.ai[0];
		public ref float Glow => ref Projectile.ai[1];

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 12;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 240;
			Projectile.hostile = true;
			Projectile.tileCollide = true;
			Projectile.frame = Main.rand.Next(10);
		}

		public override void AI()
		{
			Timer++;

			ProjectileID.Sets.TrailCacheLength[Type] = 12;
			ProjectileID.Sets.TrailingMode[Type] = 2;

			if (Timer <= 20)
				Glow = Eases.EaseCircularIn(Timer / 20f);

			Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.2f, 0.2f));

			if (Main.rand.NextBool(4))
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.PixelatedEmber>(), 0, 0, 0, new Color(255, 60, 60, 0) * Glow, 0.1f);

			Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);

			if (Timer > 60)
				Projectile.velocity.Y += 0.2f;

			Projectile.velocity.X *= 0.99f;

			Projectile.rotation += Projectile.velocity.Length() * 0.01f;
		}

		public override void OnKill(int timeLeft)
		{
			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5), 0, new Color(255, 60, 60, 0), 0.1f);
				Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Scale: 2f);
			}

			SoundEngine.PlaySound(SoundID.NPCDeath1.WithPitchOffset(Main.rand.NextFloat(-0.5f, -0.25f)));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			var tex = Assets.Bosses.TheThinkerBoss.FleshChunk.Value;
			var glow = Assets.Bosses.TheThinkerBoss.FleshChunkGlow.Value;

			Rectangle frame = new(0, 32 * Projectile.frame, 36, 32);

			for(int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 pos = Projectile.oldPos[k];
				float rot = Projectile.oldRot[k];

				float opacity = Glow * (1f - k / (float)Projectile.oldRot.Length);

				Main.spriteBatch.Draw(tex, pos + new Vector2(8, 8) - Main.screenPosition, frame, new Color(255, 100, 100) * 0.5f * opacity, rot, new Vector2(18, 16), Projectile.scale, 0, 0);
			}

			Main.spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, frame, new Color(255, 60, 60, 0) * Glow, Projectile.rotation, new Vector2(18, 16), Projectile.scale * 1.1f, 0, 0);
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, new Vector2(18, 16), Projectile.scale, 0, 0);

			return false;
		}
	}
}
