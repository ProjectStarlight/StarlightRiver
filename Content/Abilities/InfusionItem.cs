﻿using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Abilities
{
	public abstract partial class InfusionItem : ModItem
    {
        private Ability ability;
        public Color color;

        public abstract InfusionTier Tier { get; }
        public virtual Type AbilityType { get; }

        public Player Player => Main.player[Item.playerIndexTheItemIsReservedFor];

        public virtual bool Equippable => true;

        public Ability Ability
        {
            get
            {
                // If this ability doesn't have a type, don't try getting its Ability
                if (Main.gameMenu || AbilityType == null)
                    return null;
                // If it does, but ability is uninitialized, set ability to its type [if we have it unlocked]
                if (ability == null)
                    Player.GetHandler().GetAbility(AbilityType, out ability);
                // Ret.
                return ability;
            }
        }


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
