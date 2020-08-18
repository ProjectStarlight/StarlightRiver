using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Dragons;
using StarlightRiver.Dusts;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities
{
    public abstract class Ability
    {
        public abstract string Texture { get; }
        public virtual float StaminaCost { get; }
        public virtual bool Available => User.ActiveAbility == null && User.Stamina >= StaminaCost;
        public AbilityHandler User { get; internal set; }

        public virtual void ModifyDrawLayers(List<PlayerLayer> layers) { }
        public abstract bool HotkeyMatch(TriggersSet triggers);
        public virtual void OnCast() { }
        public virtual void UpdateActive() { }
        public virtual void Update() { }
        public virtual void OnExit() { }

        public void Activate()
        {
            User.ActiveAbility = this;
            OnCast();
            // TODO probably sync idk
        }
        public void Deactivate()
        {
            User.ActiveAbility = null;
            OnExit();
        }
    }
}