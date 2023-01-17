using StarlightRiver.Content.Buffs.Summon;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Magnet
{
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
			Item.knockBack = 3f;
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
			Item.buffType = BuffType<PalestoneSummonBuff>();
			Item.shoot = ProjectileType<PaleKnight>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;
			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.SilverBar, 12);
			recipe.AddRecipeGroup("StarlightRiver:BugShells");
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TungstenBar, 12);
			recipe.AddRecipeGroup("StarlightRiver:BugShells");
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class PaleKnight : ModProjectile
	{
		public const int maxMinionChaseRange = 2000;

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

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.tileCollide = true;

			Projectile.minion = true;

			Projectile.minionSlots = 1f;

			Projectile.penetrate = -1;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override void AI()
		{
			#region Active check
			if (Owner.dead || !Owner.active) // This is the "active check", makes sure the minion is alive while the Player is alive, and despawns if not
				Owner.ClearBuff(BuffType<PalestoneSummonBuff>());

			if (Owner.HasBuff(BuffType<PalestoneSummonBuff>()))
				Projectile.timeLeft = 2;
			#endregion

			#region Find target
			// Starting search distance
			Vector2 targetCenter = Projectile.Center;
			bool foundTarget = EnemyWhoAmI >= 0;

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

				if (NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
				}
				else
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
				}
			}

			if (!Owner.HasMinionAttackTargetNPC)
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

			#endregion
		}
	}
}