using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Misc
{
	public class CoughDrops : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public CoughDrops() : base("Cough Drops", "When debuffs wear off, gain a temporary speed and damage boost") { }

		public override void Load()
		{
			On.Terraria.Player.DelBuff += DelBuff;
		}

		public override void Unload()
		{
			On.Terraria.Player.DelBuff -= DelBuff;
		}

		private void DelBuff(On.Terraria.Player.orig_DelBuff orig, Player self, int buffId)
		{
			if (Main.debuff[self.buffType[buffId]] && Equipped(self))
				self.AddBuff(ModContent.BuffType<CoughDropsBuff>(), 180);

			orig(self, buffId);
		}
	}
}