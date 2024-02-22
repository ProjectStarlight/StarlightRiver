using StarlightRiver.Core.Systems.ExposureSystem;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Forest
{
	[AutoloadEquip(EquipType.Head)]
	public class SlimePrinceHead : ModItem
	{
		public Projectile prince;
		public Vector2 targetVel;
		public float accel = 0.06f;
		public Vector2 targetAccel;

		public SlimePrinceMinion Minion => prince?.ModProjectile as SlimePrinceMinion;

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void Load()
		{
			StarlightPlayer.PreUpdateMovementEvent += SlimeMovement;
			On_Player.KeyDoubleTap += ActivateMerge;

			StarlightPlayer.FreeDodgeEvent += AbsorbDamage;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Crown");
			Tooltip.SetDefault("10% increased summoning damage");

			ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 2;
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "A slime prince follows you around\nDouble tap DOWN to fuse with the prince\nYou can control the prince during this time\nThe prince takes damage instead of you during this time";

			if (player.whoAmI == Main.myPlayer)
			{
				// If the prince is invalid, we need to spawn a new prince
				if (prince is null || !prince.active || prince.type != ProjectileType<SlimePrinceMinion>() || prince.owner != player.whoAmI)
					prince = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Vector2.Zero, ProjectileType<SlimePrinceMinion>(), 17, 0, player.whoAmI);
			}
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
				helm?.LocateMinion(self);

				if (helm.Minion != null)
				{
					if (!helm.Minion.Merged && helm.Minion.life >= SlimePrinceMinion.MAX_LIFE)
					{
						targetVel *= 0;
						targetAccel *= 0;
						helm.Minion.State = 2;
						helm.Minion.Timer = 0;
					}
					else if (helm.Minion.Merged)
					{
						helm.Minion.State = 0;
						helm.Minion.Timer = 0;
					}

					NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, helm.Minion.Projectile.whoAmI);
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
			helm?.LocateMinion(player);

			// Custom input handling
			if (helm?.Minion?.Merged ?? false)
			{
				helm.targetVel *= 0.98f;
				helm.targetAccel *= 0.83f;

				accel = 0.06f;

				if (player.controlLeft)
					helm.targetAccel.X -= accel;

				if (player.controlRight)
					helm.targetAccel.X += accel;

				if (player.controlUp)
					helm.targetAccel.Y -= accel;

				if (player.controlDown)
					helm.targetAccel.Y += accel;

				helm.targetVel += helm.targetAccel;

				if (helm.targetVel.Length() > 10)
					helm.targetVel = Vector2.Normalize(helm.targetVel) * 9.99f;

				if (helm.targetAccel.Length() > 1)
					helm.targetAccel = Vector2.Normalize(helm.targetAccel) * 0.99f;

				player.velocity = helm.targetVel;
				player.fallStart = (int)(player.position.Y / 16f);
			}
		}

		/// <summary>
		/// Handles the prince abosrbing damage instead of damaging the player
		/// while they are fused together
		/// </summary>
		/// <param name="player"></param>
		/// <param name="info"></param>
		/// <returns></returns>
		private bool AbsorbDamage(Player player, Player.HurtInfo info)
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

		/// <summary>
		/// This method attempts to relocate the prince if they are lost, or if
		/// they have not been detected yet in a multiplayer scenario
		/// </summary>
		/// <param name="player">The player looking for their prince</param>
		private void LocateMinion(Player player)
		{
			// No need to search, we already have him!
			if (prince != null && prince.active)
				return;

			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.type == ProjectileType<SlimePrinceMinion>() && proj.owner == player.whoAmI)
				{
					prince = proj;
					return;
				}
			}
		}
	}

	[AutoloadEquip(new EquipType[] { EquipType.Body, EquipType.Front })]
	public class SlimePrinceChest : ModItem
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Curiass");
			Tooltip.SetDefault("5% increased summoning damage\nYou can summon an additional minion");

			ArmorIDs.Body.Sets.IncludedCapeFront[Item.bodySlot] = Item.frontSlot;
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
			Tooltip.SetDefault("Minions inflict 5% {{exposure}}");
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

	public class SlimePrinceDrops : GlobalNPC
	{
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
		{
			return entity.type == NPCID.KingSlime;
		}

		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
		{
			var normalMode = new LeadingConditionRule(new Conditions.NotExpert());

			normalMode.OnSuccess(ItemDropRule.FewFromOptions(3, 2, new int[] { ItemType<SlimePrinceHead>(), ItemType<SlimePrinceChest>(), ItemType<SlimePrinceLegs>() }));

			npcLoot.Add(normalMode);
		}
	}

	public class SlimePrinceBagDrops : GlobalItem
	{
		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return entity.type == ItemID.KingSlimeBossBag;
		}

		public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
		{
			itemLoot.Add(ItemDropRule.FewFromOptions(3, 2, new int[] { ItemType<SlimePrinceHead>(), ItemType<SlimePrinceChest>(), ItemType<SlimePrinceLegs>() }));
		}
	}
}