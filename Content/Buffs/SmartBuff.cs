using Terraria;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public abstract class SmartBuff : ModBuff
    {
        private readonly string ThisName;
        private readonly string ThisTooltip;
        private readonly bool Debuff;
        private readonly bool Summon;

        public bool Inflicted(Player player) => player.active && player.HasBuff(Type);
        public bool Inflicted(NPC npc)
        {
            if (npc.buffTime.Length >= Type)
                return npc.active && npc.HasBuff(Type);
            return false;
        }

        public override bool Autoload(ref string name, ref string texture)
        {
            var path = AssetDirectory.Buffs + name;
            texture = ModContent.TextureExists(path) ? path : AssetDirectory.Debug;
            return true;
        }

        public virtual void SafeSetDetafults() { }
        protected SmartBuff(string name, string tooltip, bool debuff, bool summon = false)
        {
            ThisName = name;
            ThisTooltip = tooltip;
            Debuff = debuff;
            Summon = summon;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault(ThisName);
            Description.SetDefault(ThisTooltip);
            Main.debuff[Type] = Debuff;
            if (Summon)
            {
                Main.buffNoSave[Type] = true;
                Main.buffNoTimeDisplay[Type] = true;
            }
        }
    }
}
