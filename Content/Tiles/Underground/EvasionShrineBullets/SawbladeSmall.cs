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
	class SawbladeSmall : ModProjectile
	{
		public Vector2 storedVelocity = Vector2.Zero;

		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

		public float Alpha => 1 - projectile.alpha / 255f;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cursed Sawblade");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 20;
			ProjectileID.Sets.TrailingMode[projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			projectile.width = 56;
			projectile.height = 56;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.penetrate = -1;
			projectile.alpha = 255;
		}

		public override void AI()
		{
			if (storedVelocity == Vector2.Zero)
				storedVelocity = projectile.velocity;

			projectile.ai[0]++;

			if (projectile.ai[0] <= 20)
			{
				projectile.velocity = Vector2.SmoothStep(Vector2.Zero, storedVelocity, projectile.ai[0] / 20f);
				projectile.alpha -= 255 / 20;
			}

			else if(projectile.timeLeft <= 20)
			{
				projectile.velocity = Vector2.SmoothStep(Vector2.Zero, storedVelocity, projectile.timeLeft / 20f);
				projectile.alpha += 255 / 20;
			}

			else projectile.alpha = 0;

			projectile.rotation -= 0.1f;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var glowTex = ModContent.GetTexture(Texture + "Glow");
			spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
			var texStar = ModContent.GetTexture("StarlightRiver/Assets/GUI/ItemGlow");

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, projectile.rotation, tex.Size() / 2, 1.8f, 0, 0);

			spriteBatch.Draw(texStar, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255) * Alpha, projectile.rotation * 1.1f, texStar.Size() / 2, 0.5f, 0, 0);
			spriteBatch.Draw(texStar, projectile.Center - Main.screenPosition, null, Color.White * Alpha, projectile.rotation * 1.1f, texStar.Size() / 2, 0.4f, 0, 0);

			for (int k = 0; k < projectile.oldPos.Length; k++)
			{
				Color color = new Color(100, 0, 255) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);

				float scale = 1;

				spriteBatch.Draw(tex, (projectile.oldPos[k] + projectile.Size / 2 + projectile.Center) * 0.5f - Main.screenPosition, null, color * 0.8f * Alpha, 0, tex.Size() / 2, scale, default, default);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return true;
		}
	}
}
