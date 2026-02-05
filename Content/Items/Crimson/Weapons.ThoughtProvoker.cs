using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Items.Palestone;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Core.Systems.PixelationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Crimson
{
	public class ThoughtProvoker : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thought Provoker");
			Tooltip.SetDefault(
				"Summons Thinky Jr to protect you\n" +
				"Endurance is increased by 5% per Thinky Jr alive\n" +
				"If hit by an enemy or projectile, Thinky Jr will target the nearest enemy and explode violently");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 45;
			Item.knockBack = 10f;
			Item.mana = 30;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 3);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<ThoughtProvokerSummonBuff>();
			Item.shoot = ModContent.ProjectileType<ThoughtProvokerProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			return false;
		}
	}

	public class ThoughtProvokerPlayer : ModPlayer
	{
		public override void UpdateEquips()
		{
			// 5% increased endurance per Thinky Jr
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<ThoughtProvokerProjectile>()] > 0)
				Player.endurance += 0.05f * Player.ownedProjectileCounts[ModContent.ProjectileType<ThoughtProvokerProjectile>()];
		}
	}

	public class ThoughtProvokerProjectile : ModProjectile
	{
		public int resetTimer;
		public int lifetime;
		public ref float TargetWhoAmI => ref Projectile.ai[1];
		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public bool FoundTarget => Target != null;

		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}
		public int Lifetime
		{
			get
			{
				if (Projectile.minionPos <= 0)
				{
					return lifetime;
				}
				else
				{
					Projectile proj = Main.projectile.Where(p => p.active && p.type == Type && p.owner == Projectile.owner && p.minionPos == 0).FirstOrDefault();
					if (proj != null)
					{
						var dagger = proj.ModProjectile as ThoughtProvokerProjectile;
						if (dagger != null)
							return dagger.lifetime;
					}
				}

				return 0;
			}
		}

		public const int maxMinionChaseRange = 500;
		public ref float AttackState => ref Projectile.ai[0];
		public ref float EnemyWhoAmI => ref Projectile.ai[1];
		public ref float AttackDelay => ref Projectile.ai[2];
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.CrimsonItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Thinky Junior");

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}
		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 24;

			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;

			Projectile.minion = true;

			Projectile.minionSlots = 1f;

			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.friendly = true;
		}

		public override void AI()
		{
			Player Player = Main.player[Projectile.owner];

			if (resetTimer > 0)
				resetTimer--;

			if (AttackDelay > 0)
				AttackDelay--;

			#region Active check
			if (Player.dead || !Player.active)
				Player.ClearBuff(ModContent.BuffType<ThoughtProvokerSummonBuff>());

			if (Player.HasBuff(ModContent.BuffType<ThoughtProvokerSummonBuff>()))
				Projectile.timeLeft = 2;

			lifetime++;

			//closest npc
			NPC target = FindTarget();
			if (target != default)
				TargetWhoAmI = target.whoAmI;

			#endregion

			if (AttackState == 0)
			{
				#region Shielding Behavior

				float totalCount = Owner.ownedProjectileCounts[Type] > 0 ? Owner.ownedProjectileCounts[Type] : 1;

				Vector2 idlePos = Owner.Center + new Vector2(0, 50).RotatedBy(MathHelper.ToRadians(Projectile.minionPos / totalCount * 360f + (Projectile.minionPos == totalCount ? 360f / totalCount : 0f)) + MathHelper.ToRadians(Lifetime));

				float dist = Vector2.Distance(Projectile.Center, idlePos);

				Vector2 toIdlePos = idlePos - Projectile.Center;
				if (toIdlePos.Length() < 0.0001f)
				{
					toIdlePos = Vector2.Zero;
				}
				else
				{
					float speed = 50f;
					if (dist < 1000f)
						speed = MathHelper.Lerp(15f, 50f, dist / 1000f);

					if (dist < 100f)
						speed = MathHelper.Lerp(3f, 15f, dist / 100f);

					toIdlePos.Normalize();
					toIdlePos *= speed;
				}

				Projectile.velocity = (Projectile.velocity * (5f - 1) + toIdlePos) / 5f;

				if (dist > 2000f)
				{
					Projectile.Center = idlePos;
					Projectile.velocity = Vector2.Zero;
					Projectile.netUpdate = true;
				}

				Projectile.rotation = Projectile.velocity.X * 0.05f;

				// checks if the projectile is colliding with a npc or hostile projectile hitbox

				Entity colliding = CheckCollisions(target);

				if (colliding != null && resetTimer <= 0)
				{
					//special case for projectiles
					if (colliding is Projectile)
						(colliding as Projectile).penetrate--;

					AttackState = 1;

					AttackDelay = 30;
					Projectile.velocity *= 0.05f;
					Projectile.velocity += Main.rand.NextVector2CircularEdge(10f, 10f);
				}
					

				#endregion
			}
			else
			{
				#region Exploding Behavior

				//prioritize minion target over closest npc
				if (MinionTarget != null)
					TargetWhoAmI = MinionTarget.whoAmI;

				if (Target is null)
				{
					TargetWhoAmI = -1;
					AttackDelay = 45;
				}

				if (AttackDelay <= 0)
				{
					Vector2 direction = Target.Center - Projectile.Center;
					direction.Normalize();
					if (Projectile.Distance(Target.Center) > 400f)
						direction *= 20f;
					else
					{
						float mult = MathHelper.Lerp(20f, 100f, 1f - Projectile.Distance(Target.Center) / 400f);
						direction *= mult;
					}
						
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.05f);
					Projectile.rotation += Projectile.velocity.Length() * 0.02f; // more aggressive spin when attacking

				}
				else
				{
					Projectile.rotation += Projectile.velocity.Length() * 0.01f;
					Projectile.velocity *= 0.985f;
				}

				if (AttackDelay == 1 && Target is null)
					ResetToIdle();

				#endregion
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = Assets.Items.Crimson.ThoughtProvokerProjectile.Value;

			float fade = 1f;
			if (resetTimer > 0)
				fade = 1f - resetTimer / 120f;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			return false;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.myPlayer == Projectile.owner)
				Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ThoughtProvokerExplosion>(), Projectile.damage * 2, 5f, Projectile.owner, 50);

			ResetToIdle();
		}
		public override bool? CanHitNPC(NPC target)
		{
			return target.whoAmI == TargetWhoAmI;
		}

		public override bool MinionContactDamage()
		{
			return AttackDelay <= 0 && AttackState == 1;
		}

		internal Entity CheckCollisions(NPC target)
		{
			// we dont want the shield to break if its not actively "protecting" its owner, for example on spawn
			if (Projectile.Distance(Owner.Center) > 75f)
				return null;

			Projectile closest = Main.projectile.Where(p => p.active && p.hostile && p.Distance(Projectile.Center) < 100f).OrderBy(p => p.Distance(Projectile.Center)).FirstOrDefault();

			if (closest != default && Projectile.Hitbox.Intersects(closest.Hitbox))
				return closest;

			if (target != null && Projectile.Hitbox.Intersects(target.Hitbox))
				return target;

			return null;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		internal void ResetToIdle()
		{
			AttackState = 0;
			Projectile.position = Owner.Center;
			resetTimer = 120;
			AttackDelay = 45;
		}
	}
	public class ThoughtProvokerExplosion : ModProjectile
	{
		private List<Vector2> cache;

		private Trail trail;
		private Trail trail2;
		public override string Texture => AssetDirectory.Invisible;
		private float Progress => Utils.Clamp(1 - Projectile.timeLeft / 30f, 0f, 1f);

		private float Radius => Projectile.ai[0] * Eases.EaseQuinticOut(Progress);

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 30;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Greymatter Explosion");
		}

		public override void AI()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				ManageCaches();
				ManageTrail();
			}

			/*for (int k = 0; k < 6; k++)
			{
				float rot = Main.rand.NextFloat(0, 6.28f);

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, DustID.Torch,
					Vector2.One.RotatedBy(rot) * 0.5f, 0, default, Main.rand.NextFloat(1.5f, 3f)).noGravity = true;

				Dust.NewDustPerfect(Projectile.Center + Vector2.One.RotatedBy(rot) * Radius, DustID.Torch,
					Vector2.One.RotatedBy(rot) * 0.5f + Main.rand.NextVector2Circular(2f, 2f), 50, default, Main.rand.NextFloat(0.5f, 1f));
			}*/
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
			line.Normalize();
			line *= Radius;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			//target.AddBuff(BuffID.OnFire, 300);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawPrimitives();
			return false;
		}

		private void ManageCaches()
		{
			if (cache is null)
			{
				cache = [];

				for (int i = 0; i < 40; i++)
				{
					cache.Add(Projectile.Center);
				}
			}

			for (int k = 0; k < 40; k++)
			{
				cache[k] = Projectile.Center + Vector2.One.RotatedBy(k / 38f * 6.28f) * Radius;
			}

			while (cache.Count > 40)
			{
				cache.RemoveAt(0);
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 40 * (1f - Progress), factor => Color.Lerp(new Color(255, 180, 20), new Color(255, 20, 20), Eases.EaseQuinticInOut(Progress)));

			trail2 ??= new Trail(Main.instance.GraphicsDevice, 40, new TriangularTip(1), factor => 30 * (1f - Progress), factor => Color.Lerp(new Color(255, 255, 255), new Color(255, 180, 20), Eases.EaseQuinticInOut(Progress)));

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[39];

			trail2.Positions = cache.ToArray();
			trail2.NextPosition = cache[39];
		}

		public void DrawPrimitives()
		{
			if (Projectile.timeLeft < 2)
				return;

			ModContent.GetInstance<PixelationSystem>().QueueRenderAction("UnderProjectiles", () =>
			{
				Effect effect = ShaderLoader.GetShader("CeirosRing").Value;

				if (effect != null)
				{
					var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
					Matrix view = Matrix.Identity;
					var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

					effect.Parameters["transformMatrix"].SetValue(world * view * projection);
					effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.01f);
					effect.Parameters["repeats"].SetValue(5f);
					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "FireTrail").Value);

					trail?.Render(effect);
					trail2?.Render(effect);

					effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>(AssetDirectory.Assets + "EnergyTrail").Value);

					trail2?.Render(effect);
				}
			});
		}
	}
}
