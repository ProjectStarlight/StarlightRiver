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
		public override string Texture => AssetDirectory.Assets + "Tiles/Underground/" + Name;

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
		}

		public override void AI()
		{
			projectile.rotation -= 0.1f;

			//Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), ModContent.DustType<Dusts.Shadow>(), Vector2.UnitY * -1, 0, Color.Black);
		}

		public override void PostDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			var glowTex = ModContent.GetTexture(Texture + "Glow");
			spriteBatch.Draw(glowTex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), projectile.rotation, glowTex.Size() / 2, 1, 0, 0);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
			var texStar = ModContent.GetTexture("StarlightRiver/Assets/GUI/ItemGlow");

			spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), projectile.rotation, tex.Size() / 2, 1.8f, 0, 0);

			spriteBatch.Draw(texStar, projectile.Center - Main.screenPosition, null, new Color(100, 0, 255), projectile.rotation * 1.1f, texStar.Size() / 2, 0.5f, 0, 0);
			spriteBatch.Draw(texStar, projectile.Center - Main.screenPosition, null, Color.White, projectile.rotation * 1.1f, texStar.Size() / 2, 0.4f, 0, 0);

			for (int k = 0; k < projectile.oldPos.Length; k++)
			{
				Color color = new Color(100, 0, 255) * ((projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);

				float scale = 1;

				spriteBatch.Draw(tex, (projectile.oldPos[k] + projectile.Size / 2 + projectile.Center) * 0.5f - Main.screenPosition, null, color * 0.8f, 0, tex.Size() / 2, scale, default, default);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			return true;
		}
	}
}
