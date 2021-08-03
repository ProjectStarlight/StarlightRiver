using NetEasy;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class StartAbility : Module
    {
        private readonly int fromWho;
        private readonly string abTypeName;

        public StartAbility(int fromWho, Ability ability)
        {
            this.fromWho = fromWho;
            abTypeName = ability.GetType().FullName;
        }

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
            {
                Send(-1, fromWho, false);
                return;
            }

            Player player = Main.player[fromWho];
            AbilityHandler handler = player.GetHandler();

            Type abType = Type.GetType(abTypeName);

            handler.ActiveAbility = handler.unlockedAbilities[abType];
        }
    }
}