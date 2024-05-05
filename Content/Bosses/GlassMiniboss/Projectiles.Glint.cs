using System;
using System.IO;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	internal class Glint : ModProjectile
	{
		public static Func<float, float> ease = Helpers.Helper.CubicBezier(0.25f, 0.8f, 0.75f, 0.01f);
		public static Func<float, float> ease2 = Helpers.Helper.CubicBezier(0.35f, 0.7f, 0.85f, 0.01f);

		// Color is applied in this seeming hacky way so that the colors are applied inside of OnSpawn instead of outside using Main.Projectile[] so that they exist prior to the first packet
		// So that we don't require further syncing for these.
		public static Color colorMainToApply;
		public static Color colorSecondaryToApply;

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

		public override void OnSpawn(IEntitySource source)
		{
			colorMain = colorMainToApply;
			colorSecondary = colorSecondaryToApply;
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
			Texture2D tex = Assets.StarTexture.Value;
			Texture2D tex2 = Assets.Misc.GlowRing.Value;
			Texture2D tex3 = Assets.Keys.GlowAlpha.Value;

			Color color1 = colorMain * ease(Projectile.timeLeft / 60f);
			color1.A = 0;

			Color color2 = colorSecondary * ease2(Projectile.timeLeft / 60f);
			color2.A = 0;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color1, Projectile.rotation, tex.Size() / 2f, Projectile.scale * 0.8f, 0, 0);
			Main.spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex2.Size() / 2f, RingScale * 0.2f, 0, 0);
			Main.spriteBatch.Draw(tex3, Projectile.Center - Main.screenPosition, null, color2, Projectile.rotation, tex3.Size() / 2f, RingScale, 0, 0);

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.WriteRGB(colorMain);
			writer.WriteRGB(colorSecondary);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			colorMain = reader.ReadRGB();
			colorSecondary = reader.ReadRGB();
		}

		/// <summary>
		/// Helper method for quickly spawning a glint correctly
		/// </summary>
		/// <param name="pos">Where the glint should be spawned</param>
		/// <param name="main">The color of the star</param>
		/// <param name="secondary">The color of the ring and glow</param>
		/// <returns>The index of the spawned glint</returns>
		public static int SpawnGlint(Vector2 pos, Color main, Color secondary)
		{
			Glint.colorMainToApply = main;
			Glint.colorSecondaryToApply = secondary;
			return Projectile.NewProjectile(null, pos, Vector2.Zero, ModContent.ProjectileType<Glint>(), 0, 0, Main.myPlayer);
		}
	}
}