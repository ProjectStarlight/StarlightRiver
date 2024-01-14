using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets
{
	public class SawbladeSmall : EvasionProjectile
	{
		public static int timeLeftToAssign;

		public Vector2 storedVelocity = Vector2.Zero;
		public EvasionShrineDummy parent;

		public float timer = 0;

		public float Alpha => 1 - Projectile.alpha / 255f;

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Sawblade");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.width = 56;
			Projectile.height = 56;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.alpha = 255;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = timeLeftToAssign;
		}

		public override void AI()
		{
			if (storedVelocity == Vector2.Zero)
				storedVelocity = Projectile.velocity;

			timer++;

			if (timer <= 20)
			{
				Projectile.velocity = Vector2.SmoothStep(Vector2.Zero, storedVelocity, timer / 20f);
				Projectile.alpha -= 255 / 20;
			}
			else if (Projectile.timeLeft <= 20)
			{
				Projectile.velocity = Vector2.SmoothStep(Vector2.Zero, storedVelocity, Projectile.timeLeft / 20f);
				Projectile.alpha += 255 / 20;
			}
			else
			{
				Projectile.alpha = 0;
			}

			Projectile.rotation -= 0.1f;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, Projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
			Texture2D texStar = ModContent.Request<Texture2D>("StarlightRiver/Assets/GUI/ItemGlow").Value;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, Projectile.rotation, tex.Size() / 2, 1.8f, 0, 0);

			spriteBatch.Draw(texStar, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, Projectile.rotation * 1.1f, texStar.Size() / 2, 0.5f, 0, 0);
			spriteBatch.Draw(texStar, Projectile.Center - Main.screenPosition, null, Color.White * Alpha, Projectile.rotation * 1.1f, texStar.Size() / 2, 0.4f, 0, 0);

			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Color color = new Color(100, 0, 255) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);

				float scale = 1;

				spriteBatch.Draw(tex, (Projectile.oldPos[k] + Projectile.Size / 2 + Projectile.Center) * 0.5f - Main.screenPosition, null, color * 0.8f * Alpha, 0, tex.Size() / 2, scale, default, default);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			return true;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Projectile.timeLeft);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			Projectile.timeLeft = reader.ReadInt32();
		}
	}
}