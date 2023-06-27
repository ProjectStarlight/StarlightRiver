using StarlightRiver.Core.Systems.ExposureSystem;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Forest
{
	[AutoloadEquip(EquipType.Head)]
	public class SlimePrinceHead : ModItem
	{
		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Slime Prince's Crown");
			Tooltip.SetDefault("10% increased summoning damage");
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
