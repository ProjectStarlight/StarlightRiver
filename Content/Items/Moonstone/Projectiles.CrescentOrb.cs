using StarlightRiver.Core.Loaders;
using System;
using Terraria.GameContent;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Moonstone
{
	public class CrescentOrb : ModProjectile
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 2;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			DisplayName.SetDefault("Lunar Orb");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.width = Projectile.height = 64;
			Projectile.timeLeft = 180;
			Projectile.ignoreWater = true;
			Projectile.knockBack = 5;
		}

		public override void AI()
		{
			Projectile.width = Projectile.height = (int)(64 * Projectile.scale);
			Projectile.rotation += Projectile.velocity.X * 0.01f;
			Projectile.velocity.Y += 1f;

			if (Main.rand.NextBool(3))
			{
				Dust.NewDustPerfect(Projectile.TopLeft + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
				ModContent.DustType<Dusts.MoonstoneShimmer>(), new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.2f, 0.4f)), 1,
				new Color(Main.rand.NextFloat(0.25f, 0.30f), Main.rand.NextFloat(0.25f, 0.30f), Main.rand.NextFloat(0.35f, 0.45f), 0f), Main.rand.NextFloat(0.2f, 0.4f));
			}

			Lighting.AddLight(Projectile.Center, new Vector3(0.905f, 0.89f, 1) * Projectile.scale * Projectile.Opacity);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X && Projectile.timeLeft > 10)
				Projectile.timeLeft = 10;

			Projectile.velocity.X = oldVelocity.X;

			return false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Projectile.Opacity = Projectile.timeLeft > 10 ? 1 : Projectile.timeLeft / 10f;

			Texture2D texGlow = Assets.Masks.GlowAlpha.Value;

			int sin = (int)(Math.Sin(StarlightWorld.visualTimer * 3) * 40f);
			var color = new Color(72 + sin, 30 + sin / 2, 127);

			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * Projectile.Opacity, 0, texGlow.Size() / 2, Projectile.scale / 2, default, default);
			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, null, color * Projectile.scale * 1.2f * Projectile.Opacity, 0, texGlow.Size() / 2, Projectile.scale * 0.8f, default, default);

			Effect effect1 = ShaderLoader.GetShader("CrescentOrb").Value;

			if (effect1 != null)
			{
				effect1.Parameters["sampleTexture"].SetValue(Assets.Items.Moonstone.CrescentQuarterstaffMap.Value);
				effect1.Parameters["sampleTexture2"].SetValue(Assets.Bosses.VitricBoss.LaserBallDistort.Value);
				effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.01f);
				effect1.Parameters["opacity"].SetValue(Projectile.Opacity);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, Main.Rasterizer, effect1, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, null, Color.White * Projectile.scale, Projectile.rotation, Vector2.One * 32, Projectile.scale, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, Main.Rasterizer, default, Main.GameViewMatrix.TransformationMatrix);
			}

			var glowColor = new Color(78, 87, 191, 0);
			Main.spriteBatch.Draw(texGlow, Projectile.Center - Main.screenPosition, texGlow.Frame(), glowColor * Projectile.Opacity * 0.8f, 0, texGlow.Size() / 2, 2.5f * Projectile.scale * Projectile.Opacity, 0, 0);

			return false;
		}

		public override bool? CanDamage()
		{
			if (Projectile.timeLeft < 10)
				return false;

			return base.CanDamage();
		}
	}
}