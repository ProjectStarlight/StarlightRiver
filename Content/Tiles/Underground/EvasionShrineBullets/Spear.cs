using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

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

		public float Alpha => 1 - projectile.alpha / 255f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Spear");
		}

		public override void SetDefaults()
		{
			projectile.width = 20;
			projectile.height = 36;
			projectile.hostile = true;
			projectile.timeLeft = 3;
			projectile.penetrate = -1;
			projectile.tileCollide = false;
			projectile.alpha = 255;
		}

		public override void AI()
		{
			if (startPoint == Vector2.Zero)
			{
				projectile.timeLeft = timeToRise + timeToRetract + teleTime + holdTime;
				startPoint = projectile.Center;
				projectile.rotation = (startPoint - endPoint).ToRotation() - 1.57f;
			}

			int timer = timeToRise + timeToRetract + teleTime + holdTime - projectile.timeLeft;

			if (timer > teleTime && projectile.alpha > 0)
				projectile.alpha -= 15;

			if (projectile.timeLeft < 20)
				projectile.alpha += 15;

			if (timer < teleTime)
				startPoint = projectile.Center;
			else if (timer < teleTime + timeToRise)
				projectile.Center = Vector2.SmoothStep(startPoint, endPoint, (timer - teleTime) / (float)timeToRise);
			else if (timer < teleTime + timeToRise + holdTime)
				projectile.Center = endPoint;
			else
				projectile.Center = Vector2.SmoothStep(endPoint, startPoint, (timer - timeToRise - teleTime - holdTime) / (float)timeToRetract);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			int timer = timeToRise + timeToRetract + teleTime + holdTime - projectile.timeLeft;
			bool line = Helpers.Helper.CheckLinearCollision(startPoint, projectile.Center, targetHitbox, out Vector2 intersect);

			if (line && timer > teleTime)
				return true;

			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			parent.lives--;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var glowTex = ModContent.GetTexture(Texture + "Glow");
			spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, projectile.rotation, glowTex.Size() / 2, 1, 0, 0);

			var dist = Vector2.Distance(projectile.Center, startPoint);
			var bodyTex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Underground/SpearBody");
			for(int k = bodyTex.Height; k < dist; k += bodyTex.Height)
			{
				var pos = Vector2.Lerp(projectile.Center, startPoint, k / dist);
				spriteBatch.Draw(bodyTex, pos - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, projectile.rotation, bodyTex.Size() / 2, 1, 0, 0);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			int timer = timeToRise + timeToRetract + teleTime + holdTime - projectile.timeLeft;

			if (timer > teleTime)
			{
				var tex = ModContent.GetTexture("StarlightRiver/Assets/Tiles/Moonstone/GlowSmall");
				var tex2 = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");

				float opacity;

				if (timer < teleTime + timeToRise)
					opacity = (timer - teleTime) / (float)timeToRise;

				else if (timer < teleTime + timeToRise + holdTime)
					opacity = 1;

				else 
					opacity = 1 - (timer - timeToRise - teleTime - holdTime) / (float)timeToRetract;

				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(150, 50, 255), projectile.rotation + 3.14f, new Vector2(tex.Width / 2, 70), 2.8f * opacity, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation + 3.14f, new Vector2(tex.Width / 2, 70), 2.2f * opacity, 0, 0);

				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), projectile.rotation, new Vector2(tex.Width / 2, 60), 2.2f * opacity, 0, 0);
				spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation, new Vector2(tex.Width / 2, 60), 1.5f * opacity, 0, 0);

				spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, new Color(150, 50, 255) * opacity, projectile.rotation, tex2.Size() / 2, 1.5f, 0, 0);
			}
			else
			{
				var tex = ModContent.GetTexture("StarlightRiver/Assets/GlowTrail");
				var opacity = (float)Math.Sin(timer / (float)teleTime * 3.14f) * 0.5f;

				var pos = projectile.Center - Main.screenPosition;
				var dist = Vector2.Distance(projectile.Center, endPoint) - 4;
				Rectangle target = new Rectangle((int)pos.X, (int)pos.Y, (int)dist, 40);

				spriteBatch.Draw(tex, target, null, new Color(150, 150, 155) * opacity * 0.5f, projectile.rotation + 1.57f, new Vector2(tex.Width, tex.Height / 2), 0, 0);
				target.Height = 16;
				spriteBatch.Draw(tex, target, null, Color.White * opacity * 0.7f, projectile.rotation + 1.57f, new Vector2(tex.Width, tex.Height / 2), 0, 0);

				var tex2 = ModContent.GetTexture(Texture + "Glow");
				spriteBatch.Draw(tex2, endPoint - Main.screenPosition, null, Color.White * opacity, projectile.rotation, new Vector2(tex2.Width / 2, tex2.Height - 5), 1.0f, 0, 0);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return true;
		}
	}
}
