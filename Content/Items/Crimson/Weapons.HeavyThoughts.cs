using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Forest;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Snow;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Tiles.Misc;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using Terraria.Audio;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	internal class HeavyThoughts : AbstractHeavyFlail
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override int ProjType => ModContent.ProjectileType<HeavyThoughtsProjectile>();

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Heavy Thoughts");
			// MASSIVE GENIUS LOL
			Tooltip.SetDefault("Hold to swing a mass of genius\nCreates psychic seekers upon impact\nThese seekers deal no damage, but inflict {{BUFF:Neurosis}} and {{BUFF:Psychosis}}");
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			Item.rare = ItemRarityID.Orange;
			Item.damage = 45;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TissueSample, 12);
			recipe.AddIngredient(ModContent.ItemType<DendriteBar>(), 10);	
			recipe.AddIngredient(ModContent.ItemType<HeavyFlail>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class HeavyThoughtsProjectile : AbstractHeavyFlailProjectile
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override Asset<Texture2D> ChainAsset => Assets.Items.Crimson.HeavyThoughtsProjectileChain;

		public override int MaxLength => 200;

		public override void AI()
		{
			base.AI();

			/*for (int k = 0; k < 2; k++)
			{
				Dust.NewDust(Projectile.Center - Vector2.One * 20, 40, 40, DustID.IceTorch);
			}*/
		}

		public override void OnImpact(bool wasTile)
		{
			if (wasTile)
			{
				DistortionPointHandler.AddPoint(Projectile.Center, 20f, 0f,
					(intensity, ticksPassed) => (float)Math.Sqrt(intensity) * Utils.Clamp(1f - ticksPassed / 20f, 0.5f, 1f),
					(progress, ticksPassed) => ticksPassed / 20f,
					(progress, intensity, ticksPassed) => ticksPassed < 20);

				//Helpers.SoundHelper.PlayPitched("Magic/FrostHit", 1, 0, Projectile.Center);

				if (Owner == Main.LocalPlayer)
					CameraSystem.shake += 5;

				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center + Vector2.UnitY * 8, Vector2.Zero, ModContent.ProjectileType<FrostFlailCrack>(), 0, 0);

				for (int i = 0; i < 2 + Main.rand.Next(5); i++)
				{
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(5f, 5f)
						, ModContent.ProjectileType<HeavyThoughtsSeeker>(), Projectile.damage, 0f, Projectile.owner);
				}
			}
		}
	}

	internal class HeavyThoughtsSeeker : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		private float curveOffset;
		private int curveDirection;

		private int deathTimer;

		float fade = 1f;
		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Seeker");
		}

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;

			Projectile.penetrate = 2;

			Projectile.width = Projectile.height = 8;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 180;
			Projectile.extraUpdates = 1;

			curveOffset = Main.rand.NextFloat(-2f, 2f);
			curveDirection = Main.rand.NextBool() ? -1 : 1;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override bool ShouldUpdatePosition()
		{
			return true;
		}

		public override bool? CanDamage()
		{
			return deathTimer <= 0;
		}

		public override void AI()
		{
			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}

			if (deathTimer > 0)
			{
				if (Projectile.velocity.Length() > 0)
					Projectile.velocity *= 0f;

				fade -= 0.05f;
				
				deathTimer--;
				if (deathTimer == 0)
					Projectile.Kill();

				return;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
			//just basically copied from blood amulet
			NPC target = Main.npc.Where(n => n.CanBeChasedBy(Projectile, false)
			&& Vector2.Distance(n.Center, Projectile.Center) < 1200f).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

			if (target != default)
			{
				Vector2 direction = target.Center - Projectile.Center;
				direction.Normalize();

				direction *= MathHelper.Lerp(0f, 75f, Eases.EaseQuadIn(1f - Projectile.timeLeft / 180f));
				float rot = curveOffset * (float)Math.Sin(Projectile.timeLeft * 0.05f);
				float progress = Eases.EaseCircularInOut(Projectile.timeLeft / 180f);
				//Main.NewText("Rot: " + rot + "Prog: " + progress + "Final: " + rot * progress * curveDirection);
				direction = direction.RotatedBy(rot * progress * curveDirection);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.045f);

				if (Projectile.timeLeft < 2)
					Projectile.timeLeft = 2;

				if (fade < 1f)
					fade += 0.05f;
			}
			else
			{
				fade -= 0.01f;

				float rot = curveOffset * (float)Math.Sin(Projectile.timeLeft * 0.05f);
				float progress = Eases.EaseCircularInOut(Projectile.timeLeft / 180f);
				//Main.NewText("Rot: " + rot + "Prog: " + progress + "Final: " + rot * progress * curveDirection);
				Projectile.velocity = Projectile.velocity.RotatedBy(rot * progress * curveDirection) * 1.01f;

				Projectile.timeLeft--;
			}

			//if (Main.rand.NextBool(3))
				//Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Dusts.GlowFastDecelerate>(), 0f, 0f, 0, new Color(220, 205, 140), 0.35f);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			Projectile.velocity *= 0;
			deathTimer = 20;
			Projectile.timeLeft = 20;

			for (int k = 0; k < 2; k++)
			{
				BuffInflictor.Inflict<Neurosis>(target, 300);
			}

			BuffInflictor.Inflict<Psychosis>(target, 300);

			// Projectile deals no damage
			modifiers.FinalDamage *= 0;
			modifiers.HideCombatText();
		}

		public override void OnKill(int timeLeft)
		{

		}	

		public override void PostDraw(Color lightColor)
		{

		}

		private float TrailFade()
		{
			if (fade < 0f)
				return 0f;

			if (fade > 1f)
				return 1f;

			return fade;
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 15; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 15)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			if (trail is null || trail.IsDisposed)
			{
				trail = new Trail(Main.instance.GraphicsDevice, 15, new NoTip(), factor => factor * 4.5f,
					factor => Color.White * factor.X * TrailFade());
			}

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;
		}

		public void DrawPrimitives()
		{
			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.EffectMatrix);

				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Main.GameViewMatrix.TransformationMatrix;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.03f);
					effect.Parameters["repeats"].SetValue(2f);
					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["sampleTexture"].SetValue(Assets.GlowTrail.Value);

					trail?.Render(effect);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
			});
		}
	}
}
