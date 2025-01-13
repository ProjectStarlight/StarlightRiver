using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoughDrops : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CoughDrops() : base("Cough Drops", "When debuffs wear off, gain {{BUFF:CoughDropsBuff}}") { }

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
			int buffType = self.buffType[buffId];
			if (Main.debuff[buffType] && !Main.buffNoTimeDisplay[buffType] && !BuffID.Sets.NurseCannotRemoveDebuff[buffType] && Equipped(self))
				self.AddBuff(ModContent.BuffType<CoughDropsBuff>(), 180);

			orig(self, buffId);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(silver: 50);
		}
	}
}