using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleLensUpgrade : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleLensUpgrade() : base("Truestrike Lens", "Critical strikes expose enemies near the struck enemy\nExposed enemies take significantly more damage on the first hit\n+4% critical strike chance\n+10% critical strike damage") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Orange;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Generic) += 4;

			Player.GetModPlayer<CritMultiPlayer>().AllCritMult += 0.1f;
		}

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ModifyHurtLens;
			StarlightProjectile.ModifyHitNPCEvent += ModifyProjectileLens;
		}

		private void ModifyHurtLens(Player Player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (Equipped(Player) && crit)
				target.AddBuff(BuffType<Exposed>(), 120);
		}

		private void ModifyProjectileLens(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (Equipped(Main.player[Projectile.owner]) && crit)
				target.AddBuff(BuffType<Exposed>(), 120);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<TempleLens>());
			recipe.AddIngredient(ItemType<Moonstone.MoonstoneBarItem>(), 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class Exposed : SmartBuff
	{
		public override string Texture => AssetDirectory.Debug;

		public Exposed() : base("Exposed", "How do you have this its NPC only", true) { }

		public override void Load()
		{
			StarlightNPC.ModifyHitByItemEvent += ExtraDamage;
			StarlightNPC.ModifyHitByProjectileEvent += ExtraDamageProjectile;
		}

		private void ExtraDamageProjectile(NPC NPC, Projectile Projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (Inflicted(NPC) && !crit)
			{
				damage = (int)(damage * 1.2f);
				NPC.DelBuff(NPC.FindBuffIndex(Type));
			}
		}

		private void ExtraDamage(NPC NPC, Player Player, Item Item, ref int damage, ref float knockback, ref bool crit)
		{
			if (Inflicted(NPC) && !crit)
			{
				damage = (int)(damage * 1.2f);
				NPC.DelBuff(NPC.FindBuffIndex(Type));
			}
		}

		public override void Update(NPC NPC, ref int buffIndex)
		{
			//spawn dust and do a spelunker-esque glow maybe?
		}
	}
}
