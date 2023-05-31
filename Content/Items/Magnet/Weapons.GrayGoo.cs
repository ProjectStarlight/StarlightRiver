using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core.Systems.ScreenTargetSystem;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Magnet
{
	public class GrayGooDustData
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
		public override string Texture => AssetDirectory.MagnetItem + "GrayGoo";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");
			Tooltip.SetDefault("Summons a swarm of nanomachines to devour your enemies\n'Say the line, Armstrong!'");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
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
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType<GrayGooSummonBuff>();
			Item.shoot = ProjectileType<GrayGooProj>();
			Item.knockBack = 0;
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);

			position = Main.MouseWorld;

			var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = damage;
			return false;
		}
	}

	public class GrayGooProj : ModProjectile
	{
		public const int MAX_MINION_RANGE = 2000;

		public float lerper;

		public float oldLerper;

		public static ScreenTarget NPCTarget;

		public bool foundTarget;

		public float oldEnemyWhoAmI;

		public ref float EnemyWhoAmI => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void Load()
		{
			NPCTarget = new(DrawTarget, () => Main.projectile.Any(n => n.active && n.type == ProjectileType<GrayGooProj>()), 1);
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");

			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
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
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI != (int)EnemyWhoAmI)
				return false;

			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			if (Owner.dead || !Owner.active)
				Owner.ClearBuff(BuffType<GrayGooSummonBuff>());

			if (Owner.HasBuff(BuffType<GrayGooSummonBuff>()))
				Projectile.timeLeft = 2;

			Vector2 targetCenter = Projectile.Center;
			foundTarget = EnemyWhoAmI >= 0;

			var alreadyTargetted = new List<NPC>();
			var goos = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<GrayGooProj>() && n != Projectile && (n.ModProjectile as GrayGooProj).foundTarget).ToList();
			goos.ForEach(n => alreadyTargetted.Add(Main.npc[(int)(n.ModProjectile as GrayGooProj).EnemyWhoAmI]));

			if (Owner.HasMinionAttackTargetNPC)
			{
				NPC NPC = Main.npc[Owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(NPC.Center, Projectile.Center);
				if (between < MAX_MINION_RANGE)
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

				if (NPC.active && NPC.CanBeChasedBy() && betweenPlayer < MAX_MINION_RANGE)
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
				NPC target = Main.npc.Where(n =>
				n.CanBeChasedBy() &&
				n.Distance(Owner.Center) < MAX_MINION_RANGE &&
				Collision.CanHitLine(Projectile.position, 0, 0, n.position, 0, 0) &&
				!alreadyTargetted.Contains(n)).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();

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
				if (foundTarget)
					ReadjustDust();

				oldEnemyWhoAmI = EnemyWhoAmI;
			}

			if (!Main.dedServ && lerper < 1)
			{
				if (lerper == 0)
					KillDust();

				for (int i = 0; i < 5; i++)
				{
					Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
					Vector2 offset = Main.rand.NextVector2Circular(20, 20);
					var dust = Dust.NewDustPerfect(startPos, ModContent.DustType<GrayGooDust>(), Vector2.Zero, 0, Color.Transparent, 1);
					dust.customData = new GrayGooDustData((int)offset.X, (int)offset.Y, Projectile, Main.rand.NextFloat(0.07f, 0.17f), Main.rand.NextFloat(9, 20));
				}

				lerper += 0.1f;
			}

			if (foundTarget)
			{
				NPC actualTarget = Main.npc[(int)EnemyWhoAmI];

				if (!actualTarget.active)
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
					Projectile.velocity = Vector2.Zero;
				}

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(actualTarget.Center) * 16, 0.07f);
			}
			else
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * 16, 0.02f);
				EnemyWhoAmI = -1;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			float scale = 1;
			int amount = 5;

			if (target.life <= 0)
			{
				amount = 12;
				scale = 1.5f;
			}

			for (int i = 0; i < amount; i++)
			{
				Vector2 dir = Main.rand.NextVector2Circular(6, 6);
				Dust.NewDustPerfect(target.Center + dir, DustType<GrayGooSplashDust>(), dir, default, default, scale);
			}
		}

		private void ReadjustDust()
		{
			NPC target = Main.npc[(int)EnemyWhoAmI];

			foreach (Dust dust in Main.dust)
			{
				if (dust.type == DustType<GrayGooDust>() && dust.customData is GrayGooDustData data && data.proj == Projectile)
				{
					Vector2 offset = Main.rand.NextVector2Circular(target.width / 2, target.height / 2);
					data.x = (int)offset.X;
					data.y = (int)offset.Y;
				}
			}
		}

		private void KillDust()
		{
			foreach (Dust dust in Main.dust)
			{
				if (dust.type == DustType<GrayGooDust>() && dust.customData is GrayGooDustData data && data.proj == Projectile)
					dust.active = false;
			}
		}

		private static void DrawGooTarget(Projectile goo, SpriteBatch spriteBatch)
		{
			if (goo == default)
				return;

			var modproj = goo.ModProjectile as GrayGooProj;

			if (!modproj.foundTarget)
				return;

			NPC NPC = Main.npc[(int)modproj.EnemyWhoAmI];

			if (!NPC.active)
				return;

			if (NPC.active)
			{
				if (NPC.ModNPC != null)
				{
					ModNPC ModNPC = NPC.ModNPC;

					if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
						Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);

					ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));

				}
				else
				{
					Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);
				}
			}
		}

		private static void DrawTarget(SpriteBatch spriteBatch)
		{
			Main.graphics.GraphicsDevice.Clear(Color.Transparent);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			var goos = Main.projectile.Where(n => n.active && n.type == ProjectileType<GrayGooProj>()).ToList();
			goos.ForEach(n => DrawGooTarget(n, spriteBatch));
		}
	}

	public class GrayGooSplashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = false;
			dust.noLight = false;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity.Y += 0.2f;

			Tile tile = Framing.GetTileSafely((int)dust.position.X / 16, (int)dust.position.Y / 16);
			if (tile.HasTile && tile.BlockType == Terraria.ID.BlockType.Solid && Main.tileSolid[tile.TileType])
				dust.active = false;

			dust.rotation = dust.velocity.ToRotation();
			dust.scale *= 0.99f;

			if (dust.scale < 0.5f)
				dust.active = false;
			return false;
		}
	}

	public class GrayGooDust : Glow
	{
		public override bool Update(Dust dust)
		{
			var data = (GrayGooDustData)dust.customData;
			if (!data.proj.active)
			{
				dust.active = false;
				return false;
			}

			var MP = data.proj.ModProjectile as GrayGooProj;
			float lerper = data.lerp / 3;

			Vector2 entityCenter = MP.Owner.Center;
			if (MP.foundTarget)
			{
				NPC npc = Main.npc[(int)MP.EnemyWhoAmI];

				if (npc.active)
				{
					entityCenter = npc.Center;
					lerper *= 3;
				}
			}

			Vector2 posToBe = entityCenter + new Vector2(data.x, data.y);
			Terraria.Graphics.Shaders.ArmorShaderData unused = dust.shader.UseColor(dust.color);

			if ((posToBe - dust.position).Length() < 5)
			{
				dust.position = posToBe;
				dust.velocity = Vector2.Zero;
				return false;
			}

			Vector2 direction = dust.position.DirectionTo(posToBe);

			if (posToBe.Distance(dust.position) > 20)
				dust.velocity = Vector2.Lerp(dust.velocity, direction * data.speed, lerper);
			dust.position += dust.velocity;
			return false;
		}
	}
}