using StarlightRiver.Content.Buffs.Summon;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Starwood
{
	public class StarwoodScepter : StarwoodItem
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodScepter() : base(ModContent.Request<Texture2D>(AssetDirectory.StarwoodItem + "StarwoodScepter_Alt").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Scepter");
			Tooltip.SetDefault("Summons two halves of a sentient star");

			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 6;
			Item.knockBack = 3f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(silver: 25);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<StarwoodSummonBuff>();
			Item.shoot = ModContent.ProjectileType<StarwoodScepterSummonSplit>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);

			Projectile proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI, ai2: 1f);
			proj.originalDamage = Item.damage;

			Projectile proj2 = Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI, ai2: 0f);
			proj2.originalDamage = Item.damage;

			(proj.ModProjectile as StarwoodScepterSummonSplit).otherProj = proj2;
			(proj2.ModProjectile as StarwoodScepterSummonSplit).otherProj = proj;

			Helpers.DustHelper.DrawStar(Main.MouseWorld, ModContent.DustType<Dusts.GlowFastDecelerate>(), 5, 1, 1, 0.5f, 1, 0.5f, 0, -1, new Color(240, 200, 20));

			return false;
		}
	}

	public class StarwoodScepterSummonSplit : ModProjectile
	{
		public int lifetime;
		public int empowermentTimer;
		public Projectile otherProj;
		public Vector2 rotationalVelocity;
		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}

		public Vector2 IdlePosition
		{
			get
			{
				if (IsParent)
				{
					return Owner.Center + new Vector2(-25 * Owner.direction, -50) + new Vector2(-35 * Owner.direction * Projectile.minionPos, 0f) + new Vector2(MathHelper.Lerp(12f, 20f, Utils.Clamp((float)Math.Sin(lifetime * 0.015f), 0, 1))).RotatedBy(MathHelper.ToRadians(lifetime));
				}
				else
				{
					return Owner.Center + new Vector2(-25 * Owner.direction, -50) + new Vector2(-35 * Owner.direction * otherProj.minionPos, 0f) + new Vector2(MathHelper.Lerp(-12f, -20f, Utils.Clamp((float)Math.Sin(lifetime * 0.015f), 0, 1))).RotatedBy(MathHelper.ToRadians(lifetime));
				}
			}
		}

		public StarwoodScepterSummonSplit OtherStar => otherProj.ModProjectile as StarwoodScepterSummonSplit;
		public bool IsEmpowered => Owner.GetModPlayer<StarlightPlayer>().empowered;
		public bool FoundTarget => Target != null;
		public ref float TargetWhoAmI => ref Projectile.ai[0];
		public ref float AttackTimer => ref Projectile.ai[1];
		public bool IsParent => Projectile.ai[2] != 0f;
		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.StarwoodItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Split Star");
			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			Main.projFrames[Projectile.type] = 2;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(18);
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.minionSlots = 0.5f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
		}

		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
		}

		public override bool MinionContactDamage()
		{
			return AttackTimer <= 0;
		}

		public override void AI()
		{
			if (!IsParent)
			{
				TargetWhoAmI = OtherStar.TargetWhoAmI;
			}

			if (MinionTarget != null && AttackTimer <= 0 && IsParent)
				TargetWhoAmI = MinionTarget.whoAmI;

			if (!FoundTarget)
			{
				DoIdleMovement();

				NPC target = FindTarget();
				if (target != default)
					TargetWhoAmI = target.whoAmI;

				AttackTimer = 0;
			}
			else
			{
				if (AttackTimer > 0)
				{
					Projectile.velocity *= 0.95f;
					Projectile.rotation += 0.05f;
					AttackTimer--;
				}
				else
				{
					float distance = Vector2.Distance(Target.Center, Projectile.Center);

					Vector2 direction = Target.Center - Projectile.Center;
					direction.Normalize();
					direction *= 15f;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.085f);
				}

				Projectile.rotation += Projectile.velocity.Length() * 0.03f + 0.01f;

				if (!Target.active || Target.Distance(Owner.Center) > 1000f)
				{
					TargetWhoAmI = -1;
					AttackTimer = 0;
				}
			}

			UpdateProjectileLifetime();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target == Target)
			{
				AttackTimer = 40;
				Projectile.velocity *= -1f;
				Projectile.velocity += Main.rand.NextVector2CircularEdge(5f, 5f);
			}
		}

		internal void DoIdleMovement()
		{
			rotationalVelocity = Vector2.Lerp(rotationalVelocity, Projectile.DirectionTo(otherProj.Center), 0.05f);

			float dist = Vector2.Distance(Projectile.Center, IdlePosition);

			Vector2 toIdlePos = IdlePosition - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = 35f;
				if (dist < 1000f)
					speed = MathHelper.Lerp(10f, 25f, dist / 1000f);

				if (dist < 100f)
					speed = MathHelper.Lerp(0.1f, 10f, dist / 100f);

				toIdlePos.Normalize();
				toIdlePos *= speed;
			}

			Projectile.velocity = (Projectile.velocity * (25f - 1) + toIdlePos) / 25f;

			Projectile.rotation = rotationalVelocity.ToRotation() + MathHelper.ToRadians(0f);

			if (dist > 2000f)
			{
				Projectile.Center = Owner.Center;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
		}

		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<StarwoodSummonBuff>())
				Projectile.timeLeft = 2;

			if (!IsParent)
			{
				Dust.NewDustPerfect(Projectile.Center, DustID.Torch);
				lifetime = (otherProj.ModProjectile as StarwoodScepterSummonSplit).lifetime;
			}
			else
			{
				lifetime++;
			}
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}
	}

	public class StarwoodScepterSummonEmpowered : ModProjectile
	{
		public int[] Children = new int[2];
 		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}
		public bool IsEmpowered => Owner.GetModPlayer<StarlightPlayer>().empowered;
		public bool FoundTarget => Target != null;
		public ref float TargetWhoAmI => ref Projectile.ai[0];
		public ref float AttackDelay => ref Projectile.ai[1];
		public bool IsParent => Projectile.ai[2] != 0f;
		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public Player Owner => Main.player[Projectile.owner];
		public override string Texture => AssetDirectory.StarwoodItem + Name;
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Empowered Star");
			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void AI()
		{
			DoIdleMovement();

			UpdateProjectileLifetime();
		}

		internal void DoIdleMovement()
		{
			float distance = Projectile.Distance(Owner.Center);

			Projectile.rotation += Projectile.velocity.Length() * 0.01f;

			if (distance > 100f)
			{
				Projectile.velocity += Projectile.DirectionTo(Owner.Center) * 2f;
			}

			if (distance > 2000f)
			{
				Projectile.Center = Owner.Center;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
		}

		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<StarwoodSummonBuff>())
				Projectile.timeLeft = 2;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}
	}
}