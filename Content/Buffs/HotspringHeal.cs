using StarlightRiver.Content.Abilities;
using System.Linq;

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
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			if (!Main.npc.Any(n => n.active && n.boss))
			{
				Player.lifeRegen += 50;
				Player.manaRegen += 50;
				Player.GetHandler().StaminaRegenRate += 1;
			}
		}

		public override void ModifyBuffTip(ref string tip, ref int rare)
		{
			if (Main.npc.Any(n => n.active && n.boss))
				tip = "An evil presence prevents you from relaxing in the hot springs!";
		}
	}
}
