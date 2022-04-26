using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.SteampunkSet;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Misc
{
	public class BizarrePotion : ModItem
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
			Tooltip.SetDefault("Throws a random potion");

		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Generic;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 19;
			Item.useAnimation = 19;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 0;
			Item.rare = ItemRarityID.Blue;
			Item.shoot = ModContent.ProjectileType<BizarrePotionProj>();
			Item.shootSpeed = 12f;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.consumable = true;
			Item.maxStack = 999;
		}
	}

	internal enum BottleType
	{
		Regular = 0,
		Bounce = 1,
		Float = 2,
		Slide = 3
	}

	internal enum LiquidType
	{
		Fire = 0,
		Ice = 1,
		Lightning = 2,
		Poison = 3
	}
	public class BizarrePotionProj : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private bool initialized = false;

		private LiquidType liquidType;

		private BottleType bottleType;

		int bounces = 2;

		private Player owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 24;

			Projectile.aiStyle = -1;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Generic;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int xFrameSize = texture.Width / 4;
			int yFrameSize = texture.Height / 4;

			int xFrame = (int)bottleType;
			int yFrame = (int)liquidType;
			Rectangle frame = new Rectangle(xFrame * xFrameSize, yFrame * yFrameSize, xFrameSize, yFrameSize);

			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, new Vector2(xFrameSize, yFrameSize) / 2, Projectile.scale, SpriteEffects.None, 0f);

			Texture2D glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Main.spriteBatch.Draw(glowTexture, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, new Vector2(xFrameSize, yFrameSize) / 2, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				liquidType = (LiquidType)Main.rand.Next(4);
				//liquidType = LiquidType.Lightning;
				bottleType = (BottleType)Main.rand.Next(4);

				switch (bottleType)
				{
					case BottleType.Float:
						Projectile.velocity.Y += 5;
						break;
					case BottleType.Slide:
						Projectile.velocity /= 1.5f;
						break;
				}
			}

			Lighting.AddLight(Projectile.Center, GetColor().ToVector3() * 0.5f);

			switch (bottleType)
			{
				case BottleType.Regular:
					Projectile.aiStyle = 2;
					break;
				case BottleType.Float:
					Projectile.rotation = 0f;
					Projectile.velocity.Y -= 0.4f;
					break;
				case BottleType.Slide:
					Projectile.rotation = 0f;
					Projectile.velocity.Y += 0.3f;
					break;
				case BottleType.Bounce:
					Projectile.aiStyle = 2;
					break;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (bottleType == BottleType.Slide)
			{
				if (oldVelocity.Y != Projectile.velocity.Y && oldVelocity.X == Projectile.velocity.X)
					return false;
			}
			if (bottleType == BottleType.Bounce)
			{
				if (oldVelocity.Y != Projectile.velocity.Y)
					Projectile.velocity.Y = -oldVelocity.Y * 0.8f;

				if (oldVelocity.X != Projectile.velocity.X)
					Projectile.velocity.X = -oldVelocity.X;

				return (bounces-- <= 0);
			}
			return true;
		}

		public override void Kill(int timeLeft)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item, (int)Projectile.position.X, (int)Projectile.position.Y, 107);
			switch (liquidType)
			{
				case LiquidType.Fire:
					Explode();
					break;
				case LiquidType.Lightning:
					SpawnLightning();
					break;
			}
		}

		private void SpawnLightning()
		{
			var targets = Main.npc.Where(x => x.active && !x.townNPC/*  && !x.immortal && !x.dontTakeDamage&& !x.friendly*/ && x.Distance(Projectile.Center) < 400);
			foreach (NPC target in targets)
			{
				Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BizarreLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI, target.Distance(Projectile.Center));
			}
		}

		private void Explode()
		{
			owner.GetModPlayer<StarlightPlayer>().Shake += 8;

			Terraria.Audio.SoundEngine.PlaySound(SoundLoader.GetLegacySoundSlot(Mod, "Sounds/Magic/FireHit"), Projectile.Center);
			Helper.PlayPitched("Impacts/AirstrikeImpact", 0.4f, Main.rand.NextFloat(-0.1f, 0.1f));

			for (int i = 0; i < 4; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<JetwelderDust>());
				dust.velocity = Main.rand.NextVector2Circular(2, 2);
				dust.scale = Main.rand.NextFloat(1f, 1.5f);
				dust.alpha = Main.rand.Next(80) + 40;
				dust.rotation = Main.rand.NextFloat(6.28f);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(25, 25), ModContent.DustType<CoachGunDustFour>()).scale = 0.9f;
			}

			for (int i = 0; i < 3; i++)
			{
				var velocity = Main.rand.NextFloat(6.28f).ToRotationVector2() * Main.rand.NextFloat(1, 2);
				Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			Projectile.NewProjectileDirect(Projectile.GetProjectileSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), Projectile.damage, 0, owner.whoAmI, -1);
			for (int i = 0; i < 2; i++)
			{
				Vector2 vel = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16) + (vel * Main.rand.Next(20)), 0, 0, ModContent.DustType<JetwelderDustTwo>());
				dust.velocity = vel * Main.rand.Next(2);
				dust.scale = Main.rand.NextFloat(0.3f, 0.7f);
				dust.alpha = 70 + Main.rand.Next(60);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
		}

		private Color GetColor()
		{
			switch (liquidType)
			{
				case LiquidType.Fire:
					return Color.Orange;
				case LiquidType.Ice:
					return Color.Cyan;
				case LiquidType.Lightning:
					return Color.Yellow;
				case LiquidType.Poison:
					return Color.Purple;
			}
			return Color.White;
		}
	}
	public class BizarreLightning : ModProjectile, IDrawPrimitive
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private NPC target => Main.npc[(int)Projectile.ai[0]];

		private float thickness = 2;

		private int fadeTime = 30;

		private int trailLength => (int)MathHelper.Clamp((int)(Projectile.ai[1] / 15), 2, 100);
		private float fade => Projectile.extraUpdates == 0 ? EaseFunction.EaseCubicOut.Ease(Projectile.timeLeft / (float)fadeTime) : 1;

		private List<Vector2> cache;
		private List<Vector2> cache2;
		private Trail trail;
		private Trail trail2;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.extraUpdates = 14;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
		}

        public override void AI()
        {
			if (Projectile.extraUpdates > 0)
				Projectile.velocity = Projectile.DirectionTo(target.Center) * 4;
			else
				Projectile.velocity = Vector2.Zero;
			ManageCaches();
			ManageTrails();
        }

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < trailLength; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			if (Projectile.extraUpdates > 0 && Projectile.timeLeft % 5 == 0)
				cache.Add(Projectile.Center);

			while (cache.Count > trailLength)
            {
                cache.RemoveAt(0);
            }

			if (Projectile.timeLeft % 5 == 0)
			{
				cache2 = new List<Vector2>();
				for (int i = 0; i < cache.Count; i++)
				{
					Vector2 point = cache[i];
					Vector2 nextPoint = i == cache.Count - 1 ? Projectile.Center + Projectile.velocity : cache[i + 1];
					Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);
					if (i > cache.Count - 3 || dir == Vector2.Zero)
						cache2.Add(point);
					else
						cache2.Add(point + (dir * Main.rand.NextFloat(15)));
				}
			}
		}

		private void ManageTrails()
        {
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, trailLength, new TriangularTip(4), factor => thickness * Main.rand.NextFloat(0.75f, 1.25f) * 16, factor =>
			{
				if (factor.X > 0.99f)
					return Color.Transparent;

				return Color.Yellow * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X) * fade;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
			trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, trailLength, new TriangularTip(4), factor => thickness * 3 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.Lerp(Color.Yellow, Color.Red, EaseFunction.EaseCubicIn.Ease(Math.Min(1.2f - progress, 1))) * progress * fade;
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

			Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);
			trail2?.Render(effect);
		}


		public override bool? CanHitNPC(NPC hitting)
        {
			if (Projectile.extraUpdates == 0)
				return false;
			if (target != hitting)
				return false;
            return base.CanHitNPC(hitting);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
			Projectile.extraUpdates = 0;
			Projectile.timeLeft = fadeTime;
        }
    }
}