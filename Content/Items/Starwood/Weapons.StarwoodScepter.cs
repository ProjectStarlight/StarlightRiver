using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Buffs.Summon;
using System;
using System.IO;
using System.Linq;
using Terraria.Audio;
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
			Tooltip.SetDefault("Summons two halves of a sentient star\nThe stars drop mana stars when hitting summon tagged enemies");

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
			player.AddBuff(Item.buffType, 2, false);

#pragma warning disable IDE0007
			Projectile proj = Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI, ai2: 1f);
			proj.originalDamage = Item.damage; //original damage doesn't need to be synced so this is okay

			StarwoodScepterSummonSplit.otherProjToAssign = proj.whoAmI;
			Projectile proj2 = Projectile.NewProjectileDirect(source, Main.MouseWorld, Main.rand.NextVector2CircularEdge(5f, 5f), type, damage, knockback, player.whoAmI, ai2: 0f);
			proj2.originalDamage = Item.damage;
#pragma warning restore

			(proj.ModProjectile as StarwoodScepterSummonSplit).otherProj = proj2;

			Helpers.DustHelper.DrawStar(Main.MouseWorld, ModContent.DustType<Dusts.GlowFastDecelerate>(), 5, 1, 1, 0.5f, 1, 1f, 0, -1, new Color(240, 200, 20));

			return false;
		}
	}

	public class StarwoodScepterSummonSplit : ModProjectile
	{
		public static int otherProjToAssign; //only really relevant for first frame on proj2 so it can visually spawn and split away instead of waiting for the first netupdate

		public int lifetime;

		public int empowermentTimer;

		public float targetWhoAmI;

		public Projectile otherProj;

		public Vector2 rotationalVelocity = Vector2.Zero;

		public StarwoodScepterSummonSplit OtherStar => otherProj.ModProjectile as StarwoodScepterSummonSplit;

		public Projectile EmpoweredStar => Main.projectile.Where(p => p.active && p.type == ModContent.ProjectileType<StarwoodScepterSummonEmpowered>() && (p.ModProjectile as StarwoodScepterSummonEmpowered).Children.Contains(Projectile.whoAmI)).FirstOrDefault();
		public bool HasEmpoweredStar => EmpoweredStar != null;
		public bool IsEmpowered => Owner.GetModPlayer<StarlightPlayer>().empowered;

		public bool FoundTarget => Target != null;
		public NPC Target => targetWhoAmI > -1 ? Main.npc[(int)targetWhoAmI] : null;

		public ref float DowntimeTimer => ref Projectile.ai[0];
		public ref float AttackTimer => ref Projectile.ai[1];
		public bool IsParent => Projectile.ai[2] != 0f;

		public Player Owner => Main.player[Projectile.owner];

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
					return Owner.Center + new Vector2(-25 * Owner.direction, -50) + new Vector2(-35 * Owner.direction * Projectile.minionPos, 0f) + new Vector2(MathHelper.Lerp(5f, 15f, Utils.Clamp((float)Math.Sin(lifetime * 0.015f), 0, 1))).RotatedBy(MathHelper.ToRadians(lifetime));
				else
					return Owner.Center + new Vector2(-25 * Owner.direction, -50) + new Vector2(-35 * Owner.direction * otherProj.minionPos, 0f) + new Vector2(-MathHelper.Lerp(5f, 15f, Utils.Clamp((float)Math.Sin(lifetime * 0.015f), 0, 1))).RotatedBy(MathHelper.ToRadians(lifetime));
			}
		}

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
			targetWhoAmI = -1f;
			rotationalVelocity = Projectile.velocity;
			otherProj = Main.projectile[otherProjToAssign];
		}

		public override bool MinionContactDamage()
		{
			return AttackTimer <= 0 && !HasEmpoweredStar;
		}

		public override void AI()
		{
			if (IsEmpowered)
				EmpoweredBehavior();
			else
				DefaultBehavior();

			UpdateProjectileLifetime();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target == Target)
			{
				AttackTimer = 40;
				if (Main.myPlayer == Projectile.owner)
				{
					Projectile.velocity *= -1f;
					Projectile.velocity += Main.rand.NextVector2CircularEdge(5f, 5f);
					Projectile.netUpdate = true;
				}

				SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
			}

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragmentYellow>(), Projectile.velocity.RotatedByRandom(0.65f) * Main.rand.NextFloat(0.35f), 100, Color.White with { A = 0 }, 1.25f);
			}

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Projectile.velocity.RotatedByRandom(0.65f) * Main.rand.NextFloat(0.35f), 0, Main.rand.NextBool() ? new Color(255, 200, 0) : new Color(255, 150, 0), 0.35f);
			}

			bool hasTag = false;

			for (int i = 0; i < target.buffType.Length; i++)
			{
				if (BuffID.Sets.IsATagBuff[target.buffType[i]])
					hasTag = true;
			}

			if (Main.rand.NextBool(10) && Owner.HasMinionAttackTargetNPC && Owner.MinionAttackTargetNPC == target.whoAmI && hasTag)
			{
				if (Projectile.owner == Main.myPlayer)
				{
					int newItem = Item.NewItem(target.GetSource_Loot(), Projectile.Hitbox, ItemID.Star); // Create a new item in the world.
					Main.item[newItem].noGrabDelay = 0; // Set the new item to be able to be picked up instantly

					// Here we need to make sure the item is synced in multiplayer games.
					if (Main.netMode == NetmodeID.MultiplayerClient && newItem >= 0)
						NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);
				}
			}

			Main.player[Projectile.owner].TryGetModPlayer<StarlightPlayer>(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D blurTex = ModContent.Request<Texture2D>(Texture + "_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			if (HasEmpoweredStar)
				return false;

			Rectangle frame = tex.Frame(verticalFrames: 2, frameY: Projectile.frame);
			Rectangle glowFrame = glowTex.Frame(verticalFrames: 2, frameY: Projectile.frame);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 200, 0, 0) * 0.15f, Projectile.rotation, bloomTex.Size() / 2f, 0.75f, 0f, 0f);

			Main.spriteBatch.Draw(blurTex, Projectile.Center - Main.screenPosition, glowFrame, Color.White with { A = 0 }, Projectile.rotation, glowFrame.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(blurTex, Projectile.Center - Main.screenPosition, glowFrame, Color.White with { A = 0 } * 0.4f, Projectile.rotation, glowFrame.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(glowTex, Projectile.Center - Main.screenPosition, glowFrame, new Color(255, 155, 0, 0) * 0.15f, Projectile.rotation, glowFrame.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(255, 155, 0, 0) * 0.01f, Projectile.rotation, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

			return false;
		}

		/// <summary>
		/// Contains all of the projectiles behavior when it is NOT empowered. This includes finding targets, idling, and attacking.
		/// </summary>
		internal void DefaultBehavior()
		{
			if (empowermentTimer > 0)
				empowermentTimer = 0;

			if (!IsParent)
				targetWhoAmI = OtherStar.targetWhoAmI;

			if (MinionTarget != null && AttackTimer <= 0 && IsParent)
				targetWhoAmI = MinionTarget.whoAmI;

			if (DowntimeTimer > 0)
			{
				Projectile.velocity *= 0.95f;
				Projectile.rotation += 0.05f;
				rotationalVelocity = Projectile.rotation.ToRotationVector2();
				DowntimeTimer--;
				return;
			}

			if (!FoundTarget)
			{
				DoIdleMovement();

				if (Main.rand.NextBool(90))
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, Main.rand.NextBool() ? new Color(255, 200, 0) : new Color(255, 150, 0), 0.35f);

				if (IsParent)
				{
					NPC target = FindTarget();

					if (target != default)
						targetWhoAmI = target.whoAmI;
				}

				AttackTimer = 0;
			}
			else
			{
				if (AttackTimer > 0)
				{
					Projectile.velocity *= 0.95f;
					Projectile.rotation += 0.05f;

					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, Main.rand.NextBool() ? new Color(255, 200, 0) : new Color(255, 150, 0), 0.2f);

					AttackTimer--;
				}
				else
				{
					float distance = Vector2.Distance(Target.Center, Projectile.Center);

					Vector2 direction = Target.Center - Projectile.Center;
					direction.Normalize();
					direction *= 15f;
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.085f);

					if (Main.rand.NextBool(2))
						Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.5f), 0, Main.rand.NextBool() ? new Color(255, 200, 0) : new Color(255, 150, 0), 0.3f);

					if (Main.rand.NextBool(5))
						Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragmentYellow>(), -Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.65f), 0, Color.White with { A = 0 }, 1f);
				}

				Projectile.rotation += Projectile.velocity.Length() * 0.03f + 0.01f;

				rotationalVelocity = Projectile.rotation.ToRotationVector2();

				if (!Target.active || Target.Distance(Owner.Center) > 1000f)
				{
					targetWhoAmI = -1;
					AttackTimer = 0;
				}
			}
		}

		/// <summary>
		/// Includes the behavior for doing the empowerment animation, as well as the minimal behavior while there as an empowered star present.
		/// </summary>
		internal void EmpoweredBehavior()
		{
			if (HasEmpoweredStar)
			{
				Projectile.Center = EmpoweredStar.Center; // the split stars never actually go away, the need to stay alive to occupy the players minion slots. which is why the empowered star has no minion slot value
			}
			else if (empowermentTimer < 60)
			{
				if (empowermentTimer == 0)
				{
					if (Main.myPlayer == Projectile.owner)
					{
						Projectile.velocity += Main.rand.NextVector2CircularEdge(10f, 10f);
						Projectile.netUpdate = true;
					}

					for (int i = 0; i < 10; i++)
					{
						Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(255, 20, 0), 0.3f);
					}
				}

				if (empowermentTimer < 25)
				{
					Projectile.velocity *= 0.94f;
					Projectile.rotation += Projectile.velocity.Length() * 0.02f;
					rotationalVelocity = Projectile.rotation.ToRotationVector2();
				}
				else
				{
					if (empowermentTimer == 25)
						SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/ImpactHeal"), Projectile.Center);

					rotationalVelocity = Vector2.Lerp(rotationalVelocity, Projectile.DirectionTo(otherProj.Center), 0.15f);
					Projectile.rotation = rotationalVelocity.ToRotation();

					Projectile.Center = Vector2.Lerp(Projectile.Center, otherProj.Center, EaseBuilder.EaseCubicIn.Ease((empowermentTimer - 25f) / 45f));
				}

				if (Main.rand.NextBool(5))
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(4f, 4f), 0, Main.rand.NextBool() ? new Color(255, 200, 0) : new Color(255, 150, 0), 0.4f);

				if (Main.rand.NextBool(10))
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragmentYellow>(), Main.rand.NextVector2Circular(4f, 4f), 0, Color.White with { A = 0 }, 1.25f);

				empowermentTimer++;
			}
			else if (IsParent) // only spawn the projectile on the parent
			{
				if (Main.myPlayer == Projectile.owner)
				{
					StarwoodScepterSummonEmpowered.childrenToAssign = new int[] { Projectile.whoAmI, otherProj.whoAmI };
					int downtimeTimer = 35;
					var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center,
						Main.rand.NextVector2CircularEdge(5f, 5f), ModContent.ProjectileType<StarwoodScepterSummonEmpowered>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: downtimeTimer);

					proj.originalDamage = (int)(Projectile.originalDamage * 2.5); // this doesn't actually need to be synced so it can be assigned like this
				}

				empowermentTimer = 0;

				Helpers.DustHelper.DrawStar(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), 5, 1.5f, 1.5f, 0.5f, 1, 1f, 0, -1, new Color(0, 0, 255));

				for (int i = 0; i < 20; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragment>(), Main.rand.NextVector2Circular(5f, 5f), 0, Color.White with { A = 0 }, 2.5f);
				}
			}
		}

		/// <summary>
		/// Performs the idle movement for the Projectile
		/// </summary>
		internal void DoIdleMovement()
		{
			if (lifetime > 2) // otherwise you get NaNs from DirectionTo
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

			Projectile.rotation = rotationalVelocity.ToRotation();

			if (dist > 2000f)
			{
				Projectile.Center = Owner.Center;
				Projectile.velocity *= 0.95f;
				Projectile.netUpdate = true;
			}
		}

		/// <summary>
		/// Updates the projectiles lifetime according to the buff, as well as some stuff for the parent / non parent behavior
		/// </summary>
		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<StarwoodSummonBuff>())
				Projectile.timeLeft = 2;

			if (!IsParent)
			{
				Projectile.frame = 1;
				lifetime = (otherProj.ModProjectile as StarwoodScepterSummonSplit).lifetime;
			}
			else
			{
				lifetime++;
			}
		}

		/// <summary>
		/// Finds the closest valid target to the Projectile's Center
		/// </summary>
		/// <returns></returns>
		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(otherProj.identity);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int otherProjIdentity = reader.ReadInt32();
			otherProj = Main.projectile.FirstOrDefault(n => n.active && n.identity == otherProjIdentity);
		}
	}

	public class StarwoodScepterSummonEmpowered : ModProjectile
	{
		public static int[] childrenToAssign;

		public float targetWhoAmI;

		public int[] Children = new int[2];

		public Projectile[] ProjectileChildren => new Projectile[2] { Main.projectile[Children[0]], Main.projectile[Children[1]] };

		public bool IsEmpowered => Owner.GetModPlayer<StarlightPlayer>().empowered;

		public NPC Target => targetWhoAmI > -1 ? Main.npc[(int)targetWhoAmI] : null;
		public bool FoundTarget => Target != null;

		public ref float DowntimeTimer => ref Projectile.ai[0];
		public ref float AttackTimer => ref Projectile.ai[1];
		public bool IsParent => Projectile.ai[2] != 0f;

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public NPC MinionTarget
		{
			get
			{
				if (Owner.HasMinionAttackTargetNPC && Main.npc[Owner.MinionAttackTargetNPC].Distance(Projectile.Center) < 1000f)
					return Main.npc[Owner.MinionAttackTargetNPC];

				return null;
			}
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Empowered Star");
			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs

			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(20);
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;

			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Children[0] = childrenToAssign[0]; //split to avoid aliasing if you have multiple of these active
			Children[1] = childrenToAssign[1];
		}

		public override bool MinionContactDamage()
		{
			return FoundTarget;
		}

		public override void Kill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.DD2_WitherBeastHurt, Projectile.Center);

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragment>(), Main.rand.NextVector2Circular(5f, 5f), 100, Color.White with { A = 0 }, 2f);
			}

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(0, 0, 255), 0.65f);
			}
		}

		public override void AI()
		{
			if (!IsEmpowered)
			{
				if (Main.myPlayer == Projectile.owner)
				{
					ProjectileChildren[0].velocity += Main.rand.NextVector2Circular(8f, 8f);
					ProjectileChildren[1].velocity += Main.rand.NextVector2Circular(8f, 8f);

					(ProjectileChildren[0].ModProjectile as StarwoodScepterSummonSplit).DowntimeTimer = 35;
					(ProjectileChildren[1].ModProjectile as StarwoodScepterSummonSplit).DowntimeTimer = 35;

					ProjectileChildren[0].netUpdate = true;
					ProjectileChildren[1].netUpdate = true;
				}

				Projectile.Kill();
			}

			if (DowntimeTimer > 0)
			{
				Projectile.velocity *= 0.935f;
				Projectile.rotation += Projectile.velocity.Length() * 0.01f + 0.05f;
				DowntimeTimer--;
			}
			else
			{
				if (!FoundTarget)
				{
					DoIdleMovement();

					if (Main.rand.NextBool(60))
						Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 0, new Color(0, 0, 255), 0.35f);

					NPC target = FindTarget();

					if (target != default)
						targetWhoAmI = target.whoAmI;
				}
				else
				{
					AttackTimer++;

					if (AttackTimer < 15f)
					{
						Projectile.velocity *= 0.94f;
						Projectile.rotation += MathHelper.Lerp(0.05f, 1f, EaseBuilder.EaseCircularIn.Ease(AttackTimer / 15f));
					}
					else
					{
						if (AttackTimer == 15f)
						{
							Projectile.velocity += Projectile.DirectionTo(Target.Center) * 10f;
							SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);

							Helpers.DustHelper.DrawStar(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), 5, 1f, 0.65f, 0.65f, 1, 1f, 0, Main.rand.NextFloat(-1f, 1f), new Color(0, 0, 255));
						}

						float distance = Vector2.Distance(Target.Center, Projectile.Center);

						Vector2 direction = Target.Center - Projectile.Center;
						direction.Normalize();
						direction *= 22f;
						Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction, 0.08f);

						if (AttackTimer > 35 || Vector2.Distance(Target.Center, Projectile.Center) < 15f)
						{
							DowntimeTimer = 15;
							AttackTimer = 0;
						}

						Projectile.rotation += Projectile.velocity.Length() * 0.05f;

						if (AttackTimer > 15f)
						{
							Vector2 pos = Projectile.Center + new Vector2(5, MathHelper.Lerp(-15f, 15f, (AttackTimer - 15f) / 20f)).RotatedBy(Projectile.velocity.ToRotation());
							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(0.25f, 0.25f), 0, new Color(0, 0, 255), 0.35f);

							pos = Projectile.Center + new Vector2(5, MathHelper.Lerp(15f, -15f, (AttackTimer - 15f) / 20f)).RotatedBy(Projectile.velocity.ToRotation());
							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(0.25f, 0.25f), 0, new Color(0, 0, 255), 0.35f);
						}
					}

					if (!Target.active || Target.Distance(Owner.Center) > 1000f)
					{
						targetWhoAmI = -1;
						AttackTimer = 0;
					}
				}
			}

			UpdateProjectileLifetime();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (target == Target)
			{
				Helpers.DustHelper.DrawStar(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), 5, 1f, 1f, 0.55f, 1, 1f, 0, Main.rand.NextFloat(-1f, 1f), new Color(0, 0, 255));

				for (int i = 0; i < 10; i++)
				{
					Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.StarFragment>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f), 100, Color.White with { A = 0 }, 2f);
				}
			}

			for (int i = 0; i < 10; i++)
			{
				Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), -Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f), 0, new Color(0, 0, 255), 0.65f);
			}

			Projectile.velocity *= 1.25f;

			target.AddBuff(ModContent.BuffType<StarstruckDebuff>(), 600, true);

			Main.player[Projectile.owner].TryGetModPlayer<StarlightPlayer>(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D blurTex = ModContent.Request<Texture2D>(Texture + "_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			lightColor = Color.White;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				float lerper = i / (float)Projectile.oldPos.Length;

				Main.spriteBatch.Draw(blurTex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, Color.White with { A = 0 } * 0.75f * (1f - lerper),
					Projectile.oldRot[i], blurTex.Size() / 2f, Projectile.scale * MathHelper.Lerp(1f, 0.35f, lerper), 0, 0);

				Main.spriteBatch.Draw(tex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, Color.White * 0.35f * (1f - lerper),
					Projectile.oldRot[i], tex.Size() / 2f, Projectile.scale * MathHelper.Lerp(1f, 0.35f, lerper), 0, 0);

				Main.spriteBatch.Draw(bloomTex, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, new Color(0, 0, 255, 0) * 0.75f * (1f - lerper),
					Projectile.rotation, bloomTex.Size() / 2f, Projectile.scale * MathHelper.Lerp(0.25f, 0.05f, lerper), 0, 0);
			}

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(0, 0, 255, 0), Projectile.rotation, bloomTex.Size() / 2f, 0.75f, 0f, 0f);

			Main.spriteBatch.Draw(blurTex, Projectile.Center - Main.screenPosition, null, lightColor with { A = 0 }, Projectile.rotation, blurTex.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(blurTex, Projectile.Center - Main.screenPosition, null, lightColor with { A = 0 } * 0.5f, Projectile.rotation, blurTex.Size() / 2f, Projectile.scale, 0f, 0f);

			Main.spriteBatch.Draw(bloomTex, Projectile.Center - Main.screenPosition, null, new Color(0, 0, 255, 0) * 0.35f, Projectile.rotation, bloomTex.Size() / 2f, 0.65f, 0f, 0f);

			return false;
		}

		/// <summary>
		/// Performs the idle movement for the Projectile
		/// </summary>
		internal void DoIdleMovement()
		{
			Vector2 IdlePosition = Owner.Center + new Vector2(-25f * Owner.direction, -50f) + new Vector2(-25f * Owner.direction * ProjectileChildren[0].minionPos, ProjectileChildren[0].minionPos % 2 == 0 ? -25f : 0f);

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

			Projectile.rotation += 0.005f + Projectile.velocity.Length() * 0.025f;

			if (dist > 2000f)
			{
				Projectile.Center = Owner.Center;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}
		}

		/// <summary>
		/// Updates the Projectile's timeLeft according to the buff
		/// </summary>
		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<StarwoodSummonBuff>())
				Projectile.timeLeft = 2;
		}

		/// <summary>
		/// Finds the closest valid target to the Projectile's Center
		/// </summary>
		/// <returns></returns>
		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(Main.projectile[Children[0]].identity);
			writer.Write(Main.projectile[Children[1]].identity);
			writer.Write(Projectile.originalDamage);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			int childIdentity1 = reader.ReadInt32();
			Children[0] = Main.projectile.FirstOrDefault(n => n.active && n.identity == childIdentity1).whoAmI;

			int childIdentity2 = reader.ReadInt32();
			Children[1] = Main.projectile.FirstOrDefault(n => n.active && n.identity == childIdentity2).whoAmI;

			Projectile.originalDamage = reader.ReadInt32();
		}
	}

	class StarstruckDebuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public StarstruckDebuff() : base("Starstruck", "Reach for the stars", true) { }

		public override void Load()
		{
			StarlightNPC.UpdateLifeRegenEvent += ApplyDot;
		}

		private void ApplyDot(NPC npc, ref int damage)
		{
			if (Inflicted(npc))
			{
				npc.lifeRegen -= 16;
				if (damage < 1)
					damage = 1;
			}
		}

		public override void Update(NPC npc, ref int buffIndex)
		{
			if (Main.rand.NextBool(7))
				Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 100, new Color(255, 255, 0), 0.5f);

			if (Main.rand.NextBool(7))
				Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(5f, 5f), 100, new Color(0, 0, 255), 0.5f);

			if (Main.rand.NextBool(30))
				Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.StarFragment>(), Main.rand.NextVector2Circular(5f, 5f), 100, Color.White with { A = 0 }, 2f);

			if (Main.rand.NextBool(30))
				Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(npc.width, npc.height), ModContent.DustType<Dusts.StarFragmentYellow>(), Main.rand.NextVector2Circular(5f, 5f), 100, Color.White with { A = 0 }, 1f);
		}
	}
}