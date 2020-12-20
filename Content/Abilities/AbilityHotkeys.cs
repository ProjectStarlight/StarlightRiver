using System;
using System.Collections.Generic;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities.Purify;
using StarlightRiver.Content.Abilities.GaiasFist;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.Faeflame;

namespace StarlightRiver.Content.Abilities
{
    public class AbilityHotkeys
    {
        public AbilityHotkeys(Mod mod)
        {
            this.mod = mod;
        }

        private readonly Dictionary<Type, ModHotKey> bindings = new Dictionary<Type, ModHotKey>();
        private readonly Mod mod;

        private ModHotKey this[Type type]
        {
            get
            {
                if (type == typeof(object) || type == typeof(Ability))
                    throw new InvalidOperationException("Not a registered ability binding. This should never happen! Contact mod devs to implement a missing key binding for the ability.");
                if (bindings.TryGetValue(type, out ModHotKey ret))
                    return ret;
                return bindings[type] = this[type.BaseType];
            }
        }

        public ModHotKey Get<T>() where T : Ability => this[typeof(T)];

        public void Bind<T>(string display, string defaultKey) where T : Ability
        {
            bindings[typeof(T)] = mod.RegisterHotKey(display, defaultKey);
        }

        internal void LoadDefaults()
        {
            Bind<Dash>("Forbidden Winds", "LeftShift");
            Bind<Wisp>("Faeflame", "F");
            Bind<Pure>("Purity Crown", "N");
            Bind<Smash>("Gaia's Fist", "Z");
        }
        internal void Unload()
        {
            bindings.Clear();
        }
    }
}
