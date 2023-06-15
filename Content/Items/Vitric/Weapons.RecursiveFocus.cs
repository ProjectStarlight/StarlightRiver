using StarlightRiver.Helpers;
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

					if (proj.active && proj.type == Item.shoot && proj.owner == player.whoAmI)
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

	public class RecursiveFocusGlobalNPC : GlobalNPC
	{
		public bool Targeted;
		public override bool InstancePerEntity => true;

		public override bool PreAI(NPC npc)
		{
			Targeted = Main.projectile.Any(p => p.active && p.type == ModContent.ProjectileType<RecursiveFocusLaser>() && (p.ModProjectile as RecursiveFocusLaser).TargetNPC == npc);

			return base.PreAI(npc);
		}
	}

	public class RecursiveFocusProjectile : ModProjectile
	{
		public bool MultiMode => Projectile.ai[0] != 0f;
		public ref float SwitchTimer => ref Projectile.ai[1];
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

			if (!Main.projectile.Any(p => p.active && p.owner == Owner.whoAmI && p.type == ModContent.ProjectileType<RecursiveFocusLaser>()))
			{
				if (MultiMode)
				{
					for (int i = 1; i < 4; i++)
					{
						Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RecursiveFocusLaser>(), Projectile.damage, 0f, Owner.whoAmI, 1f, 0f, i * 5);

						proj.originalDamage = Projectile.originalDamage;
						(proj.ModProjectile as RecursiveFocusLaser).parent = Projectile;
						(proj.ModProjectile as RecursiveFocusLaser).order = i;
					}
				}
				else
				{
					Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RecursiveFocusLaser>(), Projectile.damage, 0f, Owner.whoAmI);

					proj.originalDamage = Projectile.originalDamage;
					(proj.ModProjectile as RecursiveFocusLaser).parent = Projectile;
				}
			}

			if (Main.mouseRight && SwitchTimer <= 0f)
			{
				SwitchTimer = 60f;

				if (MultiMode)
					Projectile.ai[0] = 0f;
				else
					Projectile.ai[0] = 1f;
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
			Texture2D bloomTex = ModContent.Request<Texture2D>(Texture + "_Bloom").Value;

			Texture2D crystalTexOrange = ModContent.Request<Texture2D>(Texture + "_Orange").Value;
			Texture2D baseTexOrange = ModContent.Request<Texture2D>(Texture + "Base_Orange").Value;

			Texture2D crystalTexGlow = ModContent.Request<Texture2D>(Texture + "_Glow").Value;

			Main.spriteBatch.Draw(baseTex, Projectile.Center + new Vector2(0, 20) - Projectile.oldVelocity - Main.screenPosition, null, Color.White, Projectile.velocity.X * 0.075f, baseTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
			Main.spriteBatch.Draw(crystalTex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, crystalTex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);

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
	}

	public class RecursiveFocusLaser : ModProjectile
	{
		public int order; // each projectile needs a diff attack delay
		public Projectile parent;
		public NPC TargetNPC;
		public bool HasTarget => TargetNPC != null;
		public bool MultiMode => Projectile.ai[0] != 0f;
		public ref float TimeSpentOnTarget => ref Projectile.ai[1];
		public ref float AttackDelay => ref Projectile.ai[2];
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
			if (parent == null)
			{
				Projectile.Kill();
				return;
			}

			if (AttackDelay > 0)
				AttackDelay--;

			if (HasTarget)
			{
				if (TimeSpentOnTarget < 600)
					TimeSpentOnTarget++;

				if (!TargetNPC.active || TargetNPC.Distance(Projectile.Center) > 1000f || !Collision.CanHitLine(Owner.Center, 1, 1, TargetNPC.Center, 1, 1) || !Collision.CanHitLine(Projectile.Center, 1, 1, TargetNPC.Center, 1, 1))
				{
					TargetNPC = null;
					AttackDelay = 60 + order * 5; // offset the attack delays so they dont target the same thing (in multi mode)
				}
			}
			else
			{
				if (AttackDelay <= 0f)
					TargetNPC = FindTarget();

				TimeSpentOnTarget = 0;
			}

			UpdateProjectile();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (!HasTarget)
				return false;

			float useless = 0f;

			return TimeSpentOnTarget > 2 && Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, TargetNPC.Center, 15, ref useless);
		}

		internal void UpdateProjectile()
		{
			Projectile.Center = parent.Center;

			RecursiveFocusProjectile proj = parent.ModProjectile as RecursiveFocusProjectile;

			if (proj == null)
				return;

			bool wrongMode = proj.MultiMode && !MultiMode || !proj.MultiMode && MultiMode;

			if (!wrongMode)
				Projectile.timeLeft = 2;
		}

		internal NPC FindTarget()
		{
			return Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Projectile.Center) < 1000f && !n.GetGlobalNPC<RecursiveFocusGlobalNPC>().Targeted && (Collision.CanHitLine(Owner.Center, 1, 1, n.Center, 1, 1) || Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1))).OrderBy(n => n.Distance(Projectile.Center)).FirstOrDefault();
		}
	}
}