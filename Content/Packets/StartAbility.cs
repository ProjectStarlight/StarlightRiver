/*using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class StartAbility : Module
    {
        public StartAbility(int fromWho, Ability ability)
        {
            this.fromWho = fromWho;
            abTypeName = ability.GetType().FullName;
        }

        private readonly int fromWho;
        private readonly string abTypeName;

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                Send(-1, fromWho, true);
                return;
            }

            Player player = Main.player[fromWho];
            AbilityHandler handler = player.GetHandler();

            Type abType = Type.GetType(abTypeName);
        }
    }
}*/