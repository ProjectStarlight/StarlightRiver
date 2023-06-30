using StarlightRiver.Core.Systems.ExposureSystem;
using Terraria.Audio;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Forest
{
	[AutoloadEquip(EquipType.Head)]
	public class SlimePrinceHead : ModItem
	{
		public Projectile prince;
		public Vector2 targetVel;

		public SlimePrinceMinion Minion => prince?.ModProjectile as SlimePrinceMinion;

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void Load()
		{
			StarlightPlayer.PreUpdateMovementEvent += SlimeMovement;
			On_Player.KeyDoubleTap += ActivateMerge;

			StarlightPlayer.FreeDodgeEvent += AbosrbSlime;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Crown");
			Tooltip.SetDefault("10% increased summoning damage");
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "A slime prince follows you around\nDouble tap DOWN to fuse with the prince\nYou can control the prince during this time\nThe prince takes damage instead of you during this time";

			// If the prince is invalid, we need to spawn a new prince
			if (prince is null || !prince.active || prince.type != ProjectileType<SlimePrinceMinion>() || prince.owner != player.whoAmI)
				prince = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ProjectileType<SlimePrinceMinion>(), 20, 0, player.whoAmI);
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 2;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<SlimePrinceHead>() && body.type == ItemType<SlimePrinceChest>() && legs.type == ItemType<SlimePrinceLegs>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Summon) += 0.1f;
		}

		/// <summary>
		/// This triggers the set bonus on double tap, so long as the prince's life is full
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="keyDir"></param>
		private void ActivateMerge(On_Player.orig_KeyDoubleTap orig, Player self, int keyDir)
		{
			if (keyDir == 0 && self.armor[0].type == ItemType<SlimePrinceHead>())
			{
				var helm = self.armor[0].ModItem as SlimePrinceHead;

				if (helm.Minion != null)
				{
					if (!helm.Minion.Merged && helm.Minion.life >= SlimePrinceMinion.MAX_LIFE)
					{
						helm.Minion.State = 2;
						helm.Minion.Timer = 0;
					}
					else if (helm.Minion.Merged)
					{
						helm.Minion.State = 0;
						helm.Minion.Timer = 0;
					}
				}
			}

			orig(self, keyDir);
		}

		/// <summary>
		/// This overrides the player's velocity while they are merged with the slime prince,
		/// to provide a simple custom omnidirectional movement
		/// </summary>
		/// <param name="player"></param>
		private void SlimeMovement(Player player)
		{
			var helm = player.armor[0].ModItem as SlimePrinceHead;

			// Slow the player down while they're transforming
			if (helm?.Minion?.State == 2)
			{
				player.velocity *= 0.99f;
				targetVel *= 0;
			}

			// Custom input handling
			if (helm?.Minion?.Merged ?? false)
			{
				targetVel *= 0.96f;

				if (player.controlLeft)
					targetVel.X -= 1;

				if (player.controlRight)
					targetVel.X += 1;

				if (player.controlUp)
					targetVel.Y -= 1;

				if (player.controlDown)
					targetVel.Y += 1;

				if (targetVel.Length() > 10)
					targetVel = Vector2.Normalize(targetVel) * 9.99f;

				player.velocity = targetVel;
			}
		}

		/// <summary>
		/// Handles the prince abosrbing damage instead of damaging the player
		/// while they are fused together
		/// </summary>
		/// <param name="player"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		private bool AbosrbSlime(Player player, Player.HurtInfo info)
		{
			var helm = player.armor[0].ModItem as SlimePrinceHead;

			if (helm?.Minion?.Merged ?? false)
			{
				helm.Minion.life -= info.Damage * 4;
				player.immuneTime = 15;
				player.immune = true;

				CombatText.NewText(player.Hitbox, Color.Gray, info.Damage);
				SoundEngine.PlaySound(SoundID.NPCHit1, player.Center);

				return true;
			}

			return false;
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class SlimePrinceChest : ModItem
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Curiass");
			Tooltip.SetDefault("5% increased summoning damage\nYou can summon an additional minion");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 4;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<SlimePrinceHead>() && body.type == ItemType<SlimePrinceChest>() && legs.type == ItemType<SlimePrinceLegs>();
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Summon) += 0.05f;
			player.maxMinions += 1;
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class SlimePrinceLegs : ModItem
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void Load()
		{
			StarlightProjectile.OnHitNPCEvent += InflictExposure;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Tassets");
			Tooltip.SetDefault("Minions inflict 5% exposure");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 3;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<SlimePrinceHead>() && body.type == ItemType<SlimePrinceChest>() && legs.type == ItemType<SlimePrinceLegs>();
		}

		private void InflictExposure(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
		{
			Player owner = Main.player[projectile.owner];

			if (owner.armor[2].type == Type && projectile.minion)
			{
				ExposureNPC exposure = target.GetGlobalNPC<ExposureNPC>();

				if (exposure.ExposureMultAll < 0.05f)
					exposure.ExposureMultAll = 0.05f;
			}
		}
	}
}
