using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Desert
{
	internal class Sandscript : ModItem
	{
		private float swingDirection = 1f;
		public override string Texture => AssetDirectory.DesertItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sandscript");
			Tooltip.SetDefault("Manifests a blade of sand\n`The actual words are lost to time...`");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 30;

			Item.useStyle = ItemUseStyleID.Shoot;

			Item.useAnimation = 45;
			Item.useTime = 45;

			Item.shootSpeed = 1f;
			Item.knockBack = 7f;
			Item.damage = 15;
			Item.crit = 6;
			Item.shoot = ProjectileType<SandscriptBook>();
			Item.rare = ItemRarityID.Blue;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 3;

			Item.UseSound = SoundID.DD2_MonkStaffSwing;

			Item.value = Item.sellPrice(silver: 50);
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI, 0f, swingDirection); // ai 1 for rotation

			if (swingDirection == -1f)
				swingDirection = 1f;
			else
				swingDirection = -1f;

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Sandstone, 25);
			recipe.AddIngredient(ItemID.Sapphire);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class SandscriptBook : ModProjectile
	{
		public ref float SwingDirection => ref Projectile.ai[1];
		public ref float MaxTimeleft => ref Projectile.ai[0];

		public float Progress => 1f - Projectile.timeLeft / MaxTimeleft;

		public Player Owner => Main.player[Projectile.owner];
		public Vector2? OwnerMouse => (Main.myPlayer == Owner.whoAmI) ? Main.MouseWorld : null;

		public override string Texture => AssetDirectory.DesertItem + "Sandscript";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Sandscript");
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.hostile = false;

			Projectile.width = 32;
			Projectile.height = 36;

			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Magic;

			Projectile.scale = 0.6f;
		}

		public override void OnSpawn(IEntitySource source)
		{
			float useSpeed = Owner.HeldItem.useTime * (1f - (Owner.GetTotalAttackSpeed(DamageClass.Magic) - 1f));
			Projectile.timeLeft = (int)(useSpeed * 0.9f);
			MaxTimeleft = Projectile.timeLeft;

			Projectile.velocity = Owner.DirectionTo(OwnerMouse.Value);
			Projectile.rotation = Projectile.velocity.ToRotation();
		}

		public override void AI()
		{
			Owner.heldProj = Projectile.whoAmI;

			Projectile.rotation += MathHelper.Lerp(0.5f, 0.02f, EaseBuilder.EaseQuarticOut.Ease(Progress));
			Projectile.Center = Owner.MountedCenter + Projectile.velocity.RotatedBy(45f * SwingDirection) * MathHelper.Lerp(5f, 45f, EaseBuilder.EaseQuarticOut.Ease(Progress));

			Projectile.scale = MathHelper.Lerp(0.6f, 1.2f, EaseBuilder.EaseQuarticOut.Ease(Progress));

			float lerper = MathHelper.Lerp(45f, 2f, EaseBuilder.EaseCircularInOut.Ease(Progress));

			if (Projectile.timeLeft < MaxTimeleft)
				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(lerper, lerper), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(0.5f, 0.5f), 0, Main.rand.NextBool() ? new Color(30, 230, 200) : new Color(230, 170, 100), 0.4f);

			if (Projectile.timeLeft == 10)
			{
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.Center, Owner.DirectionTo(OwnerMouse.Value), ModContent.ProjectileType<SandSlash>(), Projectile.damage, Projectile.knockBack, Projectile.owner, SwingDirection);

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Owner.DirectionTo(OwnerMouse.Value) * 3f + Main.rand.NextVector2Circular(1.5f, 1.5f), 0, Main.rand.NextBool() ? new Color(30, 230, 200) : new Color(230, 170, 100), 0.4f);

					Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Sand>(), Owner.DirectionTo(OwnerMouse.Value) * 3f + Main.rand.NextVector2Circular(1.5f, 1.5f), 140, default, 0.6f).noGravity = true;
				}
			}

			if (Projectile.timeLeft > 10)
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Owner.DirectionTo(OwnerMouse.Value), 0.1f);
		}

		public override void Kill(int timeLeft)
		{
			base.Kill(timeLeft);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			float mult = 1f;
			if (Projectile.timeLeft > MaxTimeleft * 0.8f)
				mult = 1f - (Projectile.timeLeft - MaxTimeleft * 0.8f) / MaxTimeleft * 0.2f;
			else if (Projectile.timeLeft < 15)
				mult = Projectile.timeLeft / 15f;

			Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, lightColor * mult, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			if (Projectile.timeLeft < 10)
				Main.spriteBatch.Draw(tex, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, lightColor * mult, Projectile.rotation, tex.Size() / 2f, Projectile.scale * MathHelper.Lerp(1f, 2f, EaseBuilder.EaseCubicOut.Ease(1f - Projectile.timeLeft / 10f)), 0f, 0f);
			return false;
		}
	}

	internal class SandSlash : ModProjectile, IDrawPrimitive
	{
		public bool hasPlayedSound;

		public const float maxTimeleft = 55;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		public ref float SwingDirection => ref Projectile.ai[0];

		public Vector2? OwnerMouse => (Main.myPlayer == Owner.whoAmI) ? Main.MouseWorld : null;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = (int)maxTimeleft;
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.tileCollide = false;
			Projectile.extraUpdates = 2;
			Projectile.penetrate = -1;

			Projectile.DamageType = DamageClass.Magic;
		}

		public override void AI()
		{
			float progress = 1f - Projectile.timeLeft / maxTimeleft;

			float mult = 1f;

			Projectile.rotation = Projectile.velocity.ToRotation();

			if (progress <= 0.5f)
			{
				float lerper = 1f - (Projectile.timeLeft - maxTimeleft * 0.5f) / (maxTimeleft * 0.5f);

				mult = MathHelper.Lerp(1f, 10f, EaseBuilder.EaseQuinticOut.Ease(lerper));

				if (!hasPlayedSound && progress >= 0.1f)
				{
					hasPlayedSound = true;
					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item45, Projectile.Center);

					Core.Systems.CameraSystem.CameraSystem.shake += 5;
				}
			}
			else
			{
				float lerper = 1f - Projectile.timeLeft / (maxTimeleft * 0.5f);

				mult = MathHelper.Lerp(10f, -2f, EaseBuilder.EaseQuinticIn.Ease(lerper));
			}

			Projectile.Center = Owner.Center + Vector2.Lerp(new Vector2(20f * mult, 40f * SwingDirection).RotatedBy(Projectile.rotation), new Vector2(20f * mult, -50f * SwingDirection).RotatedBy(Projectile.rotation), progress);

			if (Projectile.timeLeft < maxTimeleft)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.velocity * 2f + Main.rand.NextVector2Circular(2f, 2f), 0, Color.Lerp(new Color(30, 230, 200), new Color(230, 170, 100), progress), 0.5f);

				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Sand>(), Projectile.velocity * 3f + Main.rand.NextVector2Circular(2f, 2f), 140, default, 0.6f).noGravity = false;

				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Sand>(), Projectile.velocity * 3f + Main.rand.NextVector2Circular(2f, 2f), 140, default, 0.6f).noGravity = true;
			}

			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 4; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(Owner.Center) * Main.rand.NextFloat(7f) + Main.rand.NextVector2Circular(2f, 2f), 0, Color.Lerp(new Color(30, 230, 200), new Color(230, 170, 100), 1f - Projectile.timeLeft / maxTimeleft), 0.75f);

				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Sand>(), target.Center.DirectionTo(Owner.Center) * Main.rand.NextFloat(5f) + Main.rand.NextVector2Circular(3f, 3f), 140, default, 0.6f).noGravity = false;
			}

			Helper.PlayPitched("Impacts/StoneStrikeLight", 1f, Main.rand.NextFloat(0.6f, 1.0f), Projectile.Center); // [PH] for egshels
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D starTex = ModContent.Request<Texture2D>(AssetDirectory.Assets + "StarTexture").Value;

			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			float mult = 0f;
			if (Projectile.timeLeft > maxTimeleft * 0.6f)
				mult = 1f - (Projectile.timeLeft - maxTimeleft * 0.6f) / maxTimeleft * 0.4f;
			else if (Projectile.timeLeft < 15)
				mult = Projectile.timeLeft / 15f;

			Color color = Color.Lerp(new Color(30, 230, 200, 0), new Color(230, 170, 100, 0), 1f - Projectile.timeLeft / maxTimeleft) * mult;

			Main.spriteBatch.Draw(starTex, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, color, 0f, starTex.Size() / 2f, 0.4f, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center + new Vector2(0f, Owner.gfxOffY) - Main.screenPosition, null, color * 0.5f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

			return false;
		}

		#region Primitive Drawing

		private void ManageCaches()
		{
			if (cache == null)
			{
				cache = new List<Vector2>();
				for (int i = 0; i < 20; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			cache.Add(Projectile.Center);

			while (cache.Count > 20)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			float mult = 1f;
			if (Projectile.timeLeft > maxTimeleft * 0.5f)
			{
				mult = 1f - (Projectile.timeLeft - maxTimeleft * 0.5f) / maxTimeleft * 0.5f;
			}

			trail ??= new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(0), factor => 30f * factor, factor =>
			{
				if (factor.X >= 0.85f)
					return Color.Transparent;

				return Color.Lerp(new Color(30, 230, 200), new Color(230, 170, 100), 1f - Projectile.timeLeft / maxTimeleft) * factor.X * (float)Math.Sin(Projectile.timeLeft / maxTimeleft) * mult * 0.5f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 20, new TriangularTip(0), factor => 20f, factor =>
			{
				if (factor.X >= 0.85f)
					return Color.Transparent;

				return Color.Lerp(new Color(30, 230, 200), new Color(230, 170, 100), 1f - Projectile.timeLeft / maxTimeleft) * factor.X * (float)Math.Sin(Projectile.timeLeft / maxTimeleft) * mult * 0.5f;
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
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
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LiquidTrailAlt").Value);
			trail2?.Render(effect);
		}

		#endregion Primitive Drawing

	}
}