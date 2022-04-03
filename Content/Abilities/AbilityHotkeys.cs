using StarlightRiver.Content.Abilities.Faeflame;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Content.Abilities.GaiasFist;
using StarlightRiver.Content.Abilities.Purify;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Abilities
{
	public class AbilityHotkeys
    {
        public AbilityHotkeys(Mod Mod)
        {
            this.Mod = Mod;
        }

        private readonly Dictionary<Type, ModKeybind> bindings = new Dictionary<Type, ModKeybind>();
        private readonly Mod Mod;

        private ModKeybind this[Type type]
        {
            get
            {
                if (type == typeof(object) || type == typeof(Ability))
                    throw new InvalidOperationException("Not a registered ability binding. This should never happen! Contact Mod devs to implement a missing key binding for the ability.");
                if (bindings.TryGetValue(type, out ModKeybind ret))
                    return ret;
                return bindings[type] = this[type.BaseType];
            }
        }

        public ModKeybind Get<T>() where T : Ability => this[typeof(T)];

        public void Bind<T>(string display, string defaultKey) where T : Ability
        {
            bindings[typeof(T)] = KeybindLoader.RegisterKeybind(Mod, display, defaultKey);
        }

        internal void LoadDefaults()
        {
            Bind<Dash>("Forbidden Winds", "LeftShift");
            Bind<Whip>("Faeflame", "F");
            Bind<Pure>("Purity Crown", "N");
            Bind<Smash>("Gaia's Fist", "Z");
        }
        internal void Unload()
        {
            bindings.Clear();
        }
    }
}
