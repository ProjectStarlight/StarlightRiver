using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.VitricBoss
{
	internal class GlassVolley : ModProjectile
	{
		public ref float Timer => ref Projectile.ai[0];
		public ref float Rotation => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.hostile = false;
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.timeLeft = 2;
		}

		public override void AI()
		{
			Projectile.timeLeft = 2;
			Projectile.rotation = Rotation;
			Timer++; //ticks up the timer

			if (Timer >= 30) //when this Projectile goes off
			{
				for (int k = 0; k < 8; k++)
				{
					if (Timer == 30 + k * 3)
					{
						float rot = (k - 4) / 10f; //rotational offset

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation + rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the flurry of Projectiles

							if (Main.masterMode)
								Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, new Vector2(-9.5f, 0).RotatedBy(Projectile.rotation - rot), ProjectileType<GlassVolleyShard>(), 20, 0); //fire the second flury in master
						}

						Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact, Projectile.Center);
					}
				}
			}

			if (Timer == 50)
				Projectile.Kill(); //kill it when it expires
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Timer <= 30) //draws the projectile's tell ~0.75 seconds before it goes off
			{
				Texture2D tex = Assets.Bosses.VitricBoss.VolleyTell.Value;
				float alpha = (float)Math.Sin(Timer / 30f * 3.14f) * 0.4f;
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), new Color(100, 100 + (int)(155 * alpha), 255 - (int)(155 * alpha), 0) * alpha, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
			}

			return false;
		}
	}

	public class GlassVolleyShard : ModProjectile
	{
		public Vector2 homePos;

		public override string Texture => AssetDirectory.VitricBoss + Name;

		public override void SetDefaults()
		{
			Projectile.hostile = true;
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.timeLeft = 600;
			Projectile.scale = 0.5f;
			Projectile.extraUpdates = 3;
		}

		public override void AI()
		{
			if (Projectile.timeLeft == 599)
				homePos = Projectile.Center;

			if (Projectile.timeLeft > 570)
				Projectile.velocity *= 0.96f;

			if (Projectile.timeLeft < 500)
				Projectile.velocity *= 1.03f;

			Projectile.rotation = Projectile.velocity.ToRotation() + 1.58f;

			Color color2 = Helpers.CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(600 - Projectile.timeLeft, 120));
			var color = Color.Lerp(new Color(100, 145, 200), color2, color2.R / 255f);

			color.A = 0;

			if (Main.rand.NextBool(5))
			{
				var swirl = Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.PixelatedEmber>(), Vector2.Normalize(Projectile.velocity).RotatedByRandom(0.5f) * Main.rand.NextFloat(), 0, color, Main.rand.NextFloat(0.1f, 0.2f));
			}

			Lighting.AddLight(Projectile.Center, color.ToVector3());
		}

		public override void OnKill(int timeLeft)
		{
			for (int k = 0; k < 8; k++)
			{
				Vector2 vel = Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10f);
				var swirl = Dust.NewDustPerfect(Projectile.Center + vel, DustType<Dusts.PixelatedImpactLineDust>(), vel, 0, new Color(100, 145, 200, 0), Main.rand.NextFloat(0.15f, 0.2f));
				//swirl.customData = Projectile.Center;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Color color = Helpers.CommonVisualEffects.HeatedToCoolColor(MathHelper.Min(600 - Projectile.timeLeft, 120));

			Texture2D glow = Assets.Masks.GlowSoft.Value;

			Color bloomColor = color;
			bloomColor.A = 0;

			Main.EntitySpriteDraw(glow, Projectile.Center - Main.screenPosition, null, bloomColor, Projectile.rotation * 0.2f, glow.Size() * 0.5f, 1 - Projectile.timeLeft / 600f * 0.75f, SpriteEffects.None, 0);

			Main.EntitySpriteDraw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 32, 128), lightColor, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);
			Main.EntitySpriteDraw(Request<Texture2D>(Texture).Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 128, 32, 128), color, Projectile.rotation, new Vector2(16, 64), Projectile.scale, 0, 0);

			Texture2D tell = Assets.Masks.GlowHarsh.Value;
			Texture2D trail = Assets.GlowTrailOneEnd.Value;
			float tellLength = Helpers.Eases.BezierEase(1 - (Projectile.timeLeft - 570) / 30f) * 18f;

			color = Color.Lerp(new Color(150, 225, 255), color, color.R / 255f);
			color.A = 0;

			float trailLength = Projectile.velocity.Length() / trail.Width * 25;

			if (Projectile.timeLeft > 595)
				trailLength = 0;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("OverPlayers", () =>
			{
				Main.EntitySpriteDraw(trail, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation + 1.57f, trail.Size() * new Vector2(0f, 0.5f), new Vector2(trailLength, 0.05f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(trail, Projectile.Center - Main.screenPosition, null, color * 0.5f, Projectile.rotation + 1.57f, trail.Size() * new Vector2(0f, 0.5f), new Vector2(trailLength, 0.15f), SpriteEffects.None, 0);

				Main.EntitySpriteDraw(tell, Projectile.Center - Main.screenPosition, null, bloomColor * 0.1f, Projectile.rotation, tell.Size() * new Vector2(0.5f, 0.75f), new Vector2(0.18f, tellLength), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(tell, Projectile.Center - Main.screenPosition, null, bloomColor * 0.2f, Projectile.rotation, tell.Size() * new Vector2(0.5f, 0.75f), new Vector2(0.05f, tellLength), SpriteEffects.None, 0);
			});

			return false;
		}
	}
}