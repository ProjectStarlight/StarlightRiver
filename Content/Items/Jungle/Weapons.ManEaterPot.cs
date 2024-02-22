using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Jungle
{
	internal class ManEaterPot : ModItem
	{
		public override string Texture => AssetDirectory.JungleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Man Eater Pot");
			Tooltip.SetDefault("Causes man eaters to sprout from your head\n" +
				"These man eaters will consume heart pickups to empower themselves\n" +
				"Consumed heart pickups heal you after a delay");

			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 7;
			Item.knockBack = 5f;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.buyPrice(silver: 90);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<ManEaterPotBuff>();
			Item.shoot = ModContent.ProjectileType<ManEaterPotMinion>();
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(Item.buffType, 2);
			Projectile.NewProjectileDirect(source, player.Center + new Vector2(0, -32), velocity, type, damage, knockback, Main.myPlayer).originalDamage = Item.damage;

			for (int k = 0; k < 20; k++)
			{
				Dust.NewDustPerfect(player.Center + new Vector2(0, -48), DustID.JungleGrass, Vector2.UnitY.RotatedByRandom(1f) * -Main.rand.NextFloat(4));
			}

			for (int k = 0; k < 10; k++)
			{
				Dust.NewDustPerfect(player.Center + new Vector2(0, -48), DustID.Mud, Vector2.UnitY.RotatedByRandom(1f) * -Main.rand.NextFloat(3));
			}

			SoundEngine.PlaySound(SoundID.Grass, player.Center);

			return false;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.Vine, 2);
			recipe.AddIngredient(ItemID.JungleSpores, 6);
			recipe.AddIngredient(ItemID.ClayPot, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class ManEaterPotMinion : ModProjectile
	{
		public const int TARGET_RADIUS = 600;

		public Item heartTarget;
		public NPC npcTarget;

		public int animationTimer = 10000;

		public ref float RageTime => ref Projectile.ai[0];
		public ref float EatTime => ref Projectile.ai[1];
		public ref float State => ref Projectile.ai[2];

		public Player Owner => Main.player[Projectile.owner];

		public bool Raged => RageTime > 0;

		public Vector2 Origin => Owner.Center + new Vector2(0, -32 + Owner.gfxOffY);

		public override string Texture => AssetDirectory.JungleItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Tamed Man Eater");
		}

		public override void SetDefaults()
		{
			Projectile.width = 50;
			Projectile.height = 50;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.minion = true;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.timeLeft = 2;
		}

		public override void AI()
		{
			if (Owner.HasBuff<ManEaterPotBuff>())
				Projectile.timeLeft = 2;

			if (RageTime > 0)
				RageTime--;

			if (EatTime > 0)
				EatTime--;

			// Heal the player when the heart reaches them through the man eater
			if (EatTime == 1)
				Owner.Heal(10);

			if (Raged)
				Projectile.extraUpdates = 1;
			else
				Projectile.extraUpdates = 0;

			Projectile.rotation = Owner.Center.DirectionTo(Projectile.Center).ToRotation();

			if (State == 0)
			{
				FindTarget();

				// FindTarget can mutate state, we only want to do idle behavior if we're still idle after target searching
				if (State == 0)
					Idle();
			}

			if (State == 1)
				EatHeart();

			if (State == 2)
				AttackTarget();

			animationTimer--;

			if (animationTimer <= 2)
				animationTimer = 10000;
		}

		/// <summary>
		/// The minion picks a target, either a heart if one is around and it is not enraged, or an enemy otherwise
		/// </summary>
		public void FindTarget()
		{
			// If the minion isn't raged, it prioritizes eating the closest heart to get raged
			if (!Raged)
			{
				float longestDist = (float)Math.Pow(TARGET_RADIUS, 2) + 1;

				foreach (Item item in Main.item)
				{
					if (item.active && item.type == ItemID.Heart)
					{
						float dist2 = Vector2.DistanceSquared(item.Center, Owner.Center);
						if (dist2 <= Math.Pow(TARGET_RADIUS, 2) && dist2 < longestDist)
						{
							heartTarget = item;
							longestDist = dist2;
						}
					}
				}

				// If we found a valid heart, go into heart eating mode and stop searching
				if (heartTarget != null && heartTarget.active)
				{
					State = 1;
					return;
				}
			}

			// Else, fall back to targeting an enemy
			npcTarget = Projectile.TargetClosestNPC(TARGET_RADIUS, true, true);

			// If we found one, go into attack mode
			if (npcTarget != null && npcTarget.active)
				State = 2;
		}

		/// <summary>
		/// The minion having found a heart, attempts to move towards it, and when close enough 'eats' it,
		/// giving up if the heart its' trying to eat stops existing
		/// </summary>
		public void EatHeart()
		{
			// Abort if target is lost
			if (heartTarget is null || !heartTarget.active || heartTarget.type != ItemID.Heart)
			{
				heartTarget ??= null;
				State = 0;
				return;
			}

			Projectile.position += (heartTarget.Center - Projectile.Center) * 0.04f;

			Projectile.position += Projectile.Center.DirectionTo(Owner.Center) * (float)Math.Sin(animationTimer * 0.1f + Projectile.minionPos) * 2.9f;

			// If we intersect the heart, consume it
			if (Projectile.Hitbox.Intersects(heartTarget.Hitbox))
			{
				heartTarget.TurnToAir();
				heartTarget.active = false;
				RageTime = 600;
				EatTime = 120;
				State = 0;
				Projectile.netUpdate = true;
			}
		}

		/// <summary>
		/// The minion attempts to attack an NPC target to deal damage to it
		/// </summary>
		public void AttackTarget()
		{
			// Abort if target is lost
			if (npcTarget is null || !npcTarget.active)
			{
				npcTarget ??= null;
				State = 0;
				return;
			}

			Projectile.position += (npcTarget.Center - Projectile.Center) * 0.04f;

			Projectile.position += Projectile.Center.DirectionTo(Owner.Center) * (float)Math.Sin(animationTimer * 0.1f + Projectile.minionPos) * 2.9f;
		}

		/// <summary>
		/// The minion has no target and simply hangs around it's owner
		/// </summary>
		public void Idle()
		{
			float totalCount = Owner.ownedProjectileCounts[Type] > 0 ? Owner.ownedProjectileCounts[Type] : 1;

			Vector2 idlePos = Owner.Center + new Vector2(-32 + Projectile.minionPos / totalCount * 64, -72);
			idlePos += Vector2.UnitX.RotatedBy(animationTimer / 40f + Projectile.minionPos) * 10;

			Projectile.position += (idlePos - Projectile.Center) * 0.06f;

			Projectile.velocity *= 0.98f;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (Raged)
				modifiers.SourceDamage += 0.5f;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextBool(20))
				Item.NewItem(Projectile.GetSource_FromThis(), target.Hitbox, ItemID.Heart);

			// Retarget every hit
			State = 0;

			Owner.TryGetModPlayer(out StarlightPlayer starlightPlayer);
			starlightPlayer.SetHitPacketStatus(shouldRunProjMethods: true);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			int frameX = Raged ? 50 : 0;
			int frameY = Raged ? animationTimer / 10 % 2 * 38 : animationTimer / 10 % 4 * 38;
			var source = new Rectangle(frameX, frameY, 50, 38);

			float dist = Vector2.Distance(Projectile.Center, Origin);

			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Texture2D texVine = ModContent.Request<Texture2D>(Texture + "Vine").Value;
			Texture2D texBlob = ModContent.Request<Texture2D>(Texture + "Blob").Value;

			Texture2D texPot = ModContent.Request<Texture2D>(Texture + "Pot").Value;

			for (int k = texVine.Height; k < dist; k++)
			{
				float prog = k / dist;
				var pos = Vector2.Lerp(Projectile.Center, Origin, prog);

				Main.spriteBatch.Draw(texVine, pos - Main.screenPosition, null, Lighting.GetColor((pos / 16).ToPoint()), pos.DirectionTo(Origin).ToRotation() + 1.57f, texVine.Size() / 2f, 1, 0, 0);

				k += texVine.Height;
			}

			if (EatTime > 0)
			{
				var pos = Vector2.Lerp(Projectile.Center, Origin, 1 - EatTime / 120f);

				Main.spriteBatch.Draw(texBlob, pos - Main.screenPosition, null, lightColor, Projectile.rotation, texBlob.Size() / 2f, 1, 0, 0);
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, lightColor, Projectile.rotation + 1.57f, new Vector2(25, 17), 1, 0, 0);

			Main.spriteBatch.Draw(texPot, Origin - Main.screenPosition, null, Lighting.GetColor((Origin / 16).ToPoint()), 0, texPot.Size() / 2f, 1, 0, 0);

			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(animationTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			animationTimer = reader.ReadInt32();
		}
	}

	class ManEaterPotBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.Buffs + "ManEaterPotBuff";

		public ManEaterPotBuff() : base("Man Eaters", "Quite the hairdo you have!", false, true) { }

		public override void Update(Player Player, ref int buffIndex)
		{
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<ManEaterPotMinion>()] > 0)
			{
				Player.buffTime[buffIndex] = 18000;
			}
			else
			{
				Player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}