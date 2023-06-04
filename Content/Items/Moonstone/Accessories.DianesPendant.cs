using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Core.Systems.BarrierSystem;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Neck)]
	public class DianesPendant : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diane's Pendant");
			Tooltip.SetDefault("Consuming mana charges a Crescent Guardian, blitzing nearby enemies when fully charged\n+20 barrier");
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 28;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.buyPrice(0, 5, 0, 0);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player Player, bool hideVisual)
		{
			Player.GetModPlayer<BarrierPlayer>().maxBarrier += 20;
			Player.GetModPlayer<DianePlayer>().active = true;

			if (Player.ownedProjectileCounts[ModContent.ProjectileType<DianeCrescant>()] < 1 && !Player.dead)
				Projectile.NewProjectile(Player.GetSource_Accessory(Item), Player.Center, new Vector2(7, 7), ModContent.ProjectileType<DianeCrescant>(), 30, 1.5f, Player.whoAmI);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 4);
			recipe.AddIngredient(ModContent.ItemType<AquaSapphire>(), 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class DianePlayer : ModPlayer
	{
		public bool active = false;
		public int charge = 0;
		private int currentMana = -1;

		public override void ResetEffects()
		{
			active = false;
		}

		public override void OnMissingMana(Item item, int neededMana)
		{
			if (active && charge >= 40)
			{
				for (int i = 0; i < Main.projectile.Length; i++)
				{
					Projectile proj = Main.projectile[i];

					if (proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<DianeCrescant>())
					{
						var mp = proj.ModProjectile as DianeCrescant;

						if (!mp.attacking)
							mp.StartAttack();
						break;
					}
				}

				charge = 0;
			}
		}

		public override void PostUpdate()
		{
			if (currentMana != Player.statMana && active)
			{
				if (currentMana > Player.statMana && charge < 500)
					charge += currentMana - Player.statMana;

				currentMana = Player.statMana;
			}
		}
	}

	public class DianeCrescant : ModProjectile
	{
		private List<Vector2> oldPosition = new();
		private List<float> oldRotation = new();
		private bool initialized = false;

		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;

		private int charge = 0;

		private float flashTimer = 1;
		private bool fullyCharged = false;

		public bool attacking = false;

		private float speed = 10;

		private Vector2 oldVel = Vector2.Zero;
		private int pauseTimer = 0;

		private List<NPC> alreadyHit = new();

		private float arcTimer = 0;

		private float ChargeRatio => charge / 500f;

		private int AfterimageLength => (int)MathHelper.Lerp(11, 22, ChargeRatio);

		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crescant");
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 68;
			Projectile.height = 68;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 216000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Magic;
		}

		public override void AI()
		{
			Projectile.scale = MathHelper.Lerp(0.75f, 1f, ChargeRatio);

			if (!initialized)
			{
				initialized = true;
				oldPosition = new List<Vector2>();
				oldRotation = new List<float>();
			}

			Player Player = Main.player[Projectile.owner];

			Projectile.rotation += Projectile.velocity.Length() * 0.01f;

			var lightColor = new Color(150, 120, 255);
			Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.6f);

			if (Player.dead)
				Projectile.active = false;

			if (Player.GetModPlayer<DianePlayer>().active)
				Projectile.timeLeft = 2;

			if (attacking)
				AttackMovement();
			else
				IdleMovement(Player);

			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			oldRotation.Add(Projectile.rotation);
			oldPosition.Add(Projectile.Center);

			while (oldRotation.Count > AfterimageLength)
			{
				oldRotation.RemoveAt(0);
			}

			while (oldPosition.Count > AfterimageLength)
			{
				oldPosition.RemoveAt(0);
			}

			if (flashTimer < 1)
				flashTimer += 0.04f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			//DrawTrail(Main.spriteBatch);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, BlendState.Additive, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "Glow").Value;

			for (int k = AfterimageLength; k > 0; k--)
			{
				float progress = 1 - (float)((AfterimageLength - k) / (float)AfterimageLength);
				Color color = new Color(100, 60, 255) * EaseFunction.EaseQuarticOut.Ease(progress) * MathHelper.Lerp(0.45f, 0.75f, ChargeRatio);

				if (k > 0 && k < oldRotation.Count)
					Main.spriteBatch.Draw(tex, oldPosition[k] - Main.screenPosition, null, color, oldRotation[k], tex.Size() / 2, Projectile.scale * EaseFunction.EaseQuadOut.Ease(progress), SpriteEffects.None, 0f);
			}

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			if (flashTimer < 1)
			{
				float transparency = (float)Math.Pow(1 - flashTimer, 2);
				float scale = 1 + flashTimer;
				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * transparency, Projectile.rotation, tex.Size() / 2, Projectile.scale * scale, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			Color glowColor = new Color(150, 120, 255, 0) * 0.5f;
			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, glowTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);

			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (alreadyHit.Contains(target) || pauseTimer > 0)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player Player = Main.player[Projectile.owner];
			BarrierPlayer modPlayer = Player.GetModPlayer<BarrierPlayer>();
			int barrierRecovery = (int)MathHelper.Lerp(2, 6, ChargeRatio);

			if (modPlayer.barrier < modPlayer.maxBarrier - barrierRecovery)
				modPlayer.barrier += barrierRecovery;
			else
				modPlayer.barrier = modPlayer.maxBarrier;

			alreadyHit.Add(target);
			CameraSystem.shake += 7;
			NPC nextTarget = Main.npc.Where(x => x.active && !x.townNPC && !alreadyHit.Contains(x) && Projectile.Distance(x.Center) < 600).OrderBy(x => Projectile.Distance(x.Center)).FirstOrDefault();

			if (nextTarget != default)
			{
				float degrees = 0.5f + Projectile.Distance(nextTarget.Center) / 600f;

				if (alreadyHit.Count % 2 == 0)
					degrees *= -1;

				oldVel = Projectile.DirectionTo(nextTarget.Center).RotatedBy(degrees);
			}
			else
			{
				oldVel = Vector2.Normalize(Projectile.velocity);
			}

			pauseTimer = 10;
			flashTimer = 0;
			arcTimer = 0;
		}

		public void StartAttack()
		{
			fullyCharged = false;
			Projectile.friendly = true;
			Projectile.damage = (int)MathHelper.Lerp(10, 50, ChargeRatio);
			attacking = true;
			alreadyHit = new List<NPC>();
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

			cache.Add(Projectile.Center - Projectile.rotation.ToRotationVector2() * 20);

			while (cache.Count > 15)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{

			trail ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(40 * 4), factor => (10 + factor * 25) * MathHelper.Lerp(0.4f, 1f, ChargeRatio), factor =>
			{
				if (factor.X >= 0.96f)
					return Color.White * 0;

				return new Color(120, 20 + (int)(100 * factor.X), 255) * (float)Math.Sin(factor.X * 3.14f);
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = Projectile.Center + Projectile.velocity;

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new TriangularTip(40 * 4), factor => (80 + 0 + factor * 0) * MathHelper.Lerp(0.4f, 1f, ChargeRatio), factor =>
			{
				if (factor.X >= 0.96f)
					return Color.White * 0;

				return new Color(100, 20 + (int)(60 * factor.X), 255) * 0.15f * (float)Math.Sin(factor.X * 3.14f);
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = Projectile.Center + Projectile.velocity;
		}

		private void AttackMovement()
		{
			pauseTimer--;

			if (pauseTimer > 0)
			{
				if (flashTimer < 1)
					flashTimer += 0.04f;

				Projectile.velocity = Vector2.Zero;
				return;
			}

			if (arcTimer < 1)
				arcTimer += 0.05f;

			Projectile.rotation += Projectile.velocity.Length() * 0.05f;
			Projectile.friendly = true;
			speed = MathHelper.Lerp(30, 40, ChargeRatio);
			NPC target = Main.npc.Where(x => x.active && !x.townNPC && !alreadyHit.Contains(x) && Projectile.Distance(x.Center) < 600).OrderBy(x => Projectile.Distance(x.Center)).FirstOrDefault();

			if (target == default)
			{
				Projectile.velocity *= 0.4f;
				attacking = false;

				for (int k = 0; k < 9; k++)
				{
					Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), ModContent.DustType<Dusts.Glow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);
				}
			}
			else
			{
				float rotDifference = ((Projectile.DirectionTo(target.Center).ToRotation() - oldVel.ToRotation()) % 6.28f + 9.42f) % 6.28f - 3.14f;

				Vector2 direction = (oldVel.ToRotation() + rotDifference * arcTimer).ToRotationVector2();
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, MathHelper.Lerp(0.01f, 0.02f, ChargeRatio) + MathHelper.Max(0, 300 - Projectile.Distance(target.Center)) / 1500f);
			}

			for (int k = 0; k < 2; k++)
			{
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), ModContent.DustType<DianeGlow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);
			}

			if (Main.rand.NextBool(3))
			{
				var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
				d.customData = Main.rand.NextFloat(0.6f, 1.3f);
			}
		}

		private void IdleMovement(Player Player)
		{
			charge = Player.GetModPlayer<DianePlayer>().charge;
			speed = 20;
			Projectile.friendly = false;
			Vector2 direction = Player.Center - Projectile.Center;

			if (direction.Length() > 1500)
			{
				direction.Normalize();
				Projectile.Center = Player.Center + Main.rand.NextVector2Circular(100, 100);
			}

			if (direction.Length() > 100)
			{
				direction.Normalize();
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction.RotatedByRandom(1.5f) * speed, 0.01f);
			}
			else if (Projectile.velocity == Vector2.Zero)
			{
				Projectile.velocity = Projectile.DirectionTo(Player.Center);
			}

			if (ChargeRatio >= 1)
			{
				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), ModContent.DustType<DianeGlow>(), Vector2.Zero, 0, new Color(50, 50, 255), 0.4f);

				if (Main.rand.NextBool(6))
				{
					var d = Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(25), ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(20, 20, 100), 0.8f);
					d.customData = Main.rand.NextFloat(0.6f, 1.3f);
				}

				if (!fullyCharged)
				{
					fullyCharged = true;
					flashTimer = 0;
				}
			}
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["DatsuzeiTrail"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.TransformationMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.02f);
			effect.Parameters["repeats"].SetValue(8f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);
			effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Items/Moonstone/DatsuzeiFlameMap2").Value);

			trail?.Render(effect);

			effect.Parameters["sampleTexture2"].SetValue(TextureAssets.MagicPixel.Value);

			trail2?.Render(effect);

			spriteBatch.Begin(default, default, default, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
		}
	}

	public class DianeGlow : Glow
	{
		public override Color? GetAlpha(Dust dust, Color lightColor)
		{
			return dust.color * Math.Min(dust.fadeIn / 20f, 1);
		}

		public override bool Update(Dust dust)
		{
			dust.fadeIn++;
			dust.scale *= 1.02f;
			base.Update(dust);
			dust.shader.UseColor(dust.color * Math.Min(dust.fadeIn / 20f, 1));
			return false;
		}
	}
}