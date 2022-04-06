using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class CritMultiPlayer : ModPlayer //PortTODO: Make compatible with new damage type hoo-hah
	{
		public float MeleeCritMult = 0;
		public float RangedCritMult = 0;
		public float MagicCritMult = 0;
		public float AllCritMult = 0;

		public override void Load()
		{
			StarlightProjectile.ModifyHitNPCEvent += AddCritToProjectiles;		
		}

		private void AddCritToProjectiles(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (crit)
			{
				var mp = Main.player[Projectile.owner].GetModPlayer<CritMultiPlayer>();
				float toMult = mp.AllCritMult;

				if (Projectile.DamageType.CountsAs(DamageClass.Melee)) toMult += mp.MeleeCritMult;
				if (Projectile.DamageType.CountsAs(DamageClass.Ranged)) toMult += mp.RangedCritMult;
				if (Projectile.DamageType.CountsAs(DamageClass.Magic)) toMult += mp.MagicCritMult;

				float multiplier = 1 + toMult / 2;
				damage = (int)(damage * multiplier);
			}
		}

		public override void ModifyHitNPC(Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if(crit)
			{
				float toMult = AllCritMult;

				if (Item.DamageType.CountsAs(DamageClass.Melee)) toMult += MeleeCritMult;
				if (Item.DamageType.CountsAs(DamageClass.Ranged)) toMult += RangedCritMult;
				if (Item.DamageType.CountsAs(DamageClass.Magic)) toMult += MagicCritMult;

				float multiplier = 1 + toMult / 2;
				damage = (int)(damage * multiplier);
			}
		}

		public override void ResetEffects()
		{
			MeleeCritMult = 0;
			RangedCritMult = 0;
			MagicCritMult = 0;
			AllCritMult = 0;
		}

		public static float GetMultiplier(Projectile Projectile)
		{
			var Player = Main.player[Projectile.owner];
			var mp = Player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (Projectile.DamageType.CountsAs(DamageClass.Melee)) multiplier += mp.MeleeCritMult;
			if (Projectile.DamageType.CountsAs(DamageClass.Ranged)) multiplier += mp.RangedCritMult;
			if (Projectile.DamageType.CountsAs(DamageClass.Magic)) multiplier += mp.MagicCritMult;

			return multiplier;
		}

		public static float GetMultiplier(Item Item)
		{
			var Player = Main.player[Item.playerIndexTheItemIsReservedFor];
			var mp = Player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (Item.DamageType.CountsAs(DamageClass.Melee)) multiplier += mp.MeleeCritMult;
			if (Item.DamageType.CountsAs(DamageClass.Ranged)) multiplier += mp.RangedCritMult;
			if (Item.DamageType.CountsAs(DamageClass.Magic)) multiplier += mp.MagicCritMult;

			return multiplier;
		}
	}
}
