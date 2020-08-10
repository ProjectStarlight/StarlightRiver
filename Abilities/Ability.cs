using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dragons;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace StarlightRiver.Abilities
{
    public class Ability
    {
        public int StaminaCost;
        public bool Active;
        public int Timer;
        public int Cooldown;
        public bool Locked = true;
        public virtual Texture2D Texture { get; }
        public Player player;
        public virtual bool CanUse => true;

        public Ability(int staminaCost, Player Player)
        {
            StaminaCost = staminaCost;
            player = Player;
        }

        public virtual void StartAbility(Player player)
        {
            AbilityHandler handler = player.GetModPlayer<AbilityHandler>();
            DragonHandler dragon = player.GetModPlayer<DragonHandler>();
            //if the player: has enough stamina  && unlocked && not on CD     && Has no other abilities active
            if (CanUse && handler.StatStamina >= StaminaCost && !Locked && Cooldown == 0 && !handler.Abilities.Any(a => a.Active))
            {
                handler.StatStamina -= StaminaCost; //Consume the stamina
                                                    //if (dragon.DragonMounted) OnCastDragon(); //Do what the ability should do when it starts
                                                    /*else*/
                OnCast();
                Active = true; //Ability is activated

                if (Main.netMode == NetmodeID.MultiplayerClient)
                    SendPacket();
            }
        }

        public virtual void OnCast()
        {
        }

        public virtual void OnCastDragon()
        {
        }

        public virtual void InUse()
        {
        }

        public virtual void InUseDragon()
        {
        }

        public virtual void UseEffects()
        {
        }

        public virtual void UseEffectsDragon()
        {
        }

        public virtual void OffCooldownEffects()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void SendPacket(int toWho = -1, int fromWho = -1)
        {
            new Packets.UseAbility(Main.myPlayer, this).Send(toWho, fromWho);
        }
    }
}