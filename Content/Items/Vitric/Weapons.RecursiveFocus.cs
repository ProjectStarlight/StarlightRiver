using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Vitric
{
	public class RecursiveFocus : ModItem
	{
		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Summons an infernal crystal\n" +
				"The infernal crystal locks onto enemies, ramping up damage over time\n" +
				"Press <right> to cause the crystal to target multiple enemies, at the cost of causing all beams to not ramp up, dealing less damage");
		}

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.QueenSpiderStaff);
			Item.sentry = true;
			Item.damage = 6;
			Item.DamageType = DamageClass.Summon;
			Item.mana = 15;
			Item.knockBack = 0f;

			Item.rare = ItemRarityID.Orange;

			Item.UseSound = SoundID.Item25;
			Item.useStyle = ItemUseStyleID.HoldUp;

			Item.shoot = ModContent.ProjectileType<RecursiveFocusProjectile>();
			Item.shootSpeed = 1f;

			Item.useTime = Item.useAnimation = 35;

			Item.value = Item.sellPrice(gold: 2, silver: 75);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (player.ownedProjectileCounts[Item.shoot] > 0)
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];

					if (proj.active && (proj.type == Item.shoot || proj.type == ModContent.ProjectileType<RecursiveFocusLaser>()) && proj.owner == player.whoAmI)
						proj.Kill();
				}
			}

			Projectile.NewProjectileDirect(source, player.Center, velocity, type, damage, knockback, player.whoAmI).originalDamage = Item.damage;
			player.UpdateMaxTurrets();

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<VitricOre>(20);
			recipe.AddIngredient<MagmaCore>();
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class RecursiveFocusProjectile : ModProjectile
	{
		public float TimeSpentOnTarget
		{
			get
			{
				Projectile proj = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<RecursiveFocusLaser>() && (p.ModProjectile as RecursiveFocusLaser).parent == Projectile && !(p.ModProjectile as RecursiveFocusLaser).MultiMode).FirstOrDefault();
				if (proj != null)
				{
					return (proj.ModProjectile as RecursiveFocusLaser).TimeSpentOnTarget;
				}

				return 0;
			}
		}

		public bool HasSingleTarget
		{
			get
			{
				Projectile proj = Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<RecursiveFocusLaser>() && (p.ModProjectile as RecursiveFocusLaser).parent == Projectile && !(p.ModProjectile as RecursiveFocusLaser).MultiMode).FirstOrDefault();
				if (proj != null)
				{
					return (proj.ModProjectile as RecursiveFocusLaser).HasTarget;
				}

				return false;
			}
		}

		public int pulseTimer;

		public Vector2 LineOfSightPos => Owner.Center + new Vector2(0f, -70); // needed for LOS check

		public NPC[] targets = new NPC[3]; // for multi mode
		public bool MultiMode => Projectile.ai[0] != 0f;
		public ref float SwitchTimer => ref Projectile.ai[1];
		public ref float SwitchCooldown => ref Projectile.ai[2];
		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.VitricItem + Name;

		public override void Load()
		{
			for (int i = 1; i < 5; i++)
			{
				GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore" + i);
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Infernal Crystal");

			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;

			Projectile.sentry = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;

			Projectile.width = Projectile.height = 26;

			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
		}

		public override void AI()
		{
			if (SwitchTimer > 0)
				SwitchTimer--;

			if (SwitchCooldown > 0)
				SwitchCooldown--;

			if (pulseTimer > 0)
				pulseTimer--;

			PopulateTargets();

			if (!Main.projectile.Any(p => p.active && p.owner == Owner.whoAmI && p.type == ModContent.ProjectileType<RecursiveFocusLaser>()) && SwitchTimer <= 0)
			{
				if (MultiMode)
				{
					for (int i = 0; i < 3; i++)
					{
						Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RecursiveFocusLaser>(), Projectile.damage, 0f, Owner.whoAmI, 1f, 0f);

						proj.originalDamage = Projectile.originalDamage;
						(proj.ModProjectile as RecursiveFocusLaser).parent = Projectile;
						(proj.ModProjectile as RecursiveFocusLaser).order = i;
						(proj.ModProjectile as RecursiveFocusLaser).lifetime = i * Main.rand.Next(200, 600);
					}
				}
				else
				{
					Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RecursiveFocusLaser>(), Projectile.damage, 0f, Owner.whoAmI);

					proj.originalDamage = Projectile.originalDamage;
					(proj.ModProjectile as RecursiveFocusLaser).parent = Projectile;
				}
			}

			if (Main.mouseRight && Main.mouseRightRelease && SwitchCooldown <= 0 && Owner.HeldItem.type == ModContent.ItemType<RecursiveFocus>())
			{
				SwitchCooldown = 240f;
				SwitchTimer = 60f;
				Helper.PlayPitched("Magic/FireCast", 1f, 0f, Projectile.Center);
			}

			if (SwitchTimer > 0)
			{
				float lerper = 1f - SwitchTimer / 60f;

				float off = MathHelper.Lerp(100f, 10f, lerper);

				Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2CircularEdge(off, off), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(0.5f, 0.5f), 0, new Color(255, 150, 50), 0.5f);

				if (SwitchTimer == 1)
				{
					if (MultiMode)
						Projectile.ai[0] = 0f;
					else
						Projectile.ai[0] = 1f;

					Helper.PlayPitched("Magic/FireHit", 1f, 0f, Projectile.Center);

					for (int i = 0; i < 20; i++)
					{
						Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(255, 150, 50), 0.5f);
					}

					CameraSystem.shake += 10;

					pulseTimer = 15;
				}
			}

			DoMovement();
		}

		internal void DoMovement()
		{
			var idlePos = new Vector2(Owner.Center.X, Owner.Center.Y - 70);

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = Vector2.Distance(idlePos, Projectile.Center) * 0.2f;
				speed = Utils.Clamp(speed, 1f, 25f);
				toIdlePos.Normalize();
				toIdlePos *= speed;
			}

			Projectile.velocity = (Projectile.velocity * (45f - 1) + toIdlePos) / 45f;

			if (Vector2.Distance(Projectile.Center, idlePos) > 2000f)
			{
				Projectile.Center = idlePos;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D baseTex = ModContent.Request<Texture2D>(Texture + "Base").Value;
			Texture2D crystalTex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Texture2D crystalTexOrange = ModContent.Request<Texture2D>(Texture + "_Orange").Value;
			Texture2D baseTexOrange = ModContent.Request<Texture2D>(Texture + "Base_Orange").Value;

			Texture2D baseTexGlow = ModContent.Request<Texture2D>(Texture + "Base_Glow").Value;
			Texture2D crystalTexGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Main.spriteBatch.Draw(baseTex, Projectile.Center + new Vector2(0, 20) - Projectile.oldVelocity - Main.screenPosition, null, Color.White, Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(crystalTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

			if (HasSingleTarget)
			{
				Main.spriteBatch.Draw(baseTexOrange, Projectile.Center + new Vector2(0, 20) - Projectile.oldVelocity - Main.screenPosition, null, Color.White * (TimeSpentOnTarget / 540f), Projectile.velocity.X * 0.075f, baseTexOrange.Size() / 2f, Projectile.scale, 0f, 0f);
				Main.spriteBatch.Draw(baseTexGlow, Projectile.Center + new Vector2(0, 20) - Projectile.oldVelocity - Main.screenPosition, null, new Color(255, 165, 115, 0) * (TimeSpentOnTarget / 540f), Projectile.velocity.X * 0.075f, baseTexGlow.Size() / 2f, Projectile.scale, 0f, 0f);

				Main.spriteBatch.Draw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.White * (TimeSpentOnTarget / 540f), Projectile.rotation, crystalTexOrange.Size() / 2f, Projectile.scale, 0f, 0f);			
			}

			if (pulseTimer > 0)
			{
				if (TimeSpentOnTarget > 179)
				{
					Main.spriteBatch.Draw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, new Color(255, 150, 100, 0) * 0.5f * (pulseTimer / 15f), Projectile.rotation, crystalTexGlow.Size() / 2f, Projectile.scale * MathHelper.Lerp(1f, 3f, 1f - pulseTimer / 15f), 0f, 0f);
				}
				else
				{
					Main.spriteBatch.Draw(crystalTexOrange, Projectile.Center - Main.screenPosition, null, Color.White * (pulseTimer / 15f), Projectile.rotation, crystalTexOrange.Size() / 2f, Projectile.scale * MathHelper.Lerp(1f, 3f, 1f - pulseTimer / 15f), 0f, 0f);
				}
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			for (int i = 1; i < 5; i++)
			{
				Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, Mod.Find<ModGore>(Name + "_Gore" + i).Type, 1f).timeLeft = 90;
			}

			SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
		}

		internal void PopulateTargets()
		{
			if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
				targets[0] = Main.npc[Owner.MinionAttackTargetNPC];

			for (int i = 0; i < 3; i++)
			{
				if (targets[i] == null)
					targets[i] = Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1000f && !AlreadyTargeted(n) && CheckLOS(n)).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
				else if (!targets[i].active || targets[i].Distance(Projectile.Center) > 1000f || !CheckLOS(targets[i]))
					targets[i] = null;
			}
		}

		internal bool AlreadyTargeted(NPC target)
		{
			if (targets[0] == target || targets[1] == target || targets[2] == target)
				return true;

			return false;
		}

		internal bool CheckLOS(NPC target)
		{
			int checkCount = 3;

			if (!Collision.CanHit(Owner.Center, 1, 1, target.Center, 1, 1))
				checkCount--;

			if (!Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1))
				checkCount--;

			if (!Collision.CanHit(LineOfSightPos, 1, 1, target.Center, 1, 1))
				checkCount--;

			return checkCount > 0;
		}
	}

	public class RecursiveFocusLaser : ModProjectile
	{
		private List<Vector2> cache;
		private Trail trail;
		private Trail trail2;
		private Trail trail3;
		private Trail trail4;

		public int order;
		public Projectile parent;
		public NPC targetNPC;

		public int lifetime; // for trail drawing
		public int pulseTimer;
		public int trailFade;

		public int Stage
		{
			get
			{
				if (MultiMode)
					return 0;

				if (TimeSpentOnTarget >= 540)
					return 3;
				else if (TimeSpentOnTarget >= 360)
					return 2;
				else if (TimeSpentOnTarget >= 180)
					return 1;

				return 0;
			}
		}

		public RecursiveFocusProjectile crystal => parent.ModProjectile as RecursiveFocusProjectile;
		public bool HasTarget => targetNPC != null;
		public bool MultiMode => Projectile.ai[0] != 0f;
		public ref float TimeSpentOnTarget => ref Projectile.ai[1];
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.Invisible;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Infernal Laser");

			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;

			Projectile.timeLeft = 5;

			Projectile.width = Projectile.height = 26;

			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;

			Projectile.usesLocalNPCImmunity = true; //otherwise it hogs all iframes, making nothing else able to hit
			Projectile.localNPCHitCooldown = 12;

			Projectile.ContinuouslyUpdateDamageStats = true;
		}

		public override void AI()
		{
			if (parent == null || !parent.active)
			{
				Projectile.Kill();
				return;
			}

			if (pulseTimer > 0)
				pulseTimer--;

			lifetime++;

			if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f && !MultiMode)
				targetNPC = Main.npc[Owner.MinionAttackTargetNPC];

			if (HasTarget)
			{
				if (!Main.dedServ)
				{
					ManageCaches();
					ManageTrail();
				}

				if (TimeSpentOnTarget < 540)
					TimeSpentOnTarget++;

				if (TimeSpentOnTarget % 179 == 0 && !MultiMode && Stage < 3)
				{
					pulseTimer = 15;

					BezierCurve curve = GetBezierCurve();

					int points = 26;
					Vector2[] curvePositions = curve.GetPoints(points).ToArray();

					for (int i = 0; i < 26; i++)
					{
						for (int d = 0; d < 2; d++)
						{
							Dust.NewDustPerfect(curvePositions[i], ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(255, 150, 50), 0.7f);
						}
					}

					Main.NewText(TimeSpentOnTarget + " : " + Stage);

					crystal.pulseTimer = 15;
				}

				if (Main.rand.NextBool(3))
				{
					BezierCurve curve = GetBezierCurve();

					int points = 26;
					Vector2[] curvePositions = curve.GetPoints(points).ToArray();

					Dust.NewDustPerfect(curvePositions[Main.rand.Next(points)], ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(255, 150, 50), 0.4f);
				}

				if (Main.rand.NextBool(8))
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(255, 150, 50), 0.4f);
				}

				if (trailFade < 15)
					trailFade++;

				if (!targetNPC.active || targetNPC.Distance(Projectile.Center) > 1000f || !crystal.CheckLOS(targetNPC))
				{
					targetNPC = null;
					TimeSpentOnTarget = 0;
					trailFade = 0;
				}  
			}
			else
			{
				targetNPC = MultiMode ? crystal.targets[order] : FindTarget();

				TimeSpentOnTarget = 0;
				trailFade = 0;
			}

			UpdateProjectile();
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (!MultiMode)
				modifiers.SourceDamage *= 1 + Stage;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 4; i++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(4f, 4f), 0, new Color(255, 150, 50), MultiMode ? 0.3f : 0.6f);

				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(Projectile.Center).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.5f, 1f), 0, new Color(255, 150, 50), MultiMode ? 0.3f : 0.6f);

				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(Projectile.Center).RotatedByRandom(0.25f) * Main.rand.NextFloat(2f, 10f), 0, new Color(255, 150, 50), MultiMode ? 0.3f : 0.6f);
			}
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!HasTarget)
				return false;

			float useless = 0f;

			return TimeSpentOnTarget > 2 && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, targetNPC.Center, 15, ref useless);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D baseTex = ModContent.Request<Texture2D>(crystal.Texture + "Base").Value;
			Texture2D crystalTex = ModContent.Request<Texture2D>(crystal.Texture).Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Texture2D crystalTexOrange = ModContent.Request<Texture2D>(crystal.Texture + "_Orange").Value;
			Texture2D baseTexOrange = ModContent.Request<Texture2D>(crystal.Texture + "Base_Orange").Value;

			Texture2D crystalTexGlow = ModContent.Request<Texture2D>(crystal.Texture + "_Glow").Value;

			DrawTrail(Main.spriteBatch);

			if (HasTarget)
			{
				float fadeIn = 1f;
				if (trailFade < 15)
					fadeIn = trailFade / 15f;

				float scale = 1f + Stage * 0.55f;

				if (MultiMode)
					scale = 2f;

				Effect effect = Filters.Scene["DistortSprite"].GetShader().Shader;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

				effect.Parameters["time"].SetValue((float)Main.timeForVisualEffects * 0.075f);
				effect.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.0075f);
				effect.Parameters["screenPos"].SetValue(Main.screenPosition * new Vector2(0.5f, 0.1f) / new Vector2(Main.screenWidth, Main.screenHeight));

				effect.Parameters["offset"].SetValue(new Vector2(0.001f));
				effect.Parameters["repeats"].SetValue(1);
				effect.Parameters["uImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "Noise/SwirlyNoiseLooping").Value);
				effect.Parameters["uImage2"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.VitricBoss + "LaserBallDistort").Value);

				Color color = new Color(255, 165, 115, 0) * 0.4f * fadeIn * (MultiMode ? 1f : (TimeSpentOnTarget / 540f));
				if (pulseTimer > 0)
					color = Color.Lerp(new Color(255, 150, 100, 0) * 0.5f, color, 1f - pulseTimer / 15f);

				if (MultiMode)
					color *= 0.25f;

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.Parameters["noiseImage1"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "MagicPixel").Value);

				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.White, 0f, crystalTexGlow.Size() / 2f, 1.25f, 0f, 0f);

				color = new Color(255, 150, 50, 0) * 0.5f * fadeIn * (MultiMode ? 1f : (TimeSpentOnTarget / 540f));
				if (pulseTimer > 0)
					color = Color.Lerp(new Color(255, 150, 100, 0) * 0.5f, color, 1f - pulseTimer / 15f);
				
				if (MultiMode)
					color *= 0.25f;

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.White, 0f, crystalTexGlow.Size() / 2f, 1.25f, 0f, 0f);

				color = new Color(255, 150, 50, 0) * 0.15f * fadeIn * (MultiMode ? 1f : (TimeSpentOnTarget / 540f));
				if (pulseTimer > 0)
					color = Color.Lerp(new Color(255, 150, 100, 0) * 0.15f, color, 1f - pulseTimer / 15f);

				if (MultiMode)
					color *= 0.25f;

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(crystalTexGlow, Projectile.Center - Main.screenPosition, null, Color.White, 0f, crystalTexGlow.Size() / 2f, 2.25f, 0f, 0f);

				color = new Color(255, 150, 50, 0) * fadeIn;

				effect.Parameters["uColor"].SetValue(color.ToVector4());
				effect.CurrentTechnique.Passes[0].Apply();

				Main.spriteBatch.Draw(bloomTex, targetNPC.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.4f * scale, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, targetNPC.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.3f * scale, 0f, 0f);

				Main.spriteBatch.Draw(bloomTex, targetNPC.Center - Main.screenPosition, null, Color.White, 0f, bloomTex.Size() / 2f, 0.1f * scale, 0f, 0f);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				color = new Color(255, 165, 115, 0);
				if (pulseTimer > 0)
					color = Color.Lerp(new Color(255, 150, 100, 0), color, 1f - pulseTimer / 15f);

				if (MultiMode)
					color *= 0.5f;

				Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, color, 0f, bloomTex.Size() / 2f, 0.2f * scale, 0f, 0f);
			}

			return false;
		}

		internal void UpdateProjectile()
		{
			Projectile.Center = parent.Center;

			if (crystal == null)
				return;

			bool wrongMode = crystal.MultiMode && !MultiMode || !crystal.MultiMode && MultiMode;

			if (!wrongMode && crystal.SwitchTimer <= 0)
				Projectile.timeLeft = 2;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1000f && crystal.CheckLOS(n)).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		#region PRIMITIVE DRAWING
		private BezierCurve GetBezierCurve()
		{
			Vector2[] curvePoints =
			{
				Vector2.Lerp(Projectile.Center + parent.velocity, targetNPC.Center, 0.2f) + new Vector2(0f, -40f * (float)Math.Sin(lifetime * 0.05f)).RotatedBy(Projectile.DirectionTo(targetNPC.Center).ToRotation()),
				Vector2.Lerp(Projectile.Center + parent.velocity, targetNPC.Center, 0.4f) + new Vector2(0f, 80f * (float)Math.Cos(lifetime * -0.075f)).RotatedBy(Projectile.DirectionTo(targetNPC.Center).ToRotation()),
				Vector2.Lerp(Projectile.Center + parent.velocity, targetNPC.Center, 0.6f) + new Vector2(0f, 50f *(float)Math.Sin(lifetime * -0.05f)).RotatedBy(Projectile.DirectionTo(targetNPC.Center).ToRotation()),
				Vector2.Lerp(Projectile.Center + parent.velocity, targetNPC.Center, 0.8f) + new Vector2(0f, -30f *(float)Math.Cos(lifetime * 0.075f)).RotatedBy(Projectile.DirectionTo(targetNPC.Center).ToRotation()),
			};

			var curve = new BezierCurve(new Vector2[] { 
				Projectile.Center + parent.velocity, 
				curvePoints[0],
				curvePoints[1],
				curvePoints[2],
				curvePoints[3],
				targetNPC.Center 
			});

			return curve;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = new List<Vector2>();

				for (int i = 0; i < 26; i++)
				{
					cache.Add(Projectile.Center + parent.velocity);
				}
			}

			for (int k = 0; k < 26; k++)
			{
				BezierCurve curve = GetBezierCurve();

				int points = 26;
				Vector2[] curvePositions = curve.GetPoints(points).ToArray();

				cache[k] = curvePositions[k];
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(4), factor => MultiMode ? 4 : 4 * 1 + Stage, factor => 
			{
				if (trailFade < 15)
					return Color.Lerp(Color.Transparent, new Color(255, 165, 115), trailFade / 15f) * MathHelper.Lerp(0f, 0.6f, trailFade / 15f);

				if (pulseTimer > 0)
					return Color.Lerp(Color.White, new Color(255, 165, 115), 1f - pulseTimer / 15f) * 0.6f;

				return new Color(255, 165, 115) * 0.6f;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[25];

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(4), factor => MultiMode ? 4 : 4 * 1 + Stage, factor =>
			{
				if (trailFade < 15)
					return Color.Lerp(Color.Transparent, new Color(255, 150, 50), trailFade / 15f) * MathHelper.Lerp(0f, 0.6f, trailFade / 15f);

				if (pulseTimer > 0)
					return Color.Lerp(new Color(255, 165, 115), new Color(255, 150, 50), 1f - pulseTimer / 15f) * 0.6f;

				return new Color(255, 150, 50) * 0.6f;
			});

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[25];

			trail3 ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(4), factor => MultiMode ? 7 : 5 * 1 + Stage, factor =>
			{
				if (trailFade < 15)
					return Color.Lerp(Color.Transparent, new Color(255, 165, 115), trailFade / 15f) * MathHelper.Lerp(0f, 0.6f, trailFade / 15f);

				if (pulseTimer > 0)
					return Color.Lerp(Color.White, new Color(255, 165, 115), 1f - pulseTimer / 15f) * 0.6f;

				return new Color(255, 165, 115) * 0.6f;
			});

			trail3.Positions = cache.ToArray();
			trail3.NextPosition = cache[25];

			trail4 ??= new Trail(Main.instance.GraphicsDevice, 26, new TriangularTip(4), factor => MultiMode ? 7 : 5 * 1 + Stage, factor =>
			{
				if (trailFade < 15)
					return Color.Lerp(Color.Transparent, new Color(255, 150, 50), trailFade / 15f) * MathHelper.Lerp(0f, 0.6f, trailFade / 15f);

				if (pulseTimer > 0)
					return Color.Lerp(new Color(255, 165, 115), new Color(255, 150, 50), 1f - pulseTimer / 15f) * 0.6f;

				return new Color(255, 150, 50) * 0.6f;
			});

			trail4.Positions = cache.ToArray();
			trail4.NextPosition = cache[25];
		}

		private void DrawTrail(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			/*effect.Parameters["intensity"].SetValue(new Vector2(0.1f, 0.15f));
			effect.Parameters["sinTime"].SetValue(Lifetime * 0.075f);*/
			effect.Parameters["time"].SetValue(lifetime * -0.02f);
			effect.Parameters["repeats"].SetValue(1);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

			if (HasTarget && TimeSpentOnTarget > 2)
			{
				trail?.Render(effect);
				trail2?.Render(effect);

				effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

				trail?.Render(effect);
				trail3?.Render(effect);

				effect.Parameters["time"].SetValue(lifetime * 0.05f);
				trail2?.Render(effect);
				trail4?.Render(effect);
			}

			spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
		}

		#endregion PRIMITIVEDRAWING
	}
}