namespace StarlightRiver.Core
{
	class CritMultiPlayer : ModPlayer //TODO: Make compatible with new damage type hoo-hah
	{
		public float MeleeCritMult = 0;
		public float RangedCritMult = 0;
		public float MagicCritMult = 0;
		public float AllCritMult = 0;

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
		{
			float toMult = AllCritMult;

			if (proj.DamageType.Type == DamageClass.Melee.Type)
				toMult += MeleeCritMult;

			if (proj.DamageType.Type == DamageClass.Ranged.Type)
				toMult += RangedCritMult;

			if (proj.DamageType.Type == DamageClass.Magic.Type)
				toMult += MagicCritMult;

			float multiplier = 1 + toMult;
			modifiers.CritDamage += multiplier;
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Item, consider using ModifyHitNPC instead */
		{
			float toMult = AllCritMult;

			if (item.DamageType.Type == DamageClass.Melee.Type)
				toMult += MeleeCritMult;

			if (item.DamageType.Type == DamageClass.Ranged.Type)
				toMult += RangedCritMult;

			if (item.DamageType.Type == DamageClass.Magic.Type)
				toMult += MagicCritMult;

			float multiplier = 1 + toMult;
			modifiers.CritDamage += multiplier;
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
			Player Player = Main.player[Projectile.owner];
			CritMultiPlayer mp = Player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (Projectile.DamageType.Type == DamageClass.Melee.Type)
				multiplier += mp.MeleeCritMult;

			if (Projectile.DamageType.Type == DamageClass.Ranged.Type)
				multiplier += mp.RangedCritMult;

			if (Projectile.DamageType.Type == DamageClass.Magic.Type)
				multiplier += mp.MagicCritMult;

			return multiplier;
		}

		public static float GetMultiplier(Item Item)
		{
			Player Player = Main.player[Item.playerIndexTheItemIsReservedFor];
			CritMultiPlayer mp = Player.GetModPlayer<CritMultiPlayer>();

			float multiplier = mp.AllCritMult;

			if (Item.DamageType.Type == DamageClass.Melee.Type)
				multiplier += mp.MeleeCritMult;

			if (Item.DamageType.Type == DamageClass.Ranged.Type)
				multiplier += mp.RangedCritMult;

			if (Item.DamageType.Type == DamageClass.Magic.Type)
				multiplier += mp.MagicCritMult;

			return multiplier;
		}
	}
}