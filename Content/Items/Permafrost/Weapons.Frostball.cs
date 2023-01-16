using System;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Permafrost
{
	internal class Frostball : ModItem
	{
		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Frostball");
			Tooltip.SetDefault("Strike enemies to build aurora power\n" +
				"Heals enemies near the yoyo based on aurora power\n" +
				"Gives you regeneration when retrieved based on aurora power");

			ItemID.Sets.Yoyo[Item.type] = true;
			ItemID.Sets.GamepadExtraRange[Item.type] = 15;
			ItemID.Sets.GamepadSmartQuickReach[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 15;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.crit = 4;
			Item.knockBack = 1;
			Item.useTime = 30;
			Item.useAnimation = 30;

			Item.rare = ItemRarityID.Green;
			Item.value = 10000;

			Item.shoot = ModContent.ProjectileType<FrostballProjectile>();
			Item.shootSpeed = 14;
			Item.channel = true;

			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
		}
	}

	internal class FrostballProjectile : ModProjectile
	{
		private int auroraPower;

		private int visualTimer;

		private int AuroraRadius => auroraPower * 6;

		private float AuroraPercent => auroraPower / 7f;

		public override string Texture => AssetDirectory.PermafrostItem + Name;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 25f;
			ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 300f;
			ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 8f;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = ProjAIStyleID.Yoyo;

			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.penetrate = -1;
		}

		public override void PostAI()
		{
			visualTimer++;

			Lighting.AddLight(Projectile.Center, GetAuroraColor(0).ToVector3() * AuroraPercent * 0.5f);

			foreach (Player player in Main.player)
			{
				if (Helpers.Helper.CheckCircularCollision(Projectile.Center, AuroraRadius, player.Hitbox))
				{
					player.lifeRegen += auroraPower;
				}
			}
		}

		public override void Kill(int timeLeft)
		{
			Player player = Main.player[Projectile.owner];
			player.AddBuff(BuffID.Regeneration, auroraPower * 120);

			if (auroraPower > 2)
			{
				for (int k = 0; k < 20; k++)
				{
					Dust.NewDustPerfect(player.Center, ModContent.DustType<Dusts.GlowLine>(), Main.rand.NextVector2Circular(8, 8), 0, GetAuroraColor(0), Main.rand.NextFloat());
				}
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			if (auroraPower < 7)
				auroraPower++;
		}

		private Color GetAuroraColor(float offset)
		{
			float time = Main.GameUpdateCount / 300f * 6.28f;

			float sin2 = (float)Math.Sin(time + offset * 0.2f * 0.2f);
			float cos = (float)Math.Cos(time + offset * 0.2f);
			var color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);

			if (color.R < 80)
				color.R = 80;

			if (color.G < 80)
				color.G = 80;

			return color;
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowAlpha").Value;

			Effect effect = StarlightRiver.Instance.Assets.Request<Effect>("Effects/FrostAura").Value;

			effect.Parameters["drawTexture"].SetValue(tex);
			effect.Parameters["noiseTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Noise/SwirlyNoiseLooping").Value);

			effect.Parameters["time"].SetValue(visualTimer / 250f);
			effect.Parameters["incolor"].SetValue(GetAuroraColor(0).ToVector3() * AuroraPercent);
			effect.Parameters["width"].SetValue(2);
			effect.Parameters["height"].SetValue(2);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * AuroraPercent, 0, tex.Size() / 2f, 1.6f * AuroraPercent, 0, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

			for (int k = 0; k < 3; k++)
			{
				Texture2D texStar = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/StarAlpha").Value;

				Color color = GetAuroraColor(k / 3f * 1.5f) * AuroraPercent;
				color.A = 0;

				Color white = Color.White * AuroraPercent;
				white.A = 0;

				float rot = visualTimer * (0.03f + k * 0.005f) + k / 3f * 2.7f;
				Vector2 pos = Projectile.Center + Vector2.One.RotatedBy(rot) * (k + 1) * 14 * AuroraPercent - Main.screenPosition;

				Main.spriteBatch.Draw(texStar, pos, null, color * ((1f - k / 3f) * AuroraPercent * 0.5f), 0, texStar.Size() / 2, (0.25f + k * 0.04f) * AuroraPercent, 0, 0);
				Main.spriteBatch.Draw(texStar, pos, null, white * ((1f - k / 3f) * AuroraPercent), 0, texStar.Size() / 2, (0.08f + k * 0.02f) * AuroraPercent, 0, 0);

				float rot2 = -visualTimer * (0.022f + k * 0.007f) + k / 3f * 2.7f;
				Vector2 pos2 = Projectile.Center + Vector2.One.RotatedBy(rot2) * (k + 1) * 12 * AuroraPercent - Main.screenPosition;

				Main.spriteBatch.Draw(texStar, pos2, null, color * ((1f - k / 3f) * AuroraPercent * 0.5f), 0, texStar.Size() / 2, (0.25f + k * 0.04f) * AuroraPercent, 0, 0);
				Main.spriteBatch.Draw(texStar, pos2, null, white * ((1f - k / 3f) * AuroraPercent), 0, texStar.Size() / 2, (0.08f + k * 0.02f) * AuroraPercent, 0, 0);
			}

			base.PostDraw(lightColor);
		}
	}
}
