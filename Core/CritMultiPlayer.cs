using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Core
{
	class CritMultiPlayer : ModPlayer
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

				if (Projectile.melee) toMult += mp.MeleeCritMult;
				if (Projectile.ranged) toMult += mp.RangedCritMult;
				if (Projectile.magic) toMult += mp.MagicCritMult;

				float multiplier = 1 + toMult / 2;
				damage = (int)(damage * multiplier);
			}
		}

		public override void ModifyHitNPC(Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if(crit)
			{
				float toMult = AllCritMult;

				if (Item.melee) toMult += MeleeCritMult;
				if (Item.ranged) toMult += RangedCritMult;
				if (Item.magic) toMult += MagicCritMult;

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

			if (Projectile.melee) multiplier += mp.MeleeCritMult;
			if (Projectile.ranged) multiplier += mp.RangedCritMult;
			if (Projectile.magic) multiplier += mp.MagicCritMult;

			return multiplier;
		}

		public static float GetMultiplier(Item Item)
		{
			var Player = Main.player[Item.owner];
			var mp = Player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (Item.melee) multiplier += mp.MeleeCritMult;
			if (Item.ranged) multiplier += mp.RangedCritMult;
			if (Item.magic) multiplier += mp.MagicCritMult;

			return multiplier;
		}
	}
}
