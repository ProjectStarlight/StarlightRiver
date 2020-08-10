using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Buffs
{
    public abstract class SmartBuff : ModBuff
    {
        private readonly string ThisName;
        private readonly string ThisTooltip;
        private readonly bool Debuff;

        public bool InflictedPlayer(Player player) => player.HasBuff(Type);
        public bool InflictedNPC(NPC npc) => npc.HasBuff(Type);

        public virtual void SafeSetDetafults() { }

        public SmartBuff(string name, string tooltip, bool debuff)
        {
            ThisName = name;
            ThisTooltip = tooltip;
            Debuff = debuff;
        }

        public override void SetDefaults()
        {
            DisplayName.SetDefault(ThisName);
            Description.SetDefault(ThisTooltip);
            Main.debuff[Type] = Debuff;
        }
    }
}
