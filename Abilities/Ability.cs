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
            new Packets.StartAbility(player.whoAmI, this).Send(-1, player.whoAmI, true);
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
    }
}