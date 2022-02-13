using StarlightRiver.Content.Abilities.ForbiddenWinds;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

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
        }
        internal void Unload()
        {
            bindings.Clear();
        }
    }
}
