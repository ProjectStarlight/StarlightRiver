using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleLens : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleLens() : base("Ancient Lens", "Critical strikes cause enemies around the struck enemy to glow, revealing other enemies\n+3% critical strike chance") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Generic) += 3;
		}

		public override void Load()
		{
			StarlightPlayer.ModifyHitNPCEvent += ModifyHurtLens;
			StarlightProjectile.ModifyHitNPCEvent += ModifyProjectileLens;
		}

		private void ModifyHurtLens(Player Player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if (Equipped(Player) && crit)
				target.AddBuff(BuffType<Buffs.Illuminant>(), 900);
		}

		private void ModifyProjectileLens(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (Equipped(Main.player[Projectile.owner]) && crit)
				target.AddBuff(BuffType<Buffs.Illuminant>(), 900);
		}
	}
}
