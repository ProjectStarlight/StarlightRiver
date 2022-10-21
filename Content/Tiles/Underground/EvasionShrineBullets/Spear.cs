using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Tiles.Underground.EvasionShrineBullets
{
	class Spear : ModProjectile
	{
		public Vector2 startPoint;
		public Vector2 endPoint;
		public int timeToRise;
		public int timeToRetract;
		public int teleTime;
		public int holdTime;
		public EvasionShrineDummy parent;

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

		public float Alpha => 1 - Projectile.alpha / 255f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Spear");
		}

		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 36;
			Projectile.hostile = true;
			Projectile.timeLeft = 3;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.alpha = 255;
		}

		public override void AI()
		{
			if (startPoint == Vector2.Zero)
			{
				Projectile.timeLeft = timeToRise + timeToRetract + teleTime + holdTime;
				startPoint = Projectile.Center;
				Projectile.rotation = (startPoint - endPoint).ToRotation() - 1.57f;
			}

			int timer = timeToRise + timeToRetract + teleTime + holdTime - Projectile.timeLeft;

			if (timer > teleTime && Projectile.alpha > 0)
				Projectile.alpha -= 15;

			if (Projectile.timeLeft < 20)
				Projectile.alpha += 15;

			if (timer < teleTime)
				startPoint = Projectile.Center;
			else if (timer < teleTime + timeToRise)
				Projectile.Center = Vector2.SmoothStep(startPoint, endPoint, (timer - teleTime) / (float)timeToRise);
			else if (timer < teleTime + timeToRise + holdTime)
				Projectile.Center = endPoint;
			else
				Projectile.Center = Vector2.SmoothStep(endPoint, startPoint, (timer - timeToRise - teleTime - holdTime) / (float)timeToRetract);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			int timer = timeToRise + timeToRetract + teleTime + holdTime - Projectile.timeLeft;
			bool line = Collision.CheckAABBvLineCollision(targetHitbox.Location.ToVector2(), targetHitbox.Size(), startPoint, Projectile.Center);

			if (line && timer > teleTime)
				return true;

			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			parent.lives--;

			if (Main.rand.Next(10000) == 0)
				Main.NewText("Skill issue.");
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "Glow").Value;
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, Projectile.rotation, glowTex.Size() / 2, 1, 0, 0);

			float dist = Vector2.Distance(Projectile.Center, startPoint);
			Texture2D bodyTex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Underground/SpearBody").Value;
			for (int k = bodyTex.Height; k < dist; k += bodyTex.Height)
			{
				var pos = Vector2.Lerp(Projectile.Center, startPoint, k / dist);
				Main.spriteBatch.Draw(bodyTex, pos - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, Projectile.rotation, bodyTex.Size() / 2, 1, 0, 0);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			int timer = timeToRise + timeToRetract + teleTime + holdTime - Projectile.timeLeft;

			if (timer > teleTime)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall").Value;
				Texture2D tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;

				float opacity;

				if (timer < teleTime + timeToRise)
					opacity = (timer - teleTime) / (float)timeToRise;

				else if (timer < teleTime + timeToRise + holdTime)
					opacity = 1;

				else
					opacity = 1 - (timer - timeToRise - teleTime - holdTime) / (float)timeToRetract;

				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(150, 50, 255), Projectile.rotation + 3.14f, new Vector2(tex.Width / 2, 70), 2.8f * opacity, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation + 3.14f, new Vector2(tex.Width / 2, 70), 2.2f * opacity, 0, 0);

				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), Projectile.rotation, new Vector2(tex.Width / 2, 60), 2.2f * opacity, 0, 0);
				spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(tex.Width / 2, 60), 1.5f * opacity, 0, 0);

				spriteBatch.Draw(tex2, Projectile.Center - Main.screenPosition, null, new Color(150, 50, 255) * opacity, Projectile.rotation, tex2.Size() / 2, 1.5f, 0, 0);
			}
			else
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;
				float opacity = (float)Math.Sin(timer / (float)teleTime * 3.14f) * 0.5f;

				Vector2 pos = Projectile.Center - Main.screenPosition;
				float dist = Vector2.Distance(Projectile.Center, endPoint) - 4;
				var target = new Rectangle((int)pos.X, (int)pos.Y, (int)dist, 40);

				spriteBatch.Draw(tex, target, null, new Color(150, 150, 155) * opacity * 0.5f, Projectile.rotation + 1.57f, new Vector2(tex.Width, tex.Height / 2), 0, 0);
				target.Height = 16;
				spriteBatch.Draw(tex, target, null, Color.White * opacity * 0.7f, Projectile.rotation + 1.57f, new Vector2(tex.Width, tex.Height / 2), 0, 0);

				Texture2D tex2 = ModContent.Request<Texture2D>(Texture + "Glow").Value;
				spriteBatch.Draw(tex2, endPoint - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, new Vector2(tex2.Width / 2, tex2.Height - 5), 1.0f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return true;
		}
	}
}
