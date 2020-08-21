using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities.Content;
using StarlightRiver.Dragons;
using StarlightRiver.Dusts;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities
{
    public abstract class Ability
    {
        public abstract string Texture { get; }
        public virtual string TextureLocked => Texture + "Locked";
        public virtual float ActivationCost { get; }
        public virtual bool Available => User.ActiveAbility == null && User.Stamina >= ActivationCost;
        public virtual Color Color => Color.White;
        public AbilityHandler User { get; internal set; }
        public Player Player => User.player;
        public bool Active => ReferenceEquals(User.ActiveAbility, this);

        public abstract bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys);
        public virtual void ModifyDrawLayers(List<PlayerLayer> layers) { }
        public virtual void OnActivate() { }
        public virtual void UpdateActive() { }
        public virtual void UpdateActiveEffects() { }
        public virtual void UpdateFixed() { }
        public virtual void OnExit() { }

        public void Activate(AbilityHandler user)
        {
            if (!Active)
            {
                User = user;
                User.ActiveAbility = this;
                User.Stamina -= ActivationCost;
                OnActivate();
                // TODO probably sync idk
            }
        }
        public void Deactivate()
        {
            if (Active)
            {
                User.ActiveAbility = null;
                OnExit();
            }
        }

        // TODO load, cache, and unload. super ugly rn
        public static Ability[] GetAbilityInstances()
        {
            return new Ability[]
            {
                new Dash(), new Wisp(), new Pure(), new Smash()//, new Superdash()
            };
        }
    }
}