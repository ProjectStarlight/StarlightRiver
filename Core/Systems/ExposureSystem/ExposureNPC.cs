namespace StarlightRiver.Core.Systems.ExposureSystem
{
	public class ExposureNPC : GlobalNPC
	{
		//Maybe make this use a dictionary system in the future for some compatibility etc
		public int ExposureAddAll;
		public int ExposureAddMelee;
		public int ExposureAddRanged;
		public int ExposureAddMagic;
		public int ExposureAddSummon; //literally just summon tag without the requirement of being the players minion target

		public float ExposureMultAll;
		public float ExposureMultMelee;
		public float ExposureMultRanged;
		public float ExposureMultMagic;
		public float ExposureMultSummon;

		public override bool InstancePerEntity => true;

		public override void ResetEffects(NPC npc)
		{
			ExposureAddAll = 0;
			ExposureAddMelee = 0;
			ExposureAddRanged = 0;
			ExposureAddMagic = 0;
			ExposureAddSummon = 0;

			ExposureMultAll = 0f;
			ExposureMultMelee = 0f;
			ExposureMultRanged = 0f;
			ExposureMultMagic = 0f;
			ExposureMultSummon = 0f;
		}

		public override void ModifyIncomingHit(NPC npc, ref NPC.HitModifiers modifiers)
		{
			if (modifiers.DamageType.CountsAsClass(DamageClass.Melee))
			{
				modifiers.FinalDamage *= ExposureMultMelee;
				modifiers.FinalDamage += ExposureAddMelee;
			}

			if (modifiers.DamageType.CountsAsClass(DamageClass.Ranged))
			{
				modifiers.FinalDamage *= ExposureMultRanged;
				modifiers.FinalDamage += ExposureAddRanged;
			}

			if (modifiers.DamageType.CountsAsClass(DamageClass.Magic))
			{
				modifiers.FinalDamage *= ExposureMultMagic;
				modifiers.FinalDamage += ExposureAddMagic;
			}

			if (modifiers.DamageType.CountsAsClass(DamageClass.Summon))
			{
				modifiers.FinalDamage *= ExposureMultSummon;
				modifiers.FinalDamage += ExposureAddSummon;
			}

			modifiers.FinalDamage *= ExposureMultAll;
			modifiers.FinalDamage += ExposureAddAll;
		}
	}
}