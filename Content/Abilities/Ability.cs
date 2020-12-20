using Microsoft.Xna.Framework;
using StarlightRiver.Content.Abilities.GaiasFist;
using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Purify;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Abilities
{
    public abstract class Ability
    {
        protected Ability()
        {
            Reset();
        }

        public AbilityHandler User { get; internal set; }
        public Player Player => User.player;
        public float ActivationCostBonus { get; set; }
        public bool Active => ReferenceEquals(User.ActiveAbility, this);

        public abstract string Texture { get; }
        public virtual float ActivationCostDefault { get; }
        public virtual string TextureLocked => Texture + "Locked";
        public virtual bool Available => User.ActiveAbility == null && User.Stamina >= ActivationCost(User);
        public virtual Color Color => Color.White;

        public abstract bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys);
        public virtual void ModifyDrawLayers(List<PlayerLayer> layers) { }
        public virtual void Reset() { }
        public virtual void UpdateActiveEffects() { }
        public virtual void UpdateFixed() { }

        public virtual void OnActivate() { }
        public virtual void UpdateActive() { }
        public virtual void OnExit() { }

        public float ActivationCost(AbilityHandler handler) => (ActivationCostDefault + ActivationCostBonus) * handler.StaminaCostMultiplier + handler.StaminaCostBonus;
        public void Activate(AbilityHandler user)
        {
            user.ActiveAbility = this;
        }
        public void Deactivate()
        {
            User.ActiveAbility = null;
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