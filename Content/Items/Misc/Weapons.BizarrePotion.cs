using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.SteampunkSet;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.Audio;
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

		public override void Load()
		{
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "BizarrePotionGore1");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "BizarrePotionGore2");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "BizarrePotionGore3");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "BizarrePotionGore4");
			GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, AssetDirectory.MiscItem + "BizarrePotionGore5");
		}
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
			Tooltip.SetDefault("Throws a random potion with random damaging effects");
		}

		public override void SetDefaults()
		{
			Item.damage = 14;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 24;
			Item.height = 24;
			Item.useTime = 21;
			Item.useAnimation = 21;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.knockBack = 6;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(0, 0, 0, 20);
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
		Launch = 2,
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

		private List<float> oldRotation = new List<float>();

		private bool initialized = false;

		private NPC dontHit = default; //If the bottle hits an NPC,its resulting projectiles shouldnt hurt the hit NPC

		private LiquidType liquidType;

		private BottleType bottleType;

		private int bounces = 2;

		private bool sliding = false;

		private float radiansToSpin = 0f; //These 6 variables are for the "launch" mode
		private bool launched = false;
		private float rotation = 0f;
		private bool lockedOn = false;
		private int launchCounter = 20;
		Vector2 posToBe = Vector2.Zero;

		private NPC target;

		private Player owner => Main.player[Projectile.owner];
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 24;

			Projectile.aiStyle = -1;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			radiansToSpin = 6.28f * Main.rand.Next(2, 5) * (Main.rand.NextBool() ? -1 : 1);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			int xFrameSize = texture.Width / 4;
			int yFrameSize = texture.Height / 4;

			int xFrame = (int)bottleType;
			int yFrame = (int)liquidType;
			Rectangle frame = new Rectangle(xFrame * xFrameSize, yFrame * yFrameSize, xFrameSize, yFrameSize);

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

			for (int k = Projectile.oldPos.Length - 1; k > 0; k--) //TODO: Clean this shit up
			{
				Vector2 drawPos = Projectile.oldPos[k] + (new Vector2(Projectile.width, Projectile.height) / 2);
				Color color = Color.White * (float)(((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length)) * 0.3f;
				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(texture, drawPos - Main.screenPosition, frame, color, oldRotation[k], new Vector2(xFrameSize, yFrameSize) / 2, Projectile.scale, SpriteEffects.None, 0f);
			}

			/*Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.AlphaBlend, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);*/

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
				//liquidType = LiquidType.Poison;
				bottleType = (BottleType)Main.rand.Next(4);
				//bottleType = BottleType.Slide;

				switch (bottleType)
				{
					case BottleType.Regular:
						Projectile.velocity *= 1.2f;
						break;
					case BottleType.Launch:
						Projectile.velocity = Projectile.velocity.RotateRandom(0.3) * 0.5f;
						break;
					case BottleType.Slide:
						Projectile.velocity /= 2f;
						break;
				}
			}

			Lighting.AddLight(Projectile.Center, GetColor().ToVector3() * 0.5f);

			switch (bottleType)
			{
				case BottleType.Regular:
					Projectile.aiStyle = 2;
					break;
				case BottleType.Launch:
					if (!launched)
						target = Main.npc.Where(x => x.active && !x.townNPC && !x.immortal && !x.dontTakeDamage&& !x.friendly && x.Distance(Projectile.Center) < 1000).OrderBy(x => x.Distance(Projectile.Center)).FirstOrDefault();
					if (target != default && target.active)
						posToBe = target.Center;
					else if (posToBe == Vector2.Zero)
						posToBe = Main.MouseWorld;

					Vector2 direction = Projectile.DirectionTo(posToBe);
					if (!launched)
					{
						Projectile.velocity *= 0.96f;
						rotation = MathHelper.Lerp(rotation, direction.ToRotation() + radiansToSpin, 0.02f);
						Projectile.rotation = rotation + 1.57f;
						float difference = Math.Abs(rotation - (direction.ToRotation() + radiansToSpin));
						if (difference < 0.7f || lockedOn)
						{
							Projectile.velocity *= 0.8f;
							if (difference > 0.2f)
							{
								rotation += 0.1f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
							}
							else
								rotation = direction.ToRotation() + radiansToSpin;
							lockedOn = true;
							launchCounter--;
							Projectile.Center -= direction * 3;
							if (launchCounter <= 0)
							{
								launched = true;
								Projectile.velocity = direction * 30;
							}
						}
						else
						{
							rotation += 0.15f * Math.Sign((direction.ToRotation() + radiansToSpin) - rotation);
						}
					}
					else
					{
						Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
					}
					break;
				case BottleType.Slide:
					if (!sliding)
						Projectile.aiStyle = 2;
					else
                    {
						Projectile.velocity.X *= 0.985f;

						if (Math.Abs(Projectile.velocity.X) < 0.3f)
							Projectile.Kill();

						Projectile.rotation = 0f;
						if (Projectile.velocity.Y < 15)
							Projectile.velocity.Y += 0.1f;
                    }
					break;
				case BottleType.Bounce:
					Projectile.aiStyle = 2;
					break;
			}

			oldRotation.Add(Projectile.rotation);
			while (oldRotation.Count > Projectile.oldPos.Length)
			{
				oldRotation.RemoveAt(0);
			}
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
			fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (bottleType == BottleType.Slide)
			{
				if (oldVelocity.Y != Projectile.velocity.Y && oldVelocity.X == Projectile.velocity.X)
				{
					Projectile.aiStyle = 0;
					sliding = true;
					return false;
				}
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
			for (int i = 1; i <= 5; i++)
            {
				Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(3,3), Mod.Find<ModGore>("BizarrePotionGore" + i).Type, 1f);
			}
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item107, Projectile.position);
			switch (liquidType)
			{
				case LiquidType.Fire:
					Explode();
					break;
				case LiquidType.Lightning:
					SpawnLightning();
					break;
				case LiquidType.Ice:
					SpawnIce();
					break;
				case LiquidType.Poison:
					SpawnPoison();
					break;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			dontHit = target;
		}

		private void SpawnLightning()
		{
			for (int i = 0; i < 10; i++)
				Dust.NewDustPerfect(Projectile.Center, 133, Main.rand.NextVector2Circular(3, 3)).noGravity = false;

			var targets = Main.npc.Where(x => x.active && !x.townNPC  && !x.immortal && !x.dontTakeDamage&& !x.friendly && x.Distance(Projectile.Center) < 200);
			foreach (NPC target in targets)
			{
				if (dontHit == default || target != dontHit)
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BizarreLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner, target.whoAmI, target.Distance(Projectile.Center));
			}
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BizarreLightningOrb>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
		}

		private void Explode()
		{
			Core.Systems.CameraSystem.Shake += 8;

			Terraria.Audio.SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Magic/FireHit"), Projectile.Center);
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
				Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<CoachGunEmber>(), 0, 0, owner.whoAmI).scale = Main.rand.NextFloat(0.85f, 1.15f);
			}

			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<JetwelderJumperExplosion>(), Projectile.damage, 0, owner.whoAmI, dontHit == default ? -1 : dontHit.whoAmI);
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

		private void SpawnPoison()
        {
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BizarrePotionPoisonCloud>(), 0, 0, owner.whoAmI);
			for (int i = 0; i < 60; i++)
			{
				float lerper = Main.rand.NextFloat();
				Color color = Main.rand.NextBool() ? Color.Lerp(Color.Violet, Color.Purple, lerper) : Color.Lerp(Color.Purple, Color.Magenta, lerper);
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<BizarrePoisonDust>(), default, default, default, color);
				dust.velocity = Main.rand.NextVector2Circular(4, 4);
				dust.scale = Main.rand.NextFloat(1, 1.25f) * 0.75f;
				dust.alpha = Main.rand.Next(100);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}
		}

		private void SpawnIce()
		{
			float rotOffset = Main.rand.NextFloat(6.28f);
			for (float i = rotOffset; i < 6.28f + rotOffset; i += (float)Math.PI * 0.3f)
			{
				Vector2 direction = i.ToRotationVector2();
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center + (direction * 10), direction * Main.rand.NextFloat(2, 3), ModContent.ProjectileType<BizarreIce>(), Projectile.damage / 2, Projectile.knockBack / 2, owner.whoAmI);
				var mp = proj.ModProjectile as BizarreIce;
				mp.dontHit = dontHit;
			}

			for (int i = 0; i < 3; i++)
			{
				Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(16, 16), 0, 0, ModContent.DustType<BizarreIceDust>());
				dust.velocity = Main.rand.NextVector2Circular(2, 2);
				dust.scale = Main.rand.NextFloat(1, 1.25f) * 0.75f;
				dust.alpha = Main.rand.Next(100);
				dust.rotation = Main.rand.NextFloat(6.28f);
			}

			for (int i = 0; i < 5; i++)
				Dust.NewDustPerfect(Projectile.Center, 67, Main.rand.NextVector2Circular(2, 2)).noLight = true;
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

	public class BizarreLightningOrb : ModProjectile, IDrawAdditive
	{
		public override string Texture => AssetDirectory.Assets + "Keys/GlowSoft";

		private float fade => EaseFunction.EaseCubicOut.Ease(Projectile.timeLeft / 30f);
		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = false;
			Projectile.timeLeft = 30;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.hide = true;
		}

		public void DrawAdditive(SpriteBatch sb)
		{
			var tex = ModContent.Request<Texture2D>(Texture).Value;
			for (int i = 0; i < 3; i++)
				sb.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Yellow * fade, 0, tex.Size() / 2, 0.55f, SpriteEffects.None, 0f);
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
		private Trail trail3;

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Ranged;
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

			if (cache != null)
			{
				foreach (Vector2 v in cache)
					Lighting.AddLight(v, Color.Yellow.ToVector3() * 0.4f);
			}
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
					if (i > cache.Count - 3 || dir == Vector2.Zero || i == 0)
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
				return Color.Lerp(Color.Yellow, Color.White, EaseFunction.EaseCubicIn.Ease(Math.Min(1.2f - progress, 1))) * progress * fade;
			});

			trail2.Positions = cache2.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;

			trail3 = trail3 ?? new Trail(Main.instance.GraphicsDevice, trailLength, new TriangularTip(4), factor => thickness * 2 * Main.rand.NextFloat(0.55f, 1.45f), factor =>
			{
				float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
				return Color.White * progress * fade;
			});

			trail3.Positions = cache2.ToArray();
			trail3.NextPosition = Projectile.Center + Projectile.velocity;
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
			trail3?.Render(effect);
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

	public class BizarrePotionPoisonCloud : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		public override void SetDefaults()
		{
			Projectile.width = 250;
			Projectile.height = 250;
			Projectile.friendly = true;
			Projectile.timeLeft = 120;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = -1;
			Projectile.hide = true;
			Projectile.usesLocalNPCImmunity = true;
		}

        public override void AI()
        {
            var targets = Main.npc.Where(x => x.active && !x.townNPC && !x.immortal && !x.dontTakeDamage && x.Distance(Projectile.Center) < 73);
			foreach (NPC target in targets)
            {
				target.AddBuff(ModContent.BuffType<BizarrePotionPoisonDebuff>(), 2);
            }
		}
    }

	public class BizarreIce : ModProjectile
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		private int scaleCounter;

		private int fadeTime = 40;

		private Player owner => Main.player[Projectile.owner];

		public NPC dontHit;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Bizarre Potion");
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults()
		{
			Projectile.width = 12;
			Projectile.height = 12;

			Projectile.aiStyle = 1;
			Projectile.extraUpdates = 1;

			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.frame = Main.rand.Next(3);
		}

		public override void AI()
		{
			scaleCounter++;
			Projectile.scale = Math.Min(scaleCounter / 24f, 1);
			if (Projectile.timeLeft > fadeTime)
				Projectile.rotation = Projectile.velocity.ToRotation() + 1.57f;
			else
				Projectile.alpha = (int)(255 * (1 - ((float)Projectile.timeLeft / fadeTime)));
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target == dontHit)
				return false;
			return base.CanHitNPC(target);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = Vector2.Zero;
			Projectile.aiStyle = -1;
			if (Projectile.timeLeft > fadeTime)
			{
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
				for (int i = 0; i < 6; i++)
					Dust.NewDustPerfect(Projectile.Center, 67, Main.rand.NextVector2Circular(2, 2)).noLight = true;
				Projectile.timeLeft = fadeTime;
			}
			return false;
		}
		public override void Kill(int timeLeft)
		{
			if (timeLeft == 0)
				return;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27, Projectile.position);
			for (int i = 0; i < 6; i++)
				Dust.NewDustPerfect(Projectile.Center, 67, Main.rand.NextVector2Circular(2, 2)).noLight = true;
		}
	}

	public class BizarreIceDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.2f, 0.5f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(105, 105, 105);
			Color ret;
			if (dust.alpha < 10)
			{
				ret = Color.Lerp(Color.Cyan, Color.LightBlue, dust.alpha / 100f);
			}
			else if (dust.alpha < 200)
			{
				ret = Color.Lerp(Color.LightBlue, gray, (dust.alpha - 100) / 100f);
			}
			else
				ret = gray;
			return ret * ((255 - dust.alpha) / 255f);
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 3)
				dust.velocity *= 0.85f;
			else
				dust.velocity *= 0.92f;

			if (dust.alpha > 100)
			{
				dust.scale *= 1.01f;
				dust.alpha += 2;
			}
			else
			{
				dust.scale *= 1.01f;
				dust.alpha += 4;
			}
			Lighting.AddLight(dust.position, Color.LightBlue.ToVector3() * ((255 - dust.alpha) / 255f));
			dust.position += dust.velocity;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class BizarrePoisonDust : ModDust
	{
		public override string Texture => AssetDirectory.Dust + "NeedlerDust";

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = true;
			dust.scale *= Main.rand.NextFloat(0.2f, 0.7f);
			dust.frame = new Rectangle(0, 0, 34, 36);
		}

		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			Color gray = new Color(105, 105, 105);
			Color ret;
			if (dust.alpha < 10)
			{
				ret = Color.Lerp(Color.Purple, dust.color, dust.alpha / 100f);
			}
			else if (dust.alpha < 200)
			{
				ret = Color.Lerp(dust.color, gray, (dust.alpha - 100) / 100f);
			}
			else
				ret = gray;
			return ret * ((255 - dust.alpha) / 255f) * 0.15f;
		}

		public override bool Update(Dust dust)
		{
			if (dust.velocity.Length() > 2)
				dust.velocity *= 0.9f;
			else
				dust.velocity *= 0.95f;

			dust.alpha += 1;
			if (dust.alpha > 100)
			{
				dust.scale *= 1.001f;
			}
			else
			{
				dust.scale *= 1.002f;
			}
			Lighting.AddLight(dust.position, Color.Violet.ToVector3() * ((255 - dust.alpha) / 255f));
			dust.position += dust.velocity;
			dust.rotation += 0.02f;

			if (dust.alpha >= 255)
				dust.active = false;

			return false;
		}
	}

	public class BizarrePotionGNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;

		public bool infected = false;

		public override void ResetEffects(NPC npc)
		{
			infected = false;
		}

		public override void UpdateLifeRegen(NPC npc, ref int damage)
		{
			if (infected)
            {
				if (npc.lifeRegen > 0)
				{
					npc.lifeRegen = 0;
				}
				npc.lifeRegen -= 24;
				if (damage < 3)
				{
					damage = 3;
				}
			}
		}
	}

	class BizarrePotionPoisonDebuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public BizarrePotionPoisonDebuff() : base("Bizarre Poison", "You poisoned", true) { }

		public override void Update(NPC NPC, ref int buffIndex)
		{
			NPC.GetGlobalNPC<BizarrePotionGNPC>().infected = true;
		}
	}
}