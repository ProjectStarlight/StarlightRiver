using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace StarlightRiver.Abilities
{
    public abstract class InfusionItem : ModItem
    {
        public abstract Type AbilityType { get; }
        public Ability Ability { get; internal set; }

        public virtual void UpdateActive() { }
        public virtual void UpdateFixed() { }
        public virtual void OnEnd() { }
    }

    public abstract class InfusionItem<T> : InfusionItem where T : Ability
    {
        public override Type AbilityType => typeof(T);
        public new T Ability => (T)base.Ability;
    }
}
