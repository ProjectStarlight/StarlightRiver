using NetEasy;
using StarlightRiver.Content.Abilities;
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
            abTypeName = ability.GetType().FullName; //TODO: this string wastes packet size and would be better if we give abilities unique ids so we can send an unsigned byte instead
        }

        protected override void Receive()
        {

            Player player = Main.player[fromWho];
            AbilityHandler handler = player.GetHandler();

            Type abType = Type.GetType(abTypeName);

            handler.ActiveAbility = handler.unlockedAbilities[abType];

            if (Main.netMode == Terraria.ID.NetmodeID.Server && fromWho != -1)
                Send(-1, fromWho, false);
        }
    }
}