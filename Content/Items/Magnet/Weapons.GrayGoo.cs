//TODO:
//Balance
//Sellprice
//Rarity
//Obtainment
//Make the goo multiply
//Make the dust better tied to the projectile
//Item sprite
//Better use style

using Microsoft.Xna.Framework.Graphics.PackedVector;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Magnet
{
	public struct GrayGooDustData
	{
		public int x;

		public int y;

		public Projectile proj;

		public float speed;

		public float lerp;
		public GrayGooDustData(int x, int y, Projectile proj, float lerp, float speed)
		{
			this.x = x;
			this.y = y;
			this.proj = proj;
			this.lerp = lerp;
			this.speed = speed;
		}
	}

	public class GrayGoo : ModItem
	{
		public override string Texture => AssetDirectory.PalestoneItem + "PalestoneNail";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");
			Tooltip.SetDefault("Summons the Pale Knight");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(0, 0, 12, 0);
			Item.rare = ItemRarityID.White;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType<GrayGooSummonBuff>();
			Item.shoot = ProjectileType<GrayGooProj>();
			Item.knockBack = 0;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;

			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = damage;
			return false;
		}
	}

	public class GrayGooProj : ModProjectile
	{
		public const int maxMinionChaseRange = 2000;

		public float lerper;

		public float oldLerper;

		public static RenderTarget2D NPCTarget;

		public bool foundTarget;

		public float oldEnemyWhoAmI;

		public ref float EnemyWhoAmI => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void Load()
		{
			if (Main.dedServ)
				return;

			ResizeTarget();

			On.Terraria.Main.CheckMonoliths += DrawNPCtarget;
		}

		public static void ResizeTarget()
		{
            Main.QueueMainThreadAction(() => NPCTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
        }

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.tileCollide = false;

			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5000;
			Projectile.friendly = true;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

        public override bool MinionContactDamage() => true;

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI != (int)EnemyWhoAmI)
				return false;
            return base.CanHitNPC(target);
		}

		public override void AI()
		{
			#region Active check
			if (Owner.dead || !Owner.active) // This is the "active check", makes sure the minion is alive while the Player is alive, and despawns if not
				Owner.ClearBuff(BuffType<GrayGooSummonBuff>());

			if (Owner.HasBuff(BuffType<GrayGooSummonBuff>()))
				Projectile.timeLeft = 2;
			#endregion

			#region Find target
			// Starting search distance
			Vector2 targetCenter = Projectile.Center;
			foundTarget = EnemyWhoAmI >= 0;

			// This code is required if your minion weapon has the targeting feature
			if (Owner.HasMinionAttackTargetNPC)
			{
				NPC NPC = Main.npc[Owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(NPC.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
					EnemyWhoAmI = NPC.whoAmI;
					foundTarget = true;
				}
			}
			else if (foundTarget)
			{
				NPC NPC = Main.npc[(int)EnemyWhoAmI];
				float betweenPlayer = Vector2.Distance(NPC.Center, Owner.Center);

				if (NPC.active && NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
				}
				else
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
				}
			}

			else if (!Owner.HasMinionAttackTargetNPC)
			{
				NPC target = Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < maxMinionChaseRange && Collision.CanHitLine(Projectile.position, 0, 0, n.position, 0, 0)).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
				if (target != default)
				{
					targetCenter = target.Center;
					EnemyWhoAmI = target.whoAmI;
					foundTarget = true;
				}
				else
				{
					EnemyWhoAmI = 0;
					foundTarget = false;
				}
			}

			if (EnemyWhoAmI != oldEnemyWhoAmI)
			{
				lerper = 0;
				oldEnemyWhoAmI = EnemyWhoAmI;
			}

			#endregion

			if (foundTarget)
			{
				NPC actualTarget = Main.npc[(int)EnemyWhoAmI];

				if (!actualTarget.active)
				{
					Projectile.velocity = Vector2.Zero;
					lerper = 0;
					KillDust();
				}

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(actualTarget.Center) * 6, 0.07f);
				if (!Main.dedServ && foundTarget && lerper < 1)
				{
					for (int i = 0; i < 5; i++)
					{
						Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
						Vector2 offset = Main.rand.NextVector2Circular(actualTarget.width / 2, actualTarget.height / 2);
						Dust dust = Dust.NewDustPerfect(startPos, ModContent.DustType<GrayGooDust>(), startPos.DirectionTo(actualTarget.Center), 0, Color.Transparent, 1);
						dust.customData = new GrayGooDustData((int)offset.X, (int)offset.Y, Projectile, Main.rand.NextFloat(0.02f, 0.07f), Main.rand.NextFloat(3, 8));
					}
					lerper += 0.1f;
				}
			}
			else
			{
                Projectile.velocity = Vector2.Zero;
                lerper = 0;
                KillDust();
            }
		}

		private void KillDust()
		{
            foreach (Dust dust in Main.dust)
            {
                if (dust.type == ModContent.DustType<GrayGooDust>() && dust.customData is GrayGooDustData data && data.proj == Projectile)
                {
                    dust.active = false;
                }
            }
        }

		private void DrawNPCtarget(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			orig();

			Projectile localGoo = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<GrayGooProj>()).OrderByDescending(n => n.timeLeft).FirstOrDefault();
			if (localGoo == default)
				return;

			GrayGooProj modproj = localGoo.ModProjectile as GrayGooProj;

			if (!modproj.foundTarget)
				return;

			NPC NPC = Main.npc[(int)modproj.EnemyWhoAmI];

			if (!NPC.active)
				return;

			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (Main.gameMenu || Main.dedServ || spriteBatch is null || NPCTarget is null || gD is null)
				return;

			RenderTargetBinding[] bindings = gD.GetRenderTargets();
			gD.SetRenderTarget(NPCTarget);
			gD.Clear(Color.Transparent);

			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			if (NPC.active)
			{
				if (NPC.ModNPC != null)
				{
					if ( NPC.ModNPC is ModNPC ModNPC)
					{
						if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
							Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);

						ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
					}
				}
				else
				{
					Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);
				}
			}
			spriteBatch.End();
            gD.SetRenderTargets(bindings);
		}
	}

    public class GrayGooDust : Glow
    {
        public override bool Update(Dust dust)
        {
			GrayGooDustData data = (GrayGooDustData)dust.customData;
			if (!data.proj.active)
			{
				dust.active = false;
				return false;
			}

			var MP = data.proj.ModProjectile as GrayGooProj;

			if (!MP.foundTarget)
            {
                dust.active = false;
                return false;
            }

            var npc = Main.npc[(int)MP.EnemyWhoAmI];

			if (!npc.active)
            {
                dust.active = false;
                return false;
            }

            Vector2 posToBe = npc.Center + new Vector2(data.x, data.y);
            dust.shader.UseColor(dust.color);

            if ((posToBe - dust.position).Length() < 5)
            {
                dust.position = posToBe;
                dust.velocity = Vector2.Zero;
                return false;
            }

            Vector2 direction = dust.position.DirectionTo(posToBe);
			if (posToBe.Distance(dust.position) > 20)
				dust.velocity = Vector2.Lerp(dust.velocity, direction * data.speed, data.lerp);
            dust.position += dust.velocity;
            return false;
        }
    }
}