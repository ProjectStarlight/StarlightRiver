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

		public override bool Autoload(ref string name)
		{
			StarlightProjectile.ModifyHitNPCEvent += AddCritToProjectiles;
			return base.Autoload(ref name);
		}

		private void AddCritToProjectiles(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			if (crit)
			{
				var mp = Main.player[projectile.owner].GetModPlayer<CritMultiPlayer>();
				float toMult = mp.AllCritMult;

				if (projectile.melee) toMult += mp.MeleeCritMult;
				if (projectile.ranged) toMult += mp.RangedCritMult;
				if (projectile.magic) toMult += mp.MagicCritMult;

				float multiplier = 1 + toMult / 2;
				damage = (int)(damage * multiplier);
			}
		}

		public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			if(crit)
			{
				float toMult = AllCritMult;

				if (item.melee) toMult += MeleeCritMult;
				if (item.ranged) toMult += RangedCritMult;
				if (item.magic) toMult += MagicCritMult;

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

		public static float GetMultiplier(Projectile projectile)
		{
			var player = Main.player[projectile.owner];
			var mp = player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (projectile.melee) multiplier += mp.MeleeCritMult;
			if (projectile.ranged) multiplier += mp.RangedCritMult;
			if (projectile.magic) multiplier += mp.MagicCritMult;

			return multiplier;
		}

		public static float GetMultiplier(Item item)
		{
			var player = Main.player[item.owner];
			var mp = player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (item.melee) multiplier += mp.MeleeCritMult;
			if (item.ranged) multiplier += mp.RangedCritMult;
			if (item.magic) multiplier += mp.MagicCritMult;

			return multiplier;
		}
	}
}
