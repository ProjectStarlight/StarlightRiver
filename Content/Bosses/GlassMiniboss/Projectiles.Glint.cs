using System;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class Glint : ModProjectile
	{
		public static Func<float, float> ease = Helpers.Helper.CubicBezier(0.25f, 0.8f, 0.75f, 0.01f);
		public static Func<float, float> ease2 = Helpers.Helper.CubicBezier(0.35f, 0.7f, 0.85f, 0.01f);

		public Color colorMain;
		public Color colorSecondary;

		public ref float Timer => ref Projectile.ai[0];
		public ref float RingScale => ref Projectile.ai[1];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 60;

			ease = Helpers.Helper.CubicBezier(0.25f, 0.8f, 0.75f, 0.01f);
			ease2 = Helpers.Helper.CubicBezier(0.05f, 0.5f, 0.55f, 0.1f);
		}

		public override void AI()
		{
			Timer++;

			Projectile.rotation += 0.1f;

			Projectile.scale = ease(Projectile.timeLeft / 60f);
			RingScale = ease2(Projectile.timeLeft / 60f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/StarTexture").Value;
			Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/GlowRing").Value;
			Texture2D tex3 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			Color color1 = colorMain * ease(Projectile.timeLeft / 60f);
			color1.A = 0;

			Color color2 = colorSecondary * ease2(Projectile.timeLeft / 60f);
			color2.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color1, Projectile.rotation, tex.Size() / 2f, Projectile.scale * 0.8f, 0, 0);
			Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex2.Size() / 2f, RingScale * 0.2f, 0, 0);
			Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex3.Size() / 2f, RingScale, 0, 0);

			return false;
		}

		/// <summary>
		/// Helper method for quickly spawning a glint correctly
		/// </summary>
		/// <param name="pos">Where the glint should be spawned</param>
		/// <param name="main">The color of the star</param>
		/// <param name="secondary">The color of the ring and glow</param>
		/// <returns>The index of the spawned glint, or -1 if one failed to spawn correctly</returns>
		public static int SpawnGlint(Vector2 pos, Color main, Color secondary)
		{
			int index = Projectile.NewProjectile(null, pos, Vector2.Zero, ModContent.ProjectileType<Glint>(), 0, 0, Main.myPlayer);
			ModProjectile mp = Main.projectile[index].ModProjectile;

			if (mp is Glint glint)
			{
				glint.colorMain = main;
				glint.colorSecondary = secondary;
				return index;
			}

			return -1;
		}
	}
}
