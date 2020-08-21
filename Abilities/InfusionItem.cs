using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Abilities
{
    public abstract partial class InfusionItem : ModItem
    {
        public virtual Type AbilityType { get; }
        public Player Player { get; internal set; } // TODO sync this somehow
        public Ability Ability
        {
            get
            {
                if (AbilityType == null) return null;
                if (ability == null) Player.GetHandler().GetAbility(AbilityType, out ability);
                return ability;
            }
        }
        private Ability ability;

        public virtual void OnActivate() => Ability?.OnActivate();
        public virtual void UpdateActive() => Ability?.UpdateActive();
        public virtual void UpdateActiveEffects() => Ability?.UpdateActiveEffects();
        public virtual void UpdateFixed() => Ability?.UpdateFixed();
        public virtual void OnExit() => Ability?.OnExit();
    }

    public abstract class InfusionItem<T> : InfusionItem where T : Ability
    {
        public override Type AbilityType => typeof(T);
        public new T Ability => (T)base.Ability;
    }
}
