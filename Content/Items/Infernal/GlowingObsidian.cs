using StarlightRiver.Core.Systems.InstancedBuffSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class GlowingObsidian : ModItem
	{
		public int combo;

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/GlowingObsidian";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Glowing Obsidian");
			Tooltip.SetDefault("Inflicts stacking burning\n[RIGHT] to skewer nearby enemies, increasing burning damage taken");
		}

		public override void SetDefaults()
		{
			Item.damage = 24;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.DamageType = DamageClass.Melee;
			Item.shoot = ModContent.ProjectileType<GlowingObsidianSlash>();
			Item.shootSpeed = 1;
			Item.rare = ItemRarityID.Orange;
			Item.autoReuse = true;
			Item.knockBack = 5;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, combo);
			return false;
		}

		public override bool? UseItem(Player player)
		{
			combo++;

			if (combo > 2)
				combo = 0;

			Helpers.Helper.PlayPitched("Impacts/FireBladeStab", 1, 0.4f + combo * 0.2f);

			return null;
		}

		public override float UseTimeMultiplier(Player player)
		{
			return combo == 2 ? 1.5f : 1f;
		}
	}

	internal class GlowingObsidianSlash : ModProjectile, IDrawPrimitive
	{
		private List<Vector2> cache;
		private Trail trail;

		public ref float Combo => ref Projectile.ai[0];

		Player Owner => Main.player[Projectile.owner];

		public float Length => 68 * Projectile.scale;
		public float MaxTime => Combo switch
		{
			0 => 20,
			1 => 20,
			2 => 30,
			_ => 20
		};

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/GlowingObsidian";

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 9999;
			Projectile.penetrate = -1;
			Projectile.width = 400;
			Projectile.height = 400;
			Projectile.friendly = true;
		}

		public override void AI()
		{
			float initialOffset = Combo switch
			{
				0 => -2.5f,
				1 => 4.5f,
				2 => -3.5f,
				_ => 0
			};

			float rotSpeed = Combo switch
			{
				0 => 0.7f,
				1 => -0.7f,
				2 => 0.6f,
				_ => 0
			};

			if (Projectile.timeLeft == 9999)
			{
				Projectile.timeLeft = (int)MaxTime;
				Projectile.rotation = Projectile.velocity.ToRotation() + initialOffset;

				Projectile.scale = Combo switch
				{
					0 => 1.2f,
					1 => 1.4f,
					2 => 1.6f,
					_ => 1f
				};
			}

			Projectile.Center = Owner.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 0.9f) * 16;
			Projectile.rotation += rotSpeed * (float)Math.Pow(Projectile.timeLeft / MaxTime, 2);

			Owner.heldProj = Projectile.whoAmI;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 0.9f - 1.57f);

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			bool hit = false;

			for (int k = -2; k <= 2; k++)
			{
				hit |= Helper.CheckLinearCollision(Projectile.Center, Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation - 0.9f + k * 0.05f) * Length, target.Hitbox, out _);
			}

			return hit ? null : false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			BuffInflictor.Inflict<GlowingObsidianFire>(target, 180);
		}

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 20; i++)
				{
					cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation - 0.9) * Length, Vector2.Zero, 0.3f));
				}
			}

			cache.Add(Vector2.Lerp(Vector2.UnitX.RotatedBy(Projectile.rotation - 0.9) * Length, Vector2.Zero, 0.3f));

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(60), factor => factor * Length * 0.8f, factor =>
			{
				float mul = factor.X - (Projectile.timeLeft - (MaxTime - 20)) / 20f;

				if (mul < 0)
					mul = 0;

				if (mul > factor.X)
					mul = factor.X;

				if (factor.X >= 0.9f)
					return Color.White * 0;

				float opacity = 0.6f;

				if (Projectile.timeLeft < 5)
					opacity *= Projectile.timeLeft / 5f;

				return new Color(255, (int)(255 * mul), 0) * opacity * mul;
			});

			Vector2[] ar = cache.ToArray();
			for (int k = 0; k < ar.Length; k++)
			{
				ar[k] = ar[k] + Projectile.Center;
			}

			trail.Positions = ar;
			trail.NextPosition = Vector2.Lerp(Projectile.Center, Owner.Center, 0.15f) + Projectile.velocity;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch spriteBatch = Main.spriteBatch;

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glow = ModContent.Request<Texture2D>(Texture + "Glow").Value;

			float opacity = 1f;

			if (Projectile.timeLeft < 5)
				opacity *= Projectile.timeLeft / 5f;

			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, Vector2.UnitY * tex.Height, 1, 0, 0);
			spriteBatch.Draw(glow, Projectile.Center - Main.screenPosition, null, Color.White * opacity, Projectile.rotation, Vector2.UnitY * tex.Height, 1, 0, 0);

			return false;
		}

		public void DrawPrimitives()
		{
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/MotionTrail").Value);
			trail?.Render(effect);
		}
	}

	internal class GlowingObsidianSpike : ModProjectile
	{
		public override string Texture => "StarlightRiver/Assets/Items/Infernal/GlowingObsidianSpike";

		public override void SetDefaults()
		{
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 9999;
			Projectile.penetrate = -1;
			Projectile.width = 400;
			Projectile.height = 400;
			Projectile.friendly = true;
		}
	}

	internal class GlowingObsidianFire : StackableBuff
	{
		public override string Name => "GlowingObsidianFire";

		public override string DisplayName => "Infernal flames";

		public override string Texture => AssetDirectory.Debug;

		public override bool Debuff => true;

		public override string Tooltip => "Burning up!";

		public override BuffStack GenerateDefaultStack(int duration)
		{
			return new BuffStack()
			{
				duration = duration
			};
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 30;
		}

		public override void AnyStacksUpdateNPC(NPC npc)
		{
			if (npc.lifeRegenExpectedLossPerSecond <= 5)
				npc.lifeRegenExpectedLossPerSecond = 5;

			if (Main.rand.NextBool(4))
				Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Dusts.Cinder>(), 0, -1, 0, Color.Orange, 0.5f);
		}
	}
}