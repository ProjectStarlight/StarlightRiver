﻿using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoughDrops : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CoughDrops() : base("Cough Drops", "When debuffs wear off, gain a temporary speed and damage boost") { }

		public override void Load()
		{
			On_Player.DelBuff += DelBuff;
		}

		public override void Unload()
		{
			On_Player.DelBuff -= DelBuff;
		}

		private void DelBuff(On_Player.orig_DelBuff orig, Player self, int buffId)
		{
			if (Main.debuff[self.buffType[buffId]] && Equipped(self))
				self.AddBuff(ModContent.BuffType<CoughDropsBuff>(), 180);

			orig(self, buffId);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 50);
		}
	}
}