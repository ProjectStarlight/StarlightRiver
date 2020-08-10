using NetEasy;
using StarlightRiver.Abilities;
using System;
using System.Linq;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class UseAbility : Module
    {
        public UseAbility(int fromWho, Ability ability)
        {
            this.fromWho = fromWho;
            (abActive, abTimer) = (ability.Active, ability.Timer);
            abType = ability.GetType();
        }

        private readonly int fromWho;
        private readonly bool abActive;
        private readonly int abTimer;
        private readonly Type abType;

        protected override void Receive()
        {
            AbilityHandler mp = Main.player[fromWho].GetModPlayer<AbilityHandler>();
            Ability ab = mp.Abilities.Single(a => a.GetType() == abType);

            ab.OnCast();
            (ab.Active, ab.Timer) = (abActive, abTimer);

            if (Main.netMode == Terraria.ID.NetmodeID.Server) ab.SendPacket(-1, fromWho);
        }
    }
}