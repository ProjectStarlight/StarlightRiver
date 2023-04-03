using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.UndergroundTemple
{
	class TempleLens : SmartAccessory
	{
		public override string Texture => AssetDirectory.CaveTempleItem + Name;

		public TempleLens() : base("Ancient Lens", "Critical strikes cause enemies around the struck enemy to glow, revealing other enemies\n+3% critical strike chance \n+10% critical strike damage") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Generic) += 3;
			Player.GetModPlayer<CritMultiPlayer>().AllCritMult += 0.1f;
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += ModifyHurtLens;
			StarlightProjectile.OnHitNPCEvent += ModifyProjectileLens;
		}

		private void ModifyHurtLens(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player) && info.Crit)
			{
				target.AddBuff(BuffType<Buffs.Illuminant>(), 900);
			}
		}

		private void ModifyProjectileLens(Projectile Projectile, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Main.player[Projectile.owner]) && info.Crit)
			{
				target.AddBuff(BuffType<Buffs.Illuminant>(), 900);
			}
		}
	}
}