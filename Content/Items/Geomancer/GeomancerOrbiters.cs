using System;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Geomancer
{
	public abstract class GeoProj : ModProjectile
	{
		protected const float bigScale = 1.4f;
		protected const int STARTOFFSET = 45;
		protected float offsetLerper = 1;

		protected float glowCounter;

		protected float whiteCounter;

		protected float fade = 1;

		protected bool released = false;
		protected float releaseCounter = 0;
		protected float extraSpin = 0f;
		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(16, 16);
			Projectile.penetrate = -1;
		}
		public override Color? GetAlpha(Color lightColor)
		{
			return Color.White;
		}

		public override void AI()
		{
			if (Projectile.scale == bigScale)
				glowCounter += 0.02f;

			SafeAI();
			Projectile.rotation = 0f;
			Projectile.Center = Main.player[Projectile.owner].Center + (Projectile.ai[0] + (float)Main.timeForVisualEffects * 0.025f + extraSpin).ToRotationVector2() * MathHelper.Lerp(0, STARTOFFSET, EaseFunction.EaseCubicOut.Ease(offsetLerper));

			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			Projectile.timeLeft = 2;
			if (modPlayer.DiamondStored && modPlayer.RubyStored && modPlayer.EmeraldStored && modPlayer.SapphireStored && modPlayer.TopazStored && modPlayer.AmethystStored || released)
			{
				if (whiteCounter < 1)
					whiteCounter += 0.007f;

				releaseCounter += 0.01f;
				extraSpin += Math.Min(releaseCounter, 0.15f);
				released = true;

				if (releaseCounter > 0.5f)
					offsetLerper -= 0.015f;

				if (offsetLerper <= 0)
				{
					Destroy();
					Projectile.active = false;
					modPlayer.AmethystStored = false;
					modPlayer.TopazStored = false;
					modPlayer.EmeraldStored = false;
					modPlayer.SapphireStored = false;
					modPlayer.RubyStored = false;
					modPlayer.DiamondStored = false;

					modPlayer.storedGem = StoredGem.All;
					modPlayer.allTimer = 400;

					for (int i = 0; i < 3; i++)
					{
						float angle = Main.rand.NextFloat(6.28f);
						var dust = Dust.NewDustPerfect(Main.player[Projectile.owner].Center + angle.ToRotationVector2() * 20, ModContent.DustType<GeoRainbowDust>());
						dust.scale = 1f;
						dust.velocity = angle.ToRotationVector2() * Main.rand.NextFloat() * 4;
					}
				}
			}

			if (!modPlayer.SetBonusActive)
				Projectile.active = false;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			if (Projectile.scale == bigScale)
			{
				float progress = glowCounter % 1;
				float transparency = (float)Math.Pow(1 - progress, 2);
				float scale = 0.95f + progress;

				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Color.White * transparency, Projectile.rotation, tex.Size() / 2, scale * Projectile.scale, SpriteEffects.None, 0f);
			}

			float progress2 = 1 - fade;
			float transparency2 = (float)Math.Pow(1 - progress2, 2);
			float scale2 = 0.95f + progress2;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Color.White * transparency2, Projectile.rotation, tex.Size() / 2, Projectile.scale * scale2, SpriteEffects.None, 0f);
			return false;
		}

		public override void PostDraw(Color lightColor)
		{
			if (!released)
				return;
			Texture2D tex = ModContent.Request<Texture2D>(Texture + "_White").Value;

			float progress2 = 1 - fade;
			float transparency2 = (float)Math.Pow(1 - progress2, 2);
			float scale2 = 0.95f + progress2;
			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + new Vector2(0, Projectile.gfxOffY), null, Color.White * whiteCounter * transparency2, Projectile.rotation, tex.Size() / 2, Projectile.scale * scale2, SpriteEffects.None, 0f);
		}

		protected virtual void SafeAI() { }

		protected virtual void Destroy() { }
	}

	public class GeoAmethystProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoAmethyst";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();

			if (modPlayer.storedGem == StoredGem.Amethyst && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.AmethystStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.75f;
			}
		}
	}

	public class GeoRubyProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoRuby";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			if (modPlayer.storedGem == StoredGem.Ruby && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.RubyStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.75f;
			}
		}
	}

	public class GeoSapphireProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoSapphire";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			if (modPlayer.storedGem == StoredGem.Sapphire && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.SapphireStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.75f;
			}
		}
	}

	public class GeoEmeraldProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoEmerald";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			if (modPlayer.storedGem == StoredGem.Emerald && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.EmeraldStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.75f;
			}
		}
	}

	public class GeoTopazProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoTopaz";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			if (modPlayer.storedGem == StoredGem.Topaz && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.TopazStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.88f;
			}
		}

		protected override void Destroy()
		{
			Player Player = Main.player[Projectile.owner];
		}
	}

	public class GeoDiamondProj : GeoProj
	{
		public override string Texture => AssetDirectory.GeomancerItem + "GeoDiamond";

		protected override void SafeAI()
		{
			GeomancerPlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<GeomancerPlayer>();
			if (modPlayer.storedGem == StoredGem.Diamond && !released)
			{
				fade = Math.Min(1, modPlayer.timer / 40f);
				if (modPlayer.timer == 1)
				{
					modPlayer.DiamondStored = false;
					Projectile.active = false;
					GeomancerPlayer.PickOldGem(Main.player[Projectile.owner]);
					modPlayer.timer = 1200;
				}

				Projectile.scale = bigScale;
			}
			else
			{
				Projectile.scale = 0.75f;
			}
		}
	}
}