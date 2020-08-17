using NetEasy;
using StarlightRiver.Abilities;
using StarlightRiver.Dragons;
using System;
using System.Linq;
using Terraria;

namespace StarlightRiver.Packets
{
    [Serializable]
    public class StartAbility : Module
    {
        public StartAbility(int fromWho, Ability ability)
        {
            this.fromWho = fromWho;
            abType = ability.GetType();
        }

        private readonly int fromWho;
        private readonly Type abType;

        protected override void Receive()
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
            {
                Send(-1, fromWho, true);
                return;
            }

            Player player = Main.player[fromWho];
            AbilityHandler handler = player.GetModPlayer<AbilityHandler>();
            DragonHandler dragon = player.GetModPlayer<DragonHandler>();
            Ability ab = handler.Abilities.Single(a => a.GetType() == abType);

            //if the player: has enough stamina  && unlocked && not on CD     && Has no other abilities active
            if (ab.CanUse && handler.StatStamina >= ab.StaminaCost && !ab.Locked && ab.Cooldown == 0 && !handler.Abilities.Any(a => a.Active))
            {
                handler.StatStamina -= ab.StaminaCost; //Consume the stamina
                                                    //if (dragon.DragonMounted) OnCastDragon(); //Do what the ability should do when it starts
                /*else*/
                ab.OnCast();
                ab.Active = true; //Ability is activated
            }
        }
    }
}