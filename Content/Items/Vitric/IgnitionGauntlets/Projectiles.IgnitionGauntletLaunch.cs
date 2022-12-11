using System;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric.IgnitionGauntlets
{
	public class IgnitionGauntletLaunch : ModProjectile
	{
		public float noiseRotation;

		public float noiseRotation2;

		private Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
			On.Terraria.Main.DrawDust += DrawCone;
		}

		public override void Unload()
		{
			On.Terraria.Main.DrawDust -= DrawCone;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Ignition Gauntlets");
		}

		public override void SetDefaults()
		{
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hostile = false;
			Projectile.friendly = true;
			Projectile.timeLeft = 9999;
			Projectile.width = Projectile.height = 50;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Projectile.hide = true;
		}

		public override void AI()
		{
			if (noiseRotation < 0.02f)
				noiseRotation = Main.rand.NextFloat(6.28f);

			noiseRotation += 0.12f;

			if (noiseRotation2 < 0.02f)
				noiseRotation2 = Main.rand.NextFloat(6.28f);

			noiseRotation2 -= 0.12f;

			IgnitionPlayer modPlayer = Owner.GetModPlayer<IgnitionPlayer>();
			Projectile.Center = Owner.Center;

			Projectile.rotation = Owner.fullRotation;

			if (!modPlayer.launching)
				Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}

		private void DrawCone(On.Terraria.Main.orig_DrawDust orig, Main self) //putting this here so I dont have to load another detour to get it to load in front of the fist
		{
			orig(self);

			//putting this here so I dont have to load another detour to get it to load in front of the fist
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			Main.spriteBatch.End();

			Color color = Color.OrangeRed;
			color.A = 0;

			foreach (Projectile Projectile in Main.projectile)
			{
				Player player = Main.player[Projectile.owner];
				Texture2D starTex = ModContent.Request<Texture2D>(Texture + "_Star").Value;

				if (Projectile.type == ModContent.ProjectileType<IgnitionGauntletLaunch>() && Projectile.active && player.GetModPlayer<IgnitionPlayer>().loadedCharge > 15)
				{
					var mp = Projectile.ModProjectile as IgnitionGauntletLaunch;
					Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
					Effect effect = Filters.Scene["ConicalNoise"].GetShader().Shader;
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

					effect.Parameters["vnoise"].SetValue(ModContent.Request<Texture2D>(Texture + "_noise").Value);
					effect.Parameters["rotation"].SetValue(mp.noiseRotation);
					effect.Parameters["transparency"].SetValue(1f);
					effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_pallette").Value);
					effect.Parameters["color"].SetValue(Color.White.ToVector4());
					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(tex, Projectile.Center + (Projectile.rotation - 1.57f).ToRotationVector2() * 30 - Main.screenPosition, null, color, Projectile.rotation - 1.57f, new Vector2(250, 64), new Vector2(0.4f, 0.4f), SpriteEffects.None, 0f);

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

					effect.Parameters["vnoise"].SetValue(ModContent.Request<Texture2D>(Texture + "_noise").Value);
					effect.Parameters["rotation"].SetValue(mp.noiseRotation2);
					effect.Parameters["transparency"].SetValue(1f);
					effect.Parameters["pallette"].SetValue(ModContent.Request<Texture2D>(Texture + "_pallette").Value);
					effect.Parameters["color"].SetValue(Color.White.ToVector4());
					effect.CurrentTechnique.Passes[0].Apply();

					Main.spriteBatch.Draw(tex, Projectile.Center + (Projectile.rotation - 1.57f).ToRotationVector2() * 30 - Main.screenPosition, null, color, Projectile.rotation - 1.57f, new Vector2(250, 64), new Vector2(0.4f, 0.4f), SpriteEffects.None, 0f);

					Main.spriteBatch.End();

					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
					var handOffset = new Vector2(-10 * player.direction, -16);
					handOffset = handOffset.RotatedBy(player.fullRotation);
					Main.spriteBatch.Draw(starTex, player.Center + handOffset - Main.screenPosition, null, new Color(255, 255, 255, 0) * MathHelper.Min(1, player.GetModPlayer<IgnitionPlayer>().loadedCharge / 15f), Main.GameUpdateCount * 0.085f, starTex.Size() / 2, 0.5f + 0.07f * (float)Math.Sin(Main.GameUpdateCount * 0.285f), SpriteEffects.None, 0f);
					Main.spriteBatch.End();
				}
			}
		}
	}
}