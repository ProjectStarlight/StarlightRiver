using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Items.Haunted;
using StarlightRiver.Content.Items.Palestone;
using System;
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
}
