using StarlightRiver.Content.Abilities;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Buffs
{
	class HotspringHeal : ModBuff
	{
		public override string Texture => AssetDirectory.Buffs + "HotspringHeal";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Refreshing Dip");
			Description.SetDefault("The hot springs restore your body and mind");
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (!Main.npc.Any(n => n.active && n.boss))
			{
				Player.lifeRegen += 50;
				Player.manaRegen += 50;
				Player.GetHandler().StaminaRegenRate += 1;

				for (int k = 0; k < Player.MaxBuffs; k++)
				{
					if (!BuffID.Sets.NurseCannotRemoveDebuff[Player.buffType[k]] && Main.debuff[Player.buffType[k]])
						Player.DelBuff(k);
				}
			}
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			if (Main.npc.Any(n => n.active && n.boss))
				tip = "An evil presence prevents you from relaxing in the hot springs!";
		}
	}
}