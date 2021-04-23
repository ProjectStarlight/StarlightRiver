using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Buffs
{
    public class ForestTonic : SmartBuff
    {
        public ForestTonic() : base("Forest Tonic", "Immunity to poision\nSlowly regenerate life", false) { }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 2;
            if (player.poisoned) player.poisoned = false;
        }
    }
}