using StarlightRiver.Content.Buffs;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.Items.Misc
{
    public class CoughDrops : SmartAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public CoughDrops() : base("Cough Drops", "When debuffs wear off, gain a temporary speed and damage boost") { }

        public override bool Autoload(ref string name)
        {
            On.Terraria.Player.DelBuff += DelBuff;

            return true;
        }

        private void DelBuff(On.Terraria.Player.orig_DelBuff orig, Player self, int buffId)
        {
            if (Helper.IsValidDebuff(self, buffId) && Equipped(self))
            {
                self.AddBuff(ModContent.BuffType<CoughDropsBuff>(), 180);
            }

            orig(self, buffId);
        }
    }
}