using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Items.SpaceEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Breacher
{
	public class MediRemote : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + "Scrapshot";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("M.E.D.I Remote");
			Tooltip.SetDefault("Summons a Medical Energy Delivery Instrument, to siphon health energy from your enemies\nAfter siphoning enough health energy, it drops a healing beacon\nOnly one can be summoned at a time");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 5;
			Item.knockBack = 0f;
			Item.mana = 30;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<MediRemoteSummonBuff>();
			Item.shoot = ModContent.ProjectileType<MediRemoteProjectile>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile proj = Main.projectile.Where(p => p.active && p.owner == player.whoAmI && p.type == type).FirstOrDefault();

			if (proj != default)
				proj.active = false;

			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, Main.MouseWorld, velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			for (int i = 0; i < 15; i++)
			{
				Dust.NewDustPerfect(Main.MouseWorld, ModContent.DustType<Dusts.GlowFastDecelerate>(),
					Main.rand.NextVector2CircularEdge(3f, 3f), 0, new Color(0, 100, 255), 0.5f);
			}

			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.MeteoriteBar, 10)
				.AddIngredient<Astroscrap>(5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}

	public class MediRemoteProjectile : ModProjectile
	{
		public ref float TargetWhoAmI => ref Projectile.ai[0];
		public ref float AttackTimer => ref Projectile.ai[1];

		public NPC Target => TargetWhoAmI > -1 ? Main.npc[(int)TargetWhoAmI] : null;
		public bool FoundTarget => Target != null;

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
		public override string Texture => AssetDirectory.BreacherItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Medical Energy Delivery Instrument");
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
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
		}

		public override void OnSpawn(IEntitySource source)
		{
			TargetWhoAmI = -1f;
		}

		public override void AI()
		{
			UpdateProjectileLifetime();

			if (MinionTarget != null)
				TargetWhoAmI = MinionTarget.whoAmI;

			if (!FoundTarget)
			{
				DoIdleMovement();

				if (TargetWhoAmI < 0)
				{
					NPC target = FindTarget();
					if (target != default)
						TargetWhoAmI = target.whoAmI;
				}
			}
			else
			{
				if (AttackTimer < 30)
					AttackTimer++;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, tex.Size() / 2f, Projectile.scale, Projectile.direction == 1 ? SpriteEffects.FlipHorizontally : 0, 0f);

			return false;
		}

		internal void DoIdleMovement()
		{
			Vector2 idlePos = Owner.Center + new Vector2(50f * Owner.direction, -125f);

			float dist = Vector2.Distance(Projectile.Center, idlePos);

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = 40f;
				if (dist < 1000f)
					speed = MathHelper.Lerp(5f, 40f, dist / 1000f);

				if (dist < 100f)
					speed = MathHelper.Lerp(0.1f, 5f, dist / 100f);

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

			Projectile.rotation = Projectile.velocity.X * 0.085f;
		}

		internal void UpdateProjectileLifetime()
		{
			if (Owner.HasBuff<MediRemoteSummonBuff>())
				Projectile.timeLeft = 2;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 1000f).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}
	}
}
